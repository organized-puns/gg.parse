using gg.parse.ebnf;
using gg.parse.rulefunctions;

namespace gg.parse.tests.ebnf
{
    /// <summary>
    /// See if include statements work as intended
    /// </summary>
    [TestClass]
    public class EbnfParserIncludesTests
    {
        /// <summary>
        /// See if the tokenizer / parser breaks the include command down correctly in tokens
        /// / astTree.
        /// </summary>
        [TestMethod]
        public void SetupTokenizerParser_ParseCompiledRule_ExpectTokensAndAstTreeToMatchExpectations()
        {
            var includeCommand = "include 'assets/string_tokenization.ebnf'; string_ref = string;";

            var tokenizer = new EbnfTokenizer();
            var tokenizerParser = new EbnfTokenParser(tokenizer);

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
            var tokenIds = tokenizerTokens.Select(t => t.FunctionId).ToArray();

            Assert.IsTrue(tokenIds.SequenceEqual(expectedIds));

            var tokenizerAstTree = tokenizerParser.Parse(tokenizerTokens).Annotations;

            // expect an 'include' and a 'rule'
            var expectedAstNodes = new int[] {
                tokenizerParser.Include.Id,
                tokenizerParser.MatchRule.Id
            };

            var astNodeIds = tokenizerAstTree.Select(t => t.FunctionId).ToArray();

            Assert.IsTrue(astNodeIds.SequenceEqual(expectedAstNodes));
        }

        /// <summary>
        /// Trivial include in a tokenizer
        /// </summary>
        [TestMethod]
        public void CreateEbnfParser_FindRule_ExpectIncludedRulesToExist()
        {
            var includeCommand = "include 'assets/string_tokenization.ebnf'; /*dummy main rule */ main=.;";
            var jsonParser = new EbnfParser(includeCommand, null);

            // should have loaded the string rule from the included file
            Assert.IsTrue(jsonParser.EbnfTokenizer.FindRule("string") != null);
        }

        /// <summary>
        /// Include in a tokenizer which includes a file in the same directory. The path should be correctly 
        /// resolved
        /// </summary>
        [TestMethod]
        public void CreateEbnfParserUsingAFileContainingAnInclude_FindRule_ExpectIncludedRulesToExist()
        {
            var includeCommand = "include 'assets/include_test.ebnf'; /*dummy main rule */ grammar_root=.;";
            var jsonParser = new EbnfParser(includeCommand, null);

            // should have loaded the string rule from the included file
            Assert.IsTrue(jsonParser.EbnfTokenizer.FindRule("string") != null);
        }

        /// <summary>
        /// See if the included token can actually be used
        /// </summary>
        [TestMethod]
        public void CreateEbnfParser_ParseCompiledRule_ExpectRuleToFindMatch()
        {
            var includeCommand = "include 'assets/string_tokenization.ebnf'; string_ref = string;";
            var jsonParser = new EbnfParser(includeCommand, null);

            // should have loaded the string rule from the included file
            Assert.IsTrue(jsonParser.EbnfTokenizer.FindRule("string") != null);

            var stringRef = jsonParser.EbnfTokenizer.FindRule("string_ref");
            Assert.IsTrue(stringRef != null);

            var parseResult = stringRef.Parse("\"this is a string\"".ToCharArray(), 0);
            Assert.IsTrue(parseResult.FoundMatch);
            Assert.IsTrue(parseResult.Annotations != null);
            Assert.IsTrue(parseResult.Annotations[0].FunctionId == stringRef.Id);
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
            var includeCommand = "include 'assets/string_tokenization.ebnf'; include 'assets/string_tokenization.ebnf'; string_ref = string;";
            var jsonParser = new EbnfParser(includeCommand, null);

            // should have loaded the string rule from the included file
            Assert.IsTrue(jsonParser.EbnfTokenizer.FindRule("string") != null);

            var stringRef = jsonParser.EbnfTokenizer.FindRule("string_ref");
            Assert.IsTrue(stringRef != null);

            var parseResult = stringRef.Parse("\"this is a string\"".ToCharArray(), 0);
            Assert.IsTrue(parseResult.FoundMatch);
            Assert.IsTrue(parseResult.Annotations != null);
            Assert.IsTrue(parseResult.Annotations[0].FunctionId == stringRef.Id);
        }

