using System.Diagnostics;

using gg.parse.util;
using gg.parse.rules;
using gg.parse.script.compiler;
using gg.parse.script.pipeline;
using gg.parse.script.parser;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.script.tests.parserbuilder
{
    [TestClass]
    public sealed class ParserBuilderIntegrationTests
    {
        // short hands for annotation pruning tokens
        private const string pa = AnnotationPruningToken.All;
        private const string pc = AnnotationPruningToken.Children;
        private const string pr = AnnotationPruningToken.Root;

        [TestMethod]
        public void CreateEmptyRule_Compile_ExpectNopToShowUp()
        {
            // xxx turn this into a more unit-y test
            var parser = new ParserBuilder().From("token = 't1';", "empty_rule=;");
            var emptyRule = parser.GrammarGraph.FindRule("empty_rule") as NopRule<int>;

            IsTrue(emptyRule != null);

            var (_, outcome) = parser.Parse("t1");

            // nop doesn't do match
            IsTrue(outcome.FoundMatch && outcome.MatchLength == 0);
        }

        [TestMethod]
        public void CreateRulesWithMissingReferences_Compile_ExpectCompliationErrorPerRule()
        {
            var builder = new ParserBuilder();
            try
            {
                builder.From("dummy='foo';", "r1=bar;r2=baz;r3=qaz;");
                Fail();
            }
            catch (ScriptPipelineException ex)
            {
                IsTrue(ex.InnerException is AggregateException aex && aex.InnerExceptions.Count == 3);
            }
        }



        [TestMethod]
        public void SetupTrivalCase_Parse_ExpectAWorkingParser()
        {
            var token = "bar";
            var parser = new ParserBuilder().From($"foo='{token}';", "root=foo;");

            var (_, barParseResult) = parser.Parse(token);

            IsTrue(barParseResult.FoundMatch);
            IsTrue(barParseResult[0]!.Rule!.Name == "root");
        }

        [TestMethod]
        public void SetupFindBar_Parse_ExpectBarFoundIfPresentInString()
        {
            var parser = new ParserBuilder().From($"foo = >> lit; lit = 'bar';", "root = foo;");

            var testStringWithBar = "123ba345bar567";
            var (tokensResult, barParseResult) = parser.Parse(testStringWithBar);

            IsTrue(barParseResult.FoundMatch);
            IsTrue(barParseResult[0]!.Rule!.Name == "root");

            var rangeTillBar = tokensResult.Annotations.CombinedRange(barParseResult[0].Range);

            IsTrue(rangeTillBar.End == 8);

            var testStringWithoutBar = "123ba345ar567";

            (tokensResult, _) = parser.Parse(testStringWithoutBar);

            IsFalse(tokensResult.FoundMatch);
        }


        [TestMethod]
        public void SetupFindAllBars_Parse_ExpectBarFoundIfPresentInString()
        {
            var searchTerm = "bar";
            var tokenizer = new ParserBuilder().From(
                $"{pr}find_all_bars = +( find_bar, '{searchTerm}' );" +
                $"{pa}find_bar      = >> '{searchTerm}';"
            );

            var testStringWithBar = "123ba345bar567 bar ";
            var (result, _) = tokenizer.Parse(testStringWithBar);

            IsTrue(result);
            IsTrue(result.Count == 2);

            if (result)
            {
                Debug.WriteLine($"found ({result.Count}) instances of '{searchTerm}':");
                Debug.WriteLine(string.Join("\n", result.Select(
                    annotation => $"{annotation.Range} = '{testStringWithBar.Substring(annotation)}'."
                )));
            }
        }


        [TestMethod]
        public void SetupSkipUntilBar_Parse_ExpectBarFoundIfPresentInString()
        {
            var parser = new ParserBuilder().From($"foo = >>> lit; lit = 'bar';", "root = foo;");

            var testStringWithBar = "123ba345bar567";
            var (tokensResult, barParseResult) = parser.Parse(testStringWithBar);

            IsTrue(barParseResult.FoundMatch);
            IsTrue(barParseResult[0]!.Rule!.Name == "root");

            var rangeTillBar = tokensResult.Annotations.CombinedRange(barParseResult[0].Range);

            IsTrue(rangeTillBar.End == 8);

            var testStringWithoutBar = "123ba345ar567";

            (tokensResult, barParseResult) = parser.Parse(testStringWithoutBar);

            // unlike find, skip will be happy if no bars are found
            IsTrue(tokensResult.FoundMatch);

            rangeTillBar = tokensResult.Annotations.CombinedRange(barParseResult[0].Range);

            IsTrue(rangeTillBar.End == testStringWithoutBar.Length);
        }

        [TestMethod]
        public void CreateRulesWithTopLevelAndAnonymousBinaryRules_Compile_ExpectOutputDifferences()
        {
            var topLevelRuleName = "top_level";
            var anonymousRuleName = "anonymous";

            // parse and compile the rules
            var builder = new ParserBuilder()
                .From(
                    // create a rule which has a binary rule at the top level
                    $"{topLevelRuleName} = 'a', 'b', 'c';" +
                    // create a rule which has anonymous binary rules
                    $"{anonymousRuleName} = ('a', 'b', 'c') | ('d', 'e', 'f');"
                );

            var topLevelRule = builder.TokenGraph.FindRule(topLevelRuleName) as MatchRuleSequence<char>;

            IsNotNull(topLevelRule);
            IsTrue(topLevelRule.Rules.Count() == 3);
            IsTrue(topLevelRule.Prune == AnnotationPruning.None);

            var anonymousRule = builder.TokenGraph.FindRule(anonymousRuleName) as MatchOneOf<char>;

            IsNotNull(anonymousRule);
            IsTrue(anonymousRule.Count == 2);
            IsTrue(anonymousRule.Prune == AnnotationPruning.None);

            // the anonymous parts of the rule should by default return the children
            IsTrue(anonymousRule[0] is MatchRuleSequence<char>);
            IsTrue(anonymousRule[0].Prune == AnnotationPruning.Root);

            IsTrue(anonymousRule[1] is MatchRuleSequence<char>);
            IsTrue(anonymousRule[1].Prune == AnnotationPruning.Root);
        }

        [TestMethod]
        public void CreateRulesWithTopLevelAndAnonymousCountRules_Compile_ExpectOutputDifferences()
        {
            var topLevelRuleName = "top_level";
            var anonymousRuleName = "anonymous";

            // parse and compile the rules
            var builder = new ParserBuilder()
                .From(
                    // create a rule which has a count rule at the top level
                    $"{topLevelRuleName} = *'foo';" +
                    // create a rule which has anonymous count rules
                    $"{anonymousRuleName} = *'foo' | +'bar';"
                );

            var topLevelRule = builder.TokenGraph.FindRule(topLevelRuleName) as MatchCount<char>;

            IsNotNull(topLevelRule);
            IsTrue(topLevelRule.Rule is MatchDataSequence<char>);
            IsTrue(topLevelRule.Prune == AnnotationPruning.None);

            var anonymousRule = builder.TokenGraph.FindRule(anonymousRuleName) as MatchOneOf<char>;

            IsNotNull(anonymousRule);
            IsTrue(anonymousRule.Count == 2);
            IsTrue(anonymousRule.Prune == AnnotationPruning.None);

            // the anonymous parts of the rule should by default return the children
            IsTrue(anonymousRule[0] is MatchCount<char>);
            IsTrue(anonymousRule[0].Prune == AnnotationPruning.Root);

            IsTrue(anonymousRule[1] is MatchCount<char>);
            IsTrue(anonymousRule[1].Prune == AnnotationPruning.Root);
        }

        [TestMethod]
        public void CreateRulesWithTopLevelAndAnonymousUnaryRules_Compile_ExpectOutputSelf()
        {
            var topLevelRuleName = "top_level";
            var anonymousRuleName = "anonymous";

            // parse and compile the rules
            var builder = new ParserBuilder()
                .From(
                    // create a rule which has a unary rule at the top level
                    $"{topLevelRuleName} = !'foo';" +
                    // create a rule which has anonymous unary rules
                    $"{anonymousRuleName} = if 'foo' | !'bar';"
                );

            var topLevelRule = builder.TokenGraph.FindRule(topLevelRuleName) as MatchNot<char>;

            IsNotNull(topLevelRule);
            IsTrue(topLevelRule.Rule is MatchDataSequence<char>);
            IsTrue(topLevelRule.Prune == AnnotationPruning.None);

            var anonymousRule = builder.TokenGraph.FindRule(anonymousRuleName) as MatchOneOf<char>;

            IsNotNull(anonymousRule);
            IsTrue(anonymousRule.Count == 2);
            IsTrue(anonymousRule.Prune == AnnotationPruning.None);

            // the anonymous parts (lookahead functions) of the rule should by default prune everything
            IsTrue(anonymousRule[0] is MatchCondition<char>);
            IsTrue(anonymousRule[0].Prune == AnnotationPruning.All);

            // !bar is a lookahead so it should prune all as well
            IsTrue(anonymousRule[1] is MatchNot<char>);
            IsTrue(anonymousRule[1].Prune == AnnotationPruning.All);
        }
    }
}
