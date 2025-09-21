#nullable disable

using gg.parse.rulefunctions;
using gg.parse.script;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.tests.compiler
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
            var parser = new ScriptParser().InitializeFromDefinition("token = 't1';", "empty_rule=;");
            var emptyRule = parser.AstBuilder.FindRule("empty_rule") as NopRule<int>;
            
            IsTrue(emptyRule != null);

            var (_, outcome) = parser.Parse("t1");

            // nop doesn't do match
            IsTrue(outcome.FoundMatch && outcome.MatchedLength == 0);
        }
    }
}
