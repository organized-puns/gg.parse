#nullable disable

using gg.parse.rules;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.tests.rules
{
    [TestClass]
    public class MatchAnyTests
    {
        [TestMethod]
        public void MatchAnyCharacter_GreenPath()
        {
            var function = new MatchAnyData<char>("TestFunction");

            IsTrue(function.Parse(['a'], 0).FoundMatch);
            IsTrue(function.Parse(['1'], 0).FoundMatch);
            IsTrue(function.Parse(['%'], 0).FoundMatch);
        }

        [TestMethod]
        public void MatchAnyInt_GreenPath()
        {
            var function = new MatchAnyData<int>("TestFunction");

            IsTrue(function.Parse([1], 0).FoundMatch);
            IsTrue(function.Parse([-1, 0], 1).FoundMatch);
        }

        [TestMethod]
        public void MatchAnyCharacter_RedPath()
        {
            var function = new MatchAnyData<char>("TestFunction");

            IsFalse(function.Parse([], 0).FoundMatch);
            IsFalse(function.Parse(['a'], 1).FoundMatch);
        }
    }
}
