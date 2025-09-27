#nullable disable

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

using gg.parse.rules;

namespace gg.parse.script.tests.integration
{
    /// <summary>
    /// High level integration tests targeting compiler execution.
    /// </summary>
    [TestClass]
    public class CompilerIntegrationTests
    {
        
        [TestMethod]
        public void CreateEmptyRule_Compile_ExpectNopToShowUp()
        {
            // xxx turn this into a more unit-y test
            var parser = new RuleGraphBuilder().From("token = 't1';", "empty_rule=;");
            var emptyRule = parser.Parser.FindRule("empty_rule") as NopRule<int>;
            
            IsTrue(emptyRule != null);

            var (_, outcome) = parser.Parse("t1");

            // nop doesn't do match
            IsTrue(outcome.FoundMatch && outcome.MatchLength == 0);
        }
    }
}
