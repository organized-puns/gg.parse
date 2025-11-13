// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Diagnostics;

using gg.parse.rules;
using gg.parse.script.pipeline;
using gg.parse.core;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;


namespace gg.parse.script.tests.parserbuilder
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
                    .Where( log => log.Message.Contains("including", StringComparison.CurrentCulture))
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
                                    "include 'assets/json.grammar';root =-r json;"
                                )
                                .Build();

            IsTrue(jsonParser.Tokens.Root != null);
            
            // spot check to see if this rule is in the token rule graph
            IsTrue(jsonParser.Tokens.TryFindRule("string", out var _));

            IsTrue(jsonParser.Grammar != null);
            IsTrue(jsonParser.Grammar.Root != null);

            // spot check to see if these rules are in the grammar rule graph
            IsTrue(jsonParser.Grammar.TryFindRule("string", out _));
            IsTrue(jsonParser.Grammar.TryFindRule("object", out _));

            // check if it compiles json
            var (_, syntaxTree) = jsonParser.Parse("{ \"key\": 123 }");

            IsTrue(syntaxTree.FoundMatch);

            var objectNode = syntaxTree[0][0];

            IsTrue(objectNode == "object");
            IsTrue(objectNode[0] == "key_value_pair");
        }
        
        /// <summary>
        /// Test if include files also work with tokens AND parsers 
        /// </summary>
        [TestMethod]
        public void CreateEbnfParserIncludeJsonTokensAndGrammar_ParseGrammar_ExpectJsonGrammarIncluded()
        {
            var jsonParser = new ParserBuilder()
                                .From(
                                    "include 'assets/json.tokens';-r token_main = json_tokens;",
                                    "include 'assets/json.grammar'; -r main = json;"
                                ).Build();

            IsTrue(jsonParser.Tokens.Root != null);
            IsTrue(jsonParser.Grammar!= null);
            IsTrue(jsonParser.Grammar.Root != null);

            // spot check to see if object is in the grammar rule graph
            var objectRule = jsonParser.Grammar["object"] as MatchRuleSequence<int>;
            
            IsNotNull(objectRule);
            IsTrue(objectRule.Rules.Count() == 3);

            var scopeStart = objectRule.Rules.ElementAt(0) as RuleReference<int>;

            IsNotNull(scopeStart);
            IsTrue(scopeStart.ReferencePrune == AnnotationPruning.All);

            // check if it compiles json
            var text = "{ \"key\": 123 }";
            var (tokens, syntaxTree) = jsonParser.Parse(text);

            IsTrue(syntaxTree.FoundMatch);

            Debug.WriteLine(ScriptUtils.PrettyPrintSyntaxTree(text, tokens.Annotations, syntaxTree.Annotations));

            var objectAnnotation = syntaxTree[0][0];

            IsTrue(objectAnnotation.Rule == jsonParser.Grammar["object"]);

            var keyValueRuleAnnotation = objectAnnotation[0];

            IsTrue(keyValueRuleAnnotation.Rule == jsonParser.Grammar["key_value_pair"]);
        }
    }
}
