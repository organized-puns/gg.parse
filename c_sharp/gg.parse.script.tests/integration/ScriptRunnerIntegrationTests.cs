using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.script.tests.integration
{
    [TestClass]
    public sealed class ScriptRunnerIntegrationTests
    {
        [TestMethod]
        public void SetupTrivalCase_Parse_ExpectAWorkingParser()
        {
            var token = "bar";
            var parser = new RuleGraphBuilder().InitializeFromDefinition($"foo='{token}';", "root=foo;");

            var (_, barParseResult) = parser.Parse(token);

            IsTrue(barParseResult.FoundMatch);
            IsTrue(barParseResult[0]!.Rule!.Name == "root");
        }

        [TestMethod]
        public void SetupFindBar_Parse_ExpectBarFoundIfPresentInString()
        {
            var parser = new RuleGraphBuilder().InitializeFromDefinition($"foo = >> lit; lit = 'bar';", "root = foo;");

            var testStringWithBar = "123ba345bar567";
            var (tokensResult, barParseResult) = parser.Parse(testStringWithBar);

            IsTrue(barParseResult.FoundMatch);
            IsTrue(barParseResult[0]!.Rule!.Name == "root");

            var rangeTillBar = tokensResult.Annotations.UnionOfRanges(barParseResult[0].Range);

            IsTrue(rangeTillBar.End == 8);

            var testStringWithoutBar = "123ba345ar567";

            (tokensResult, barParseResult) = parser.Parse(testStringWithoutBar);

            IsFalse(tokensResult.FoundMatch);
        }

        [TestMethod]
        public void SetupSkipUntilBar_Parse_ExpectBarFoundIfPresentInString()
        {
            var parser = new RuleGraphBuilder().InitializeFromDefinition($"foo = >>> lit; lit = 'bar';", "root = foo;");

            var testStringWithBar = "123ba345bar567";
            var (tokensResult, barParseResult) = parser.Parse(testStringWithBar);

            IsTrue(barParseResult.FoundMatch);
            IsTrue(barParseResult[0]!.Rule!.Name == "root");

            var rangeTillBar = tokensResult.Annotations.UnionOfRanges(barParseResult[0].Range);

            IsTrue(rangeTillBar.End == 8);

            var testStringWithoutBar = "123ba345ar567";

            (tokensResult, barParseResult) = parser.Parse(testStringWithoutBar);

            // unlike find, skip will be happy if no bars are found
            IsTrue(tokensResult.FoundMatch);

            rangeTillBar = tokensResult.Annotations.UnionOfRanges(barParseResult[0].Range);

            IsTrue(rangeTillBar.End == testStringWithoutBar.Length);
        }

    }
}
