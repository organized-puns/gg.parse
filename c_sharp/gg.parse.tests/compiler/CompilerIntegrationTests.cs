using gg.parse.ebnf;
using gg.parse.rulefunctions;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.tests.compiler
{
    [TestClass]
    public class CompilerIntegrationTests
    {
        [TestMethod]
        public void CreateEmptyRule_Compile_ExpectNopToShowUp()
        {
            var parser = new EbnfParser("token = 't1';", "empty_rule=;");
            var emptyRule = parser.FindParserRule("empty_rule") as NopRule<int>;
            
            IsTrue(emptyRule != null);

            var outcome = parser.Parse("t1");

            // nop doesn't do match
            IsTrue(outcome.FoundMatch && outcome.MatchedLength == 0);
        }
    }
}
