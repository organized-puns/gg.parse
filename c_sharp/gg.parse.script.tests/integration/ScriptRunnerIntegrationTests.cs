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
    }
}
