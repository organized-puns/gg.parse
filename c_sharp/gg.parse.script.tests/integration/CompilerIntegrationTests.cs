using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

using gg.parse.script.pipeline;
using gg.parse.script.compiler;

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
                IsTrue(ex.InnerException is AggregateException aex && aex.InnerExceptions.Count() == 3);
            }
        }

        
    }
}