        /// <summary>
        /// Include a file which holds a circular dependency. This should cause an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidProgramException))]
        public void CreateEbnfParser_ParseCompiledRule_ExpectExceptionBecauseOfCircularDependencies()
        {
            var includeCommand = "include 'assets/include_circular_1.ebnf';";
            // this should throw and exception
            new EbnfParser(includeCommand, null);
        }

        /// <summary>
        /// Test if include files also work with parsers
        /// </summary>
        [TestMethod]
        public void CreateEbnfParserIncludeJsonGrammar_ParseGrammar_ExpectJsonGrammarIncluded()
        {
            var jsonParser = new EbnfParser(File.ReadAllText("assets/json_tokens.ebnf"), 
                                            "include 'assets/json_grammar.ebnf';#main=json;");

            Assert.IsTrue(jsonParser.EbnfTokenizer != null);
            Assert.IsTrue(jsonParser.EbnfTokenizer.Root != null);
            Assert.IsTrue(jsonParser.EbnfGrammarParser != null);
            Assert.IsTrue(jsonParser.EbnfGrammarParser.Root != null);

            // spot check to see if object is in the grammar rule graph
            Assert.IsTrue(jsonParser.EbnfGrammarParser.FindRule("object") != null);

            // check if it compiles json
            var result = jsonParser.Parse("{ \"key\": 123 }");

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.Annotations[0].Children[0].FunctionId == jsonParser.FindParserRule("object").Id);
            Assert.IsTrue(result.Annotations[0].Children[0].Children[0].FunctionId == jsonParser.FindParserRule("key_value_pair").Id);
        }

        /// <summary>
        /// Test if include files also work with tokens AND parsers 
        /// </summary>
        [TestMethod]
        public void CreateEbnfParserIncludeJsonTokensAndGrammar_ParseGrammar_ExpectJsonGrammarIncluded()
        {
            var jsonParser = new EbnfParser("include 'assets/json_tokens.ebnf';#token_main = json_tokens;",
                                            "include 'assets/json_grammar.ebnf'; # main = json;");

            Assert.IsTrue(jsonParser.EbnfTokenizer != null);
            Assert.IsTrue(jsonParser.EbnfTokenizer.Root != null);
            Assert.IsTrue(jsonParser.EbnfGrammarParser != null);
            Assert.IsTrue(jsonParser.EbnfGrammarParser.Root != null);

            // spot check to see if object is in the grammar rule graph
            Assert.IsTrue(jsonParser.EbnfGrammarParser.FindRule("object") != null);

            // check if it compiles json
            jsonParser.TryBuildAstTree("{ \"key\": 123 }", out var tokens, out var astTree);

            // test if the tokes came out as expected
            var expectedTokens = new int[]
            {
                jsonParser.EbnfTokenizer.FindRule("scope_start").Id,
                jsonParser.EbnfTokenizer.FindRule("string").Id,
                jsonParser.EbnfTokenizer.FindRule("kv_separator").Id,
                jsonParser.EbnfTokenizer.FindRule("int").Id,
                jsonParser.EbnfTokenizer.FindRule("scope_end").Id,
            };

            var tokenIds = tokens.Annotations.Select(t => t.FunctionId).ToArray();

            Assert.IsTrue(tokenIds.SequenceEqual(expectedTokens));

            var result = jsonParser.Parse("{ \"key\": 123 }");

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.Annotations[0].Children[0].FunctionId == jsonParser.FindParserRule("object").Id);
            Assert.IsTrue(result.Annotations[0].Children[0].Children[0].FunctionId == jsonParser.FindParserRule("key_value_pair").Id);
        }
    }
}
