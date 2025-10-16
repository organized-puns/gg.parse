using System.Diagnostics;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

using gg.parse.rules;
using gg.parse.script.pipeline;


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
            var parser = new ParserBuilder().From(includeCommand);

            // should have loaded the string rule from the included file
            IsTrue(parser.TokenGraph.FindRule("string") != null);
        }

        /// <summary>
        /// Include in a tokenizer which includes a file in the same directory. The path should be correctly 
        /// resolved
        /// </summary>
        [TestMethod]
        public void CreateEbnfParserUsingAFileContainingAnInclude_FindRule_ExpectIncludedRulesToExist()
        {
            var includeCommand = "include 'assets/include_test.tokens'; /*dummy main rule */ grammar_root=.;";
            var parser = new ParserBuilder().From(includeCommand);

            // should have loaded the string rule from the included file
            IsTrue(parser.TokenGraph.FindRule("string") != null);
        }

        /// <summary>
        /// See if the included token can actually be used
        /// </summary>
        [TestMethod]
        public void CreateEbnfParser_ParseCompiledRule_ExpectRuleToFindMatch()
        {
            var includeCommand = "include 'assets/string.tokens'; string_ref = string;";
            var parser = new ParserBuilder().From(includeCommand);

            // should have loaded the string rule from the included file
            IsTrue(parser.TokenGraph.FindRule("string") != null);

            var stringRef = parser.TokenGraph.FindRule("string_ref");
            IsTrue(stringRef != null);

            var parseResult = stringRef.Parse("\"this is a string\"");
            IsTrue(parseResult.FoundMatch);
            IsTrue(parseResult.Annotations != null);
            IsTrue(parseResult.Annotations[0].Rule == stringRef);
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
            var parser = new ParserBuilder().From(includeCommand);

            // should have one include message, despite two includes
            IsTrue(parser.LogHandler!.ReceivedLogs
                    .Where( log => log.message.Contains("including", StringComparison.CurrentCulture))
                    .Count() == 1);

            // should have loaded the string rule from the included file
            IsTrue(parser.TokenGraph.FindRule("string") != null);

            var stringRef = parser.TokenGraph.FindRule("string_ref");
            IsTrue(stringRef != null);

            var parseResult = stringRef.Parse("\"this is a string\"");
            IsTrue(parseResult.FoundMatch);
            IsTrue(parseResult.Annotations != null);
            IsTrue(parseResult.Annotations[0].Rule == stringRef);
        }

        /// <summary>
        /// Include a file which holds a circular dependency. This should cause an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ScriptPipelineException))]
        public void CreateEbnfParser_ParseCompiledRule_ExpectExceptionBecauseOfCircularDependencies()
        {
            var includeCommand = "include 'assets/include_circular_1.tokens';";
            // this should throw and exception
            new ParserBuilder().From(includeCommand);
        }

        
        /// <summary>
        /// Test if include files also work with parsers
        /// </summary>
        [TestMethod]
        public void CreateEbnfParserIncludeJsonGrammar_ParseGrammar_ExpectJsonGrammarIncluded()
        {
            var jsonParser = new ParserBuilder()
                                .From(
                                    File.ReadAllText("assets/json.tokens"), 
                                    "include 'assets/json.grammar';__main__=#json;"
                                );

            IsTrue(jsonParser.TokenGraph != null);
            IsTrue(jsonParser.TokenGraph.Root != null);
            // spot check to see if this rule is in the token rule graph
            IsTrue(jsonParser.TokenGraph.FindRule("string") != null);

            IsTrue(jsonParser.GrammarGraph != null);
            IsTrue(jsonParser.GrammarGraph.Root != null);

            // spot check to see if these rules are in the grammar rule graph
            IsTrue(jsonParser.GrammarGraph.FindRule("string") != null);
            IsTrue(jsonParser.GrammarGraph.FindRule("object") != null);

            // check if it compiles json
            var (tokens, syntaxTree) = jsonParser.Parse("{ \"key\": 123 }");

            IsTrue(syntaxTree.FoundMatch);

            var objectNode = syntaxTree[0][0];

            IsTrue(objectNode.Rule == jsonParser.GrammarGraph!.FindRule("object"));
            IsTrue(objectNode[0].Rule == jsonParser.GrammarGraph!.FindRule("key_value_pair"));
        }

        
        /// <summary>
        /// Test if include files also work with tokens AND parsers 
        /// </summary>
        [TestMethod]
        public void CreateEbnfParserIncludeJsonTokensAndGrammar_ParseGrammar_ExpectJsonGrammarIncluded()
        {
            var jsonParser = new ParserBuilder()
                                .From(
                                    "include 'assets/json.tokens';#token_main = json_tokens;",
                                    "include 'assets/json.grammar'; # main = json;"
                                );

            IsTrue(jsonParser.TokenGraph != null);
            IsTrue(jsonParser.TokenGraph.Root != null);
            IsTrue(jsonParser.GrammarGraph != null);
            IsTrue(jsonParser.GrammarGraph.Root != null);

            // spot check to see if object is in the grammar rule graph
            var objectRule = jsonParser.GrammarGraph.FindRule("object") as MatchRuleSequence<int>;
            
            IsNotNull(objectRule);
            IsTrue(objectRule.Rules.Count() == 3);

            var scopeStart = objectRule.Rules.ElementAt(0) as RuleReference<int>;

            IsNotNull(scopeStart);
            IsTrue(scopeStart.Output == RuleOutput.Void);

            // check if it compiles json
            var text = "{ \"key\": 123 }";
            var result = jsonParser.Parse(text);

            IsTrue(result.syntaxTree.FoundMatch);

            Debug.WriteLine(ScriptUtils.AstToString(text, result.tokens.Annotations, result.syntaxTree.Annotations));

            var objectAnnotation = result.syntaxTree[0][0];

            IsTrue(objectAnnotation.Rule == jsonParser.GrammarGraph.FindRule("object"));

            var keyValueRuleAnnotation = objectAnnotation[0];

            IsTrue(keyValueRuleAnnotation.Rule == jsonParser.GrammarGraph.FindRule("key_value_pair"));
        }
    }
}
