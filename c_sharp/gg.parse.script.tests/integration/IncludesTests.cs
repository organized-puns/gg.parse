#nullable disable

using gg.parse.script.common;
using gg.parse.script.parser;
using gg.parse.script.pipeline;

namespace gg.parse.script.tests.integration
{
    /// <summary>
    /// See if include statements work as intended
    /// </summary>
    [TestClass]
    public class IncludesTests
    {
        /// <summary>
        /// See if the tokenizer / parser breaks the include command down correctly in tokens
        /// / astTree.
        /// </summary>
        [TestMethod]
        public void SetupTokenizerParser_ParseCompiledRule_ExpectTokensAndAstTreeToMatchExpectations()
        {
            var includeCommand = "include 'assets/string_tokenization.ebnf'; string_ref = string;";

            var tokenizer = new ScriptTokenizer();
            var tokenizerParser = new ScriptParser(tokenizer);

            var tokenizerTokens = tokenizer.Tokenize(includeCommand).Annotations;
            var expectedIds = new int[] {
                                tokenizer.FindRule(CommonTokenNames.Include).Id,
                                tokenizer.FindRule(CommonTokenNames.SingleQuotedString).Id,
                                tokenizer.FindRule(CommonTokenNames.EndStatement).Id,
                                tokenizer.FindRule(CommonTokenNames.Identifier).Id,
                                tokenizer.FindRule(CommonTokenNames.Assignment).Id,
                                tokenizer.FindRule(CommonTokenNames.Identifier).Id,
                                tokenizer.FindRule(CommonTokenNames.EndStatement).Id
            };
            var tokenIds = tokenizerTokens.Select(t => t.Rule.Id).ToArray();

            Assert.IsTrue(tokenIds.SequenceEqual(expectedIds));

            var tokenizerAstTree = tokenizerParser.Root!.Parse(tokenizerTokens).Annotations;

            // expect an 'include' and a 'rule'
            var expectedAstNodes = new int[] {
                tokenizerParser.Include.Id,
                tokenizerParser.MatchRule.Id
            };

            var astNodeIds = tokenizerAstTree.Select(t => t.Rule.Id).ToArray();

            Assert.IsTrue(astNodeIds.SequenceEqual(expectedAstNodes));
        }

        /// <summary>
        /// Trivial include in a tokenizer
        /// </summary>
        [TestMethod]
        public void CreateEbnfParser_FindRule_ExpectIncludedRulesToExist()
        {
            var includeCommand = "include 'assets/string.tokens'; /*dummy main rule */ main=.;";
            var jsonParser = new RuleGraphBuilder().From(includeCommand);

            // should have loaded the string rule from the included file
            Assert.IsTrue(jsonParser.Tokenizer.FindRule("string") != null);
        }

        /// <summary>
        /// Include in a tokenizer which includes a file in the same directory. The path should be correctly 
        /// resolved
        /// </summary>
        [TestMethod]
        public void CreateEbnfParserUsingAFileContainingAnInclude_FindRule_ExpectIncludedRulesToExist()
        {
            var includeCommand = "include 'assets/include_test.tokens'; /*dummy main rule */ grammar_root=.;";
            var jsonParser = new RuleGraphBuilder().From(includeCommand);

            // should have loaded the string rule from the included file
            Assert.IsTrue(jsonParser.Tokenizer.FindRule("string") != null);
        }

        /// <summary>
        /// See if the included token can actually be used
        /// </summary>
        [TestMethod]
        public void CreateEbnfParser_ParseCompiledRule_ExpectRuleToFindMatch()
        {
            var includeCommand = "include 'assets/string.tokens'; string_ref = string;";
            var jsonParser = new RuleGraphBuilder().From(includeCommand);

            // should have loaded the string rule from the included file
            Assert.IsTrue(jsonParser.Tokenizer.FindRule("string") != null);

            var stringRef = jsonParser.Tokenizer.FindRule("string_ref");
            Assert.IsTrue(stringRef != null);

            var parseResult = stringRef.Parse("\"this is a string\"".ToCharArray(), 0);
            Assert.IsTrue(parseResult.FoundMatch);
            Assert.IsTrue(parseResult.Annotations != null);
            Assert.IsTrue(parseResult.Annotations[0].Rule == stringRef);
        }

