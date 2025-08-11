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
    public class MatchFunctionCountTests
    {
        [TestMethod]
        public void MatchFunctionCount_ValidSingleInput_ReturnsSuccess()
        {
            var function = new MatchDataSequence<int>("TestFunction", new[] { 1, 2, 3 });
            var rule = new MatchFunctionCount<int>("TestRule", function, AnnotationProduct.Annotation, 1, 3);

            function.Id = 1;
            rule.Id = 2;

            var input = new[] { 1, 2, 3, 4 };
            var result = rule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(3, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations![0].FunctionId == rule.Id);
            Assert.IsTrue(result.Annotations![0].Children!.Count == 1);
            Assert.IsTrue(result.Annotations![0].Children![0].FunctionId == function.Id);
        }

        [TestMethod]
        public void MatchFunctionCount_ValidMultipleInput_ReturnsSuccess()
        {
            var function = new MatchDataSequence<int>("TestFunction", new[] { 1, 2, 3 });
            var rule = new MatchFunctionCount<int>("TestRule", function, AnnotationProduct.Annotation, 1, 2);

            function.Id = 1;
            rule.Id = 2;

            var input = new[] { 1, 2, 3, 1, 2, 3, 1, 2, 3, 4 };
            var result = rule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(6, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations![0].Range.Start == 0);
            Assert.IsTrue(result.Annotations![0].Range.End == 6);
            Assert.IsTrue(result.Annotations![0].FunctionId == rule.Id);
            Assert.IsTrue(result.Annotations![0].Children!.Count == 2);
            Assert.IsTrue(result.Annotations![0].Children![0].FunctionId == function.Id);
            Assert.IsTrue(result.Annotations![0].Children![0].Start == 0);
            Assert.IsTrue(result.Annotations![0].Children![0].End == 3);
            Assert.IsTrue(result.Annotations![0].Children![1].FunctionId == function.Id);
            Assert.IsTrue(result.Annotations![0].Children![1].Start == 3);
            Assert.IsTrue(result.Annotations![0].Children![1].End == 6);
        }

        [TestMethod]
        public void MatchFunctionCount_ValidMultipleTransitiveInput_ReturnsSuccess()
        {
            var function = new MatchDataSequence<int>("TestFunction", new[] { 1, 2, 3 });
            var rule = new MatchFunctionCount<int>("TestRule", function, AnnotationProduct.Transitive, 1, 2);

            function.Id = 1;
            rule.Id = 2;

            var input = new[] { 1, 2, 3, 1, 2, 3, 1, 2, 3, 4 };
            var result = rule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(6, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 2);
            Assert.IsTrue(result.Annotations![0].Range.Start == 0);
            Assert.IsTrue(result.Annotations![0].Range.End == 3);
            Assert.IsTrue(result.Annotations![0].FunctionId == function.Id);
            Assert.IsTrue(result.Annotations![0].Children == null);
            Assert.IsTrue(result.Annotations![1].Range.Start == 3);
            Assert.IsTrue(result.Annotations![1].Range.End == 6);
            Assert.IsTrue(result.Annotations![1].FunctionId == function.Id);
            Assert.IsTrue(result.Annotations![1].Children == null);
        }

        [TestMethod]
        public void MatchFunctionCount_ValidMultipleNoneInput_ReturnsSuccess()
        {
            var function = new MatchDataSequence<int>("TestFunction", new[] { 1, 2, 3 });
            var rule = new MatchFunctionCount<int>("TestRule", function, AnnotationProduct.None, 1, 2);

            function.Id = 1;
            rule.Id = 2;

            var input = new[] { 1, 2, 3, 1, 2, 3, 1, 2, 3, 4 };
            var result = rule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(6, result.MatchedLength);
            Assert.IsTrue(result.Annotations == null);
        }
    }
}
