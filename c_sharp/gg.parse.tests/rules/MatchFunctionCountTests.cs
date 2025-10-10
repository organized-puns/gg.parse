#nullable disable

using gg.parse.rules;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.tests.rulefunctions
{
    [TestClass]
    public class MatchFunctionCountTests
    {
        [TestMethod]
        public void MatchFunctionCount_ValidSingleInput_ReturnsSuccess()
        {
            var function = new MatchDataSequence<int>("TestFunction", [1, 2, 3]);
            var rule = new MatchCount<int>("TestRule", function, RuleOutput.Self, 1, 3);

            function.Id = 1;
            rule.Id = 2;

            var input = new[] { 1, 2, 3, 4 };
            var result = rule.Parse(input, 0);
            
            IsTrue(result.FoundMatch);
            AreEqual(3, result.MatchLength);
            IsTrue(result.Annotations.Count == 1);
            IsTrue(result.Annotations[0].Rule == rule);
            IsTrue(result.Annotations[0].Children!.Count == 1);
            IsTrue(result.Annotations[0].Children![0].Rule == function);
        }

        [TestMethod]
        public void MatchFunctionCount_ValidMultipleInput_ReturnsSuccess()
        {
            var function = new MatchDataSequence<int>("TestFunction", new[] { 1, 2, 3 });
            var rule = new MatchCount<int>("TestRule", function, RuleOutput.Self, 1, 2);

            function.Id = 1;
            rule.Id = 2;

            var input = new[] { 1, 2, 3, 1, 2, 3, 1, 2, 3, 4 };
            var result = rule.Parse(input, 0);
            IsTrue(result.FoundMatch);
            AreEqual(6, result.MatchLength);
            IsTrue(result.Annotations!.Count == 1);
            IsTrue(result.Annotations![0].Range.Start == 0);
            IsTrue(result.Annotations![0].Range.End == 6);
            IsTrue(result.Annotations![0].Rule == rule);
            IsTrue(result.Annotations![0].Children!.Count == 2);
            IsTrue(result.Annotations![0].Children![0].Rule == function);
            IsTrue(result.Annotations![0].Children![0].Start == 0);
            IsTrue(result.Annotations![0].Children![0].End == 3);
            IsTrue(result.Annotations![0].Children![1].Rule == function);
            IsTrue(result.Annotations![0].Children![1].Start == 3);
            IsTrue(result.Annotations![0].Children![1].End == 6);
        }

        [TestMethod]
        public void MatchFunctionCount_ValidMultipleTransitiveInput_ReturnsSuccess()
        {
            var function = new MatchDataSequence<int>("TestFunction", new[] { 1, 2, 3 });
            var rule = new MatchCount<int>("TestRule", function, RuleOutput.Children, 1, 2);

            function.Id = 1;
            rule.Id = 2;

            var input = new[] { 1, 2, 3, 1, 2, 3, 1, 2, 3, 4 };
            var result = rule.Parse(input, 0);
            IsTrue(result.FoundMatch);
            AreEqual(6, result.MatchLength);
            IsTrue(result.Annotations.Count == 2);
            IsTrue(result.Annotations[0].Range.Start == 0);
            IsTrue(result.Annotations[0].Range.End == 3);
            IsTrue(result.Annotations[0].Rule == function);
            IsTrue(result.Annotations[0].Children == null);
            IsTrue(result.Annotations[1].Range.Start == 3);
            IsTrue(result.Annotations[1].Range.End == 6);
            IsTrue(result.Annotations[1].Rule == function);
            IsTrue(result.Annotations[1].Children == null);
        }

        [TestMethod]
        public void MatchFunctionCount_ValidMultipleNoneInput_ReturnsSuccess()
        {
            var function = new MatchDataSequence<int>("TestFunction", new[] { 1, 2, 3 });
            var rule = new MatchCount<int>("TestRule", function, RuleOutput.Void, 1, 2);

            function.Id = 1;
            rule.Id = 2;

            var input = new[] { 1, 2, 3, 1, 2, 3, 1, 2, 3, 4 };
            var result = rule.Parse(input, 0);
            IsTrue(result.FoundMatch);
            AreEqual(6, result.MatchLength);
            IsTrue(result.Annotations == null);
        }

        [TestMethod]
        public void CreateZeroOrMoreFunctionWithLiteral_Parse_ExpectSuccess()
        {
            var fooLiteral = new MatchDataSequence<char>("fooLiteral", "foo".ToCharArray());
            var function = new MatchCount<char>("TestFunction", fooLiteral, min: 0, max: 0);

            var result = function.Parse("foo".ToCharArray(), 0);

            IsTrue(result.FoundMatch);
            IsTrue(result.MatchLength == 3);

            result = function.Parse("foofoofoo".ToCharArray(), 0);

            IsTrue(result.FoundMatch);
            IsTrue(result.MatchLength == 9);

            result = function.Parse("foobarfoo".ToCharArray(), 0);

            IsTrue(result.FoundMatch);
            IsTrue(result.MatchLength == 3);

            result = function.Parse("bar".ToCharArray(), 0);

            IsTrue(result.FoundMatch);
            IsTrue(result.MatchLength == 0);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidProgramException))]
        public void CreateZeroOrMoreFunctionWithPotentialEndlessLoop_Parse_ExpectException()
        {
            var fooLiteral = new MatchDataSequence<char>("fooLiteral", "foo".ToCharArray());
            var function1 = new MatchCount<char>("TestFunction1", fooLiteral, min: 0, max: 0);
            var function2 = new MatchCount<char>("TestFunction2", function1, min: 0, max: 0);

            var result = function2.Parse("foo".ToCharArray(), 0);

            IsTrue(result.FoundMatch);
            IsTrue(result.MatchLength == 3);

            result = function2.Parse("bar".ToCharArray(), 0);
        }
    }
}
