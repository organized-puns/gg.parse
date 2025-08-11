using gg.parse.rulefunctions;
using gg.parse.rulefunctions.datafunctions;
using gg.parse.rulefunctions.rulefunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Assert.IsTrue(result.Annotations![0].FunctionId == rule.Id);
            Assert.IsTrue(result.Annotations![0].Children!.Count == 1);
            Assert.IsTrue(result.Annotations![0].Children![0].FunctionId == function1.Id);


            result = rule.Parse(input, 2);

            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(2, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations![0].FunctionId == rule.Id);
            Assert.IsTrue(result.Annotations![0].Children!.Count == 1);
            Assert.IsTrue(result.Annotations![0].Children![0].FunctionId == function2.Id);

            result = rule.Parse(input, 3);

            Assert.IsFalse(result.FoundMatch);
        }

        [TestMethod]
        public void MatchOneOfFunction_ValidInputWithTransitiveProduction_ReturnsSuccess()
        {
            var function1 = new MatchDataSequence<int>("TestFunction1", [1, 2]);
            var function2 = new MatchDataSequence<int>("TestFunction2", [3, 4]);
            var rule = new MatchOneOfFunction<int>("TestRule",AnnotationProduct.Transitive, function1, function2);

            function1.Id = 1;
            function2.Id = 2;
            rule.Id = 3;

            var input = new[] { 1, 2, 3, 4 };
            var result = rule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(2, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations![0].FunctionId == function1.Id);
            Assert.IsTrue(result.Annotations![0].Children == null);

            result = rule.Parse(input, 2);

            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(2, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations![0].FunctionId == function2.Id);
            Assert.IsTrue(result.Annotations![0].Children == null);

            result = rule.Parse(input, 3);

            Assert.IsFalse(result.FoundMatch);
        }
    }
}
