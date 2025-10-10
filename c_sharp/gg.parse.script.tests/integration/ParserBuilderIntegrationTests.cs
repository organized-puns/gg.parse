using System.Diagnostics;

using gg.parse.rules;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.script.tests.integration
{
    [TestClass]
    public sealed class ParserBuilderIntegrationTests
    {
        [TestMethod]
        public void SetupReferencesWithDifferentRuleOutputs_BuildParser_ExpectRulesToMatchSpecifiedOutput()
        {
            var parser = new ParserBuilder().From($"#tokens=*(foo|bar);foo='foo';bar='bar';", "root=foo, #bar, ~foo;");

            var root = parser.GrammarGraph.FindRule("root") as MatchRuleSequence<int>;

            // validate the run and its outputs
            IsTrue(root != null);
            
            IsTrue((root[0] as RuleReference<int>).Reference == "foo");
            IsTrue(root[0].Output == RuleOutput.Self);

            IsTrue((root[1] as RuleReference<int>).Reference == "bar");
            IsTrue(root[1].Output == RuleOutput.Children);

            IsTrue((root[2] as RuleReference<int>).Reference == "foo");
            IsTrue(root[2].Output == RuleOutput.Void);

            var result = parser.Parse("foobarfoo");

            IsTrue(result.syntaxTree);

            IsTrue(result.syntaxTree.Count == 1);

            // should hold foo and bar, the third output is dropped
            IsTrue(result.syntaxTree[0].Count == 2);
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

            (tokensResult, barParseResult) = parser.Parse(testStringWithoutBar);

            IsFalse(tokensResult.FoundMatch);
        }


        [TestMethod]
        public void SetupFindAllBars_Parse_ExpectBarFoundIfPresentInString()
        {
            var searchTerm = "bar";
            var tokenizer = new ParserBuilder().From(
                $"#find_all_bars = +( find_bar, '{searchTerm}' );" +
                $"~find_bar      = >> '{searchTerm}';"
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
            IsTrue(topLevelRule.SequenceRules.Length == 3);
            IsTrue(topLevelRule.Output == RuleOutput.Self);

            var anonymousRule = builder.TokenGraph.FindRule(anonymousRuleName) as MatchOneOf<char>;

            IsNotNull(anonymousRule);
            IsTrue(anonymousRule.RuleOptions.Length == 2);
            IsTrue(anonymousRule.Output == RuleOutput.Self);

            // the anonymous parts of the rule should by default return the children
            IsTrue(anonymousRule[0] is MatchRuleSequence<char>);
            IsTrue(anonymousRule[0].Output == RuleOutput.Children);

            IsTrue(anonymousRule[1] is MatchRuleSequence<char>);
            IsTrue(anonymousRule[1].Output == RuleOutput.Children);
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
            IsTrue(topLevelRule.Output == RuleOutput.Self);

            var anonymousRule = builder.TokenGraph.FindRule(anonymousRuleName) as MatchOneOf<char>;

            IsNotNull(anonymousRule);
            IsTrue(anonymousRule.RuleOptions.Length == 2);
            IsTrue(anonymousRule.Output == RuleOutput.Self);

            // the anonymous parts of the rule should by default return the children
            IsTrue(anonymousRule[0] is MatchCount<char>);
            IsTrue(anonymousRule[0].Output == RuleOutput.Children);

            IsTrue(anonymousRule[1] is MatchCount<char>);
            IsTrue(anonymousRule[1].Output == RuleOutput.Children);
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
            IsTrue(topLevelRule.Output == RuleOutput.Self);

            var anonymousRule = builder.TokenGraph.FindRule(anonymousRuleName) as MatchOneOf<char>;

            IsNotNull(anonymousRule);
            IsTrue(anonymousRule.RuleOptions.Length == 2);
            IsTrue(anonymousRule.Output == RuleOutput.Self);

            // the anonymous parts (lookahead functions) of the rule should by default return self 
            IsTrue(anonymousRule[0] is MatchCondition<char>);
            IsTrue(anonymousRule[0].Output == RuleOutput.Self);

            IsTrue(anonymousRule[1] is MatchNot<char>);
            IsTrue(anonymousRule[1].Output == RuleOutput.Self);
        }
    }
}
