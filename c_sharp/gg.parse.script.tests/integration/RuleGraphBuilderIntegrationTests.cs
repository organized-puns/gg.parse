using gg.parse.rules;
using System.Diagnostics;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.script.tests.integration
{
    [TestClass]
    public sealed class RuleGraphBuilderIntegrationTests
    {
        [TestMethod]
        public void SetupReferencesWithDifferentRuleOutputs_BuildParser_ExpectRulesToMatchSpecifiedOutput()
        {
            var parser = new ParserBuilder().From($"foo='foo';bar='bar';", "root=foo, #bar, ~foo;");

            var root = parser.GrammarGraph.FindRule("root") as MatchRuleSequence<int>;

            IsTrue(root != null);
            
            IsTrue((root[0] as RuleReference<int>).Reference == "foo");
            IsTrue(root[0].Production == IRule.Output.Self);

            IsTrue((root[1] as RuleReference<int>).Reference == "bar");
            IsTrue(root[1].Production == IRule.Output.Children);

            IsTrue((root[2] as RuleReference<int>).Reference == "foo");
            IsTrue(root[2].Production == IRule.Output.Void);

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

    }
}
