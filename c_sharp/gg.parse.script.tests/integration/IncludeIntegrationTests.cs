#nullable disable

using gg.parse.rulefunctions.rulefunctions;
using gg.parse.script.pipeline;
using System.Diagnostics;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.script.tests.integration
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
            var parser = new RuleGraphBuilder().InitializeFromDefinition(includeCommand);

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
            var parser = new RuleGraphBuilder().InitializeFromDefinition(includeCommand);

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
            var parser = new RuleGraphBuilder().InitializeFromDefinition(includeCommand);

            // should have loaded the string rule from the included file
            IsTrue(parser.Tokenizer.FindRule("string") != null);

            var stringRef = parser.Tokenizer.FindRule("string_ref");
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
            var parser = new RuleGraphBuilder().InitializeFromDefinition(includeCommand);

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
            IsTrue(parseResult.Annotations[0].Rule == stringRef);
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
            new RuleGraphBuilder().InitializeFromDefinition(includeCommand);
        }

        
        /// <summary>
        /// Test if include files also work with parsers
        /// </summary>
        [TestMethod]
        public void CreateEbnfParserIncludeJsonGrammar_ParseGrammar_ExpectJsonGrammarIncluded()
        {
            var jsonParser = new RuleGraphBuilder()
                                .InitializeFromDefinition(
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
            var (_, result) = jsonParser.Parse("{ \"key\": 123 }");

            IsTrue(result.FoundMatch);
            IsTrue(result.Annotations![0].Children![0].Rule == jsonParser.Parser!.FindRule("object"));
            IsTrue(result.Annotations![0].Children![0].Children![0].Rule == jsonParser.Parser!.FindRule("key_value_pair"));
        }

        
        /// <summary>
        /// Test if include files also work with tokens AND parsers 
        /// </summary>
        [TestMethod]
        public void CreateEbnfParserIncludeJsonTokensAndGrammar_ParseGrammar_ExpectJsonGrammarIncluded()
        {
            var jsonParser = new RuleGraphBuilder()
                                .InitializeFromDefinition(
                                    "include 'assets/json.tokens';#token_main = json_tokens;",
                                    "include 'assets/json.grammar'; # main = json;"
                                );

            IsTrue(jsonParser.Tokenizer != null);
            IsTrue(jsonParser.Tokenizer.Root != null);
            IsTrue(jsonParser.Parser != null);
            IsTrue(jsonParser.Parser.Root != null);

            // spot check to see if object is in the grammar rule graph
            var objectRule = jsonParser.Parser.FindRule("object") as MatchFunctionSequence<int>;
            
            IsNotNull(objectRule);
            IsTrue(objectRule.Rules.Count() == 3);

            var scopeStart = objectRule.Rules.ElementAt(0) as RuleReference<int>;

            IsNotNull(scopeStart);
            IsTrue(scopeStart.Production == AnnotationProduct.None);

            // check if it compiles json
            var text = "{ \"key\": 123 }";
            var result = jsonParser.Parse(text);

            IsTrue(result.astNodes.FoundMatch);

            Debug.WriteLine(ScriptUtils.AstToString(text, result.tokens.Annotations, result.astNodes.Annotations));

            var objectAnnotation = result.astNodes[0][0];

            IsTrue(objectAnnotation.Rule == jsonParser.Parser.FindRule("object"));

            var keyValueRuleAnnotation = objectAnnotation[0];

            IsTrue(keyValueRuleAnnotation.Rule == jsonParser.Parser.FindRule("key_value_pair"));
        }
    }
}
