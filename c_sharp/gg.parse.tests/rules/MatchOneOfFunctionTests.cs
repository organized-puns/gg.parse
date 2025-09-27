
using gg.parse.rules;

namespace gg.parse.tests.rulefunctions
{
    [TestClass]
    public class MatchOneOfFunctionTests
    {
        [TestMethod]
        public void MatchOneOfFunction_ValidSingleInput_ReturnsSuccess()
        {
            var function1 = new MatchDataSequence<int>("TestFunction1", [1, 2]);
            var function2 = new MatchDataSequence<int>("TestFunction2", [3, 4]);
            var rule = new MatchOneOfFunction<int>("TestRule", function1, function2);

            function1.Id = 1;
            function2.Id = 2;
            rule.Id = 3;

            var input = new[] { 1, 2, 3, 4 };
            var result = rule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(2, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations![0].Rule == rule);
            Assert.IsTrue(result.Annotations![0].Children!.Count == 1);
            Assert.IsTrue(result.Annotations![0].Children![0].Rule == function1);


            result = rule.Parse(input, 2);

            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(2, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations![0].Rule == rule);
            Assert.IsTrue(result.Annotations![0].Children!.Count == 1);
            Assert.IsTrue(result.Annotations![0].Children![0].Rule == function2);

            result = rule.Parse(input, 3);

            Assert.IsFalse(result.FoundMatch);
        }

        [TestMethod]
        public void MatchOneOfFunction_ValidInputWithTransitiveProduction_ReturnsSuccess()
        {
            var function1 = new MatchDataSequence<int>("TestFunction1", [1, 2]);
            var function2 = new MatchDataSequence<int>("TestFunction2", [3, 4]);
            var rule = new MatchOneOfFunction<int>("TestRule", IRule.Output.Children, 0, function1, function2);

            function1.Id = 1;
            function2.Id = 2;
            rule.Id = 3;

            var input = new[] { 1, 2, 3, 4 };
            var result = rule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(2, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations![0].Rule == function1);
            Assert.IsTrue(result.Annotations![0].Children == null);

            result = rule.Parse(input, 2);

            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(2, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations![0].Rule == function2);
            Assert.IsTrue(result.Annotations![0].Children == null);

            result = rule.Parse(input, 3);

            Assert.IsFalse(result.FoundMatch);
        }
    }
}
