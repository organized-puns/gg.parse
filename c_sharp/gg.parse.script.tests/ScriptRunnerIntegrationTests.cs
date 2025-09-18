using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.script.tests
{
    [TestClass]
    public sealed class ScriptRunnerIntegrationTests
    {
        [TestMethod]
        public void SetupTrivalCase_Parse_ExpectAWorkingParser()
        {
            var token = "bar";
            var parser = new ScriptParser().CreateFromDefinition($"foo='{token}';", "root=foo;");

            var barParseResult = parser.Parse(token);

            IsTrue(barParseResult.FoundMatch);
            IsTrue(parser.Parser.FindRule(barParseResult[0]!.RuleId)!.Name == "root");
        }
    }
}
