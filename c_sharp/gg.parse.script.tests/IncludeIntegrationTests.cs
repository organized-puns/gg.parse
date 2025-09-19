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
        /// // xxx should be ignored and downgraded to a warning
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ScriptPipelineException))]
        public void CreateEbnfParser_ParseCompiledRule_ExpectExceptionBecauseOfCircularDependencies()
        {
            var includeCommand = "include 'assets/include_circular_1.tokens';";
            // this should throw and exception
            new ScriptParser().CreateFromDefinition(includeCommand);
        }

        
        /// <summary>
        /// Test if include files also work with parsers
        /// </summary>
        [TestMethod]
        public void CreateEbnfParserIncludeJsonGrammar_ParseGrammar_ExpectJsonGrammarIncluded()
        {
            var jsonParser = new ScriptParser()
                                .CreateFromDefinition(
                                    File.ReadAllText("assets/json.tokens"), 
                                    "include 'assets/json.grammar';#main=json;"
                                );

            IsTrue(jsonParser.Tokenizer != null);
            IsTrue(jsonParser.Tokenizer.Root != null);
            // spot check to see if this rule is in the token rule graph
            IsTrue(jsonParser.Tokenizer.FindRule("string") != null);

            IsTrue(jsonParser.Parser != null);
            IsTrue(jsonParser.Parser.Root != null);

            // spot check to see if these rules are in the grammar rule graph
            IsTrue(jsonParser.Parser.FindRule("string") != null);
            IsTrue(jsonParser.Parser.FindRule("object") != null);

            // check if it compiles json
            var result = jsonParser.Parse("{ \"key\": 123 }");

            IsTrue(result.FoundMatch);
            IsTrue(result.Annotations![0].Children![0].RuleId == jsonParser.Parser!.FindRule("object")!.Id);
            IsTrue(result.Annotations![0].Children![0].Children![0].RuleId == jsonParser.Parser!.FindRule("key_value_pair")!.Id);
        }

        
        /// <summary>
        /// Test if include files also work with tokens AND parsers 
        /// </summary>
        [TestMethod]
        public void CreateEbnfParserIncludeJsonTokensAndGrammar_ParseGrammar_ExpectJsonGrammarIncluded()
        {
            var jsonParser = new ScriptParser()
                                .CreateFromDefinition(
                                    "include 'assets/json.tokens';#token_main = json_tokens;",
                                    "include 'assets/json.grammar'; # main = json;"
                                );

            IsTrue(jsonParser.Tokenizer != null);
            IsTrue(jsonParser.Tokenizer.Root != null);
            IsTrue(jsonParser.Parser != null);
            IsTrue(jsonParser.Parser.Root != null);

            // spot check to see if object is in the grammar rule graph
            IsTrue(jsonParser.Parser.FindRule("object") != null);

            // check if it compiles json
            var result = jsonParser.Parse("{ \"key\": 123 }");

            IsTrue(result.FoundMatch);
            IsTrue(result.Annotations![0].Children![0].RuleId == jsonParser.Parser.FindRule("object")!.Id);
            IsTrue(result.Annotations![0].Children![0].Children![0].RuleId == jsonParser.Parser.FindRule("key_value_pair")!.Id);
        }
    }
}
