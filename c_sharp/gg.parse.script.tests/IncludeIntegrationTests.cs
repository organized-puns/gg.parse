using gg.parse.ebnf;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.script.tests
{
    /// <summary>
    /// See if include statements work as intended
    /// </summary>
    [TestClass]
    public class IncludeIntegrationTests
    {

        /// <summary>
        /// Trivial include in a tokenizer
        /// </summary>
        [TestMethod]
        public void CreateEbnfParser_FindRule_ExpectIncludedRulesToExist()
        {
            var includeCommand = "include 'assets/string.tokens'; /*dummy main rule */ main=.;";
            var parser = new ScriptParser().CreateFromDefinition(includeCommand);

            // should have loaded the string rule from the included file
            IsTrue(parser.Tokenizer.FindRule("string") != null);
        }

        /// <summary>
        /// Include in a tokenizer which includes a file in the same directory. The path should be correctly 
        /// resolved
        /// </summary>
        [TestMethod]
        public void CreateEbnfParserUsingAFileContainingAnInclude_FindRule_ExpectIncludedRulesToExist()
        {
            var includeCommand = "include 'assets/include_test.tokens'; /*dummy main rule */ grammar_root=.;";
            var parser = new ScriptParser().CreateFromDefinition(includeCommand);

            // should have loaded the string rule from the included file
            IsTrue(parser.Tokenizer.FindRule("string") != null);
        }

        /// <summary>
        /// See if the included token can actually be used
        /// </summary>
        [TestMethod]
        public void CreateEbnfParser_ParseCompiledRule_ExpectRuleToFindMatch()
        {
            var includeCommand = "include 'assets/string.tokens'; string_ref = string;";
            var parser = new ScriptParser().CreateFromDefinition(includeCommand);

            // should have loaded the string rule from the included file
            IsTrue(parser.Tokenizer.FindRule("string") != null);

            var stringRef = parser.Tokenizer.FindRule("string_ref");
            IsTrue(stringRef != null);

            var parseResult = stringRef.Parse("\"this is a string\"");
            IsTrue(parseResult.FoundMatch);
            IsTrue(parseResult.Annotations != null);
            IsTrue(parseResult.Annotations[0].RuleId == stringRef.Id);
        }

        /// <summary>
        /// See if the duplicate includes are ignored 
        /// (xxx however until we report warnings, this cannot be detected from the outside
        /// directly and we just have to see it doesn't throw an error because of duplicate 
        /// function being registers)
        /// </summary>
        [TestMethod]
        public void CreateEbnfParser_ParseCompiledRule_ExpectDuplicateIncludesToBeIgnored()
        {
            var includeCommand = "include 'assets/string.tokens'; include 'assets/string.tokens'; string_ref = string;";
            var parser = new ScriptParser().CreateFromDefinition(includeCommand);

            // should have one include message, despite two includes
            IsTrue(parser.LogHandler!.ReceivedLogs
                    .Where( log => log.message.Contains("including", StringComparison.CurrentCulture))
                    .Count() == 1);

            // should have loaded the string rule from the included file
            IsTrue(parser.Tokenizer.FindRule("string") != null);

            var stringRef = parser.Tokenizer.FindRule("string_ref");
            IsTrue(stringRef != null);

            var parseResult = stringRef.Parse("\"this is a string\"");
            IsTrue(parseResult.FoundMatch);
            IsTrue(parseResult.Annotations != null);
            IsTrue(parseResult.Annotations[0].RuleId == stringRef.Id);
        }

        /// <summary>
        /// Include a file which holds a circular dependency. This should cause an exception.
        /// </summary>
        /*[TestMethod]
        [ExpectedException(typeof(InvalidProgramException))]
        public void CreateEbnfParser_ParseCompiledRule_ExpectExceptionBecauseOfCircularDependencies()
        {
            var includeCommand = "include 'assets/include_circular_1.ebnf';";
            // this should throw and exception
            new ScriptPipeline(includeCommand, null);
        }

        /// <summary>
        /// Test if include files also work with parsers
        /// </summary>
        [TestMethod]
        public void CreateEbnfParserIncludeJsonGrammar_ParseGrammar_ExpectJsonGrammarIncluded()
        {
            var jsonParser = new ScriptPipeline(File.ReadAllText("assets/json_tokens.ebnf"), 
                                            "include 'assets/json_grammar.ebnf';#main=json;");

            IsTrue(jsonParser.EbnfTokenizer != null);
            IsTrue(jsonParser.EbnfTokenizer.Root != null);
            IsTrue(jsonParser.EbnfGrammarParser != null);
            IsTrue(jsonParser.EbnfGrammarParser.Root != null);

            // spot check to see if object is in the grammar rule graph
            IsTrue(jsonParser.EbnfGrammarParser.FindRule("object") != null);

            // check if it compiles json
            var result = jsonParser.Parse("{ \"key\": 123 }");

            IsTrue(result.FoundMatch);
            IsTrue(result.Annotations[0].Children[0].RuleId == jsonParser.FindParserRule("object").Id);
            IsTrue(result.Annotations[0].Children[0].Children[0].RuleId == jsonParser.FindParserRule("key_value_pair").Id);
        }

        /// <summary>
        /// Test if include files also work with tokens AND parsers 
        /// </summary>
        [TestMethod]
        public void CreateEbnfParserIncludeJsonTokensAndGrammar_ParseGrammar_ExpectJsonGrammarIncluded()
        {
            var jsonParser = new ScriptPipeline("include 'assets/json_tokens.ebnf';#token_main = json_tokens;",
                                            "include 'assets/json_grammar.ebnf'; # main = json;");

            IsTrue(jsonParser.EbnfTokenizer != null);
            IsTrue(jsonParser.EbnfTokenizer.Root != null);
            IsTrue(jsonParser.EbnfGrammarParser != null);
            IsTrue(jsonParser.EbnfGrammarParser.Root != null);

            // spot check to see if object is in the grammar rule graph
            IsTrue(jsonParser.EbnfGrammarParser.FindRule("object") != null);

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

            var tokenIds = tokens.Annotations.Select(t => t.RuleId).ToArray();

            IsTrue(tokenIds.SequenceEqual(expectedTokens));

            var result = jsonParser.Parse("{ \"key\": 123 }");

            IsTrue(result.FoundMatch);
            IsTrue(result.Annotations[0].Children[0].RuleId == jsonParser.FindParserRule("object").Id);
            IsTrue(result.Annotations[0].Children[0].Children[0].RuleId == jsonParser.FindParserRule("key_value_pair").Id);
        }*/
    }
}