        /// <summary>
        /// See if the duplicate includes are ignored 
        /// (xxx however until we report warnings, this cannot be detected from the outside
        /// directly and we just have to see it doesn't throw an error because of duplicate 
        /// function being registeres)
        /// </summary>
        [TestMethod]
        public void CreateEbnfParser_ParseCompiledRule_ExpectDuplicateIncludesToBeIgnored()
        {
            var includeCommand = "include 'assets/string.tokens'; include 'assets/string.tokens'; string_ref = string;";
            var jsonParser = new RuleGraphBuilder().From(includeCommand);

            // should have loaded the string rule from the included file
            Assert.IsTrue(jsonParser.Tokenizer.FindRule("string") != null);

            var stringRef = jsonParser.Tokenizer.FindRule("string_ref");
            Assert.IsTrue(stringRef != null);

            var parseResult = stringRef.Parse("\"this is a string\"".ToCharArray(), 0);
            Assert.IsTrue(parseResult.FoundMatch);
            Assert.IsTrue(parseResult.Annotations != null);
            Assert.IsTrue(parseResult.Annotations[0].Rule == stringRef);
        }

        /// <summary>
        /// Include a file which holds a circular dependency. This should cause an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ScriptPipelineException))]
        public void CreateEbnfParser_ParseCompiledRule_ExpectExceptionBecauseOfCircularDependencies()
        {
            // this should throw and exception
            new RuleGraphBuilder().From("include 'assets/include_circular_1.tokens';");
        }

        /// <summary>
        /// Test if include files also work with parsers
        /// </summary>
        [TestMethod]
        public void CreateEbnfParserIncludeJsonGrammar_ParseGrammar_ExpectJsonGrammarIncluded()
        {
            var jsonParser = new RuleGraphBuilder()
                .From(
                    File.ReadAllText("assets/json.tokens"), 
                    "include 'assets/json.grammar';#main=json;"
            );

            Assert.IsTrue(jsonParser.Tokenizer != null);
            Assert.IsTrue(jsonParser.Tokenizer.Root != null);
            Assert.IsTrue(jsonParser.Parser != null);
            Assert.IsTrue(jsonParser.Parser.Root != null);

            // spot check to see if object is in the grammar rule graph
            Assert.IsTrue(jsonParser.Parser.FindRule("object") != null);

            // check if it compiles json
            var (_, result) = jsonParser.Parse("{ \"key\": 123 }");

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.Annotations[0].Children[0].Rule == jsonParser.Parser.FindRule("object"));
            Assert.IsTrue(result.Annotations[0].Children[0].Children[0].Rule == jsonParser.Parser.FindRule("key_value_pair"));
        }

        /// <summary>
        /// Test if include files also work with tokens AND parsers 
        /// </summary>
        [TestMethod]
        public void CreateEbnfParserIncludeJsonTokensAndGrammar_ParseGrammar_ExpectJsonGrammarIncluded()
        {
            var jsonParser = new RuleGraphBuilder()
                .From(
                    "include 'assets/json.tokens';#token_main = json_tokens;",
                    "include 'assets/json.grammar'; # main = json;"
                );

            Assert.IsTrue(jsonParser.Tokenizer != null);
            Assert.IsTrue(jsonParser.Tokenizer.Root != null);
            Assert.IsTrue(jsonParser.Parser != null);
            Assert.IsTrue(jsonParser.Parser.Root != null);

            // spot check to see if object is in the grammar rule graph
            Assert.IsTrue(jsonParser.Parser.FindRule("object") != null);

            // check if it compiles json
            var (tokens, ast) = jsonParser.Parse("{ \"key\": 123 }");

            // test if the tokes came out as expected
            var expectedTokens = new int[]
            {
                jsonParser.Tokenizer.FindRule("scope_start").Id,
                jsonParser.Tokenizer.FindRule("string").Id,
                jsonParser.Tokenizer.FindRule("kv_separator").Id,
                jsonParser.Tokenizer.FindRule("int").Id,
                jsonParser.Tokenizer.FindRule("scope_end").Id,
            };

            var tokenIds = tokens.Annotations.Select(t => t.Rule.Id).ToArray();

            Assert.IsTrue(tokenIds.SequenceEqual(expectedTokens));

            var (_, result) = jsonParser.Parse("{ \"key\": 123 }");

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.Annotations[0].Children[0].Rule == jsonParser.Parser.FindRule("object"));
            Assert.IsTrue(result.Annotations[0].Children[0].Children[0].Rule == jsonParser.Parser.FindRule("key_value_pair"));
        }
    }
}
