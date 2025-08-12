using gg.parse.rulefunctions.datafunctions;

namespace gg.parse.tests.rules
{
    [TestClass]
    public class MatchAnyTests
    {
        [TestMethod]
        public void MatchAnyCharacter_GreenPath()
        {
            var function = new MatchAnyData<char>("TestFunction");

            Assert.IsTrue(function.Parse("a".ToCharArray(), 0).FoundMatch);
            Assert.IsTrue(function.Parse("1".ToCharArray(), 0).FoundMatch);
            Assert.IsTrue(function.Parse("%".ToCharArray(), 0).FoundMatch);
        }

        [TestMethod]
        public void MatchAnyInt_GreenPath()
        {
            var function = new MatchAnyData<int>("TestFunction");

            Assert.IsTrue(function.Parse([1], 0).FoundMatch);
            Assert.IsTrue(function.Parse([-1, 0], 1).FoundMatch);
        }

        [TestMethod]
        public void MatchAnyCharacter_RedPath()
        {
            var function = new MatchAnyData<char>("TestFunction");

            Assert.IsFalse(function.Parse([], 0).FoundMatch);
            Assert.IsFalse(function.Parse(['a'], 1).FoundMatch);
        }
    }
}
