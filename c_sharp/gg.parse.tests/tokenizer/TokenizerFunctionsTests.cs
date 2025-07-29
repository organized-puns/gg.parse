using gg.parse.tokenizer;

namespace gg.parse.tests.tokenizer
{
    [TestClass]
    public class TokenizerFunctionsTests
    {
        private class MockTokenFunction : TokenFunction
        {
            private readonly Func<string, int, Annotation?> _parseFunc;

            public MockTokenFunction(Func<string, int, Annotation?> parseFunc)
                : base("Mock", 1) => _parseFunc = parseFunc;

            public override Annotation? Parse(string text, int start) => _parseFunc(text, start);
        }

        [TestMethod]
        public void AnyCharacter_ReturnsRange_WhenEnoughCharacters()
        {
            var range = TokenizerFunctions.AnyCharacter("abcdef", 2, 2, 3);
            Assert.IsNotNull(range);
            Assert.AreEqual(2, range.Value.Start);
            Assert.AreEqual(3, range.Value.Length);
        }

        [TestMethod]
        public void AnyCharacter_ReturnsNull_WhenNotEnoughCharacters()
        {
            var range = TokenizerFunctions.AnyCharacter("abc", 2, 2, 3);
            Assert.IsNull(range);
        }

        [TestMethod]
        public void InCharacterRange_ReturnsRange_WhenInRange()
        {
            var range = TokenizerFunctions.InCharacterRange("abc", 1, 'a', 'c');
            Assert.IsNotNull(range);
            Assert.AreEqual(1, range.Value.Start);
            Assert.AreEqual(1, range.Value.Length);
        }

        [TestMethod]
        public void InCharacterRange_ReturnsNull_WhenOutOfRange()
        {
            var range = TokenizerFunctions.InCharacterRange("abc", 1, 'd', 'z');
            Assert.IsNull(range);
        }

        [TestMethod]
        public void InCharacterSet_ReturnsRange_WhenInSet()
        {
            var range = TokenizerFunctions.InCharacterSet("abc", 1, "bx");
            Assert.IsNotNull(range);
            Assert.AreEqual(1, range.Value.Start);
            Assert.AreEqual(1, range.Value.Length);
        }

        [TestMethod]
        public void InCharacterSet_ReturnsNull_WhenNotInSet()
        {
            var range = TokenizerFunctions.InCharacterSet("abc", 1, "yz");
            Assert.IsNull(range);
        }

        [TestMethod]
        public void Literal_ReturnsRange_WhenMatch()
        {
            var range = TokenizerFunctions.Literal("abcdef", 2, "cde");
            Assert.IsNotNull(range);
            Assert.AreEqual(2, range.Value.Start);
            Assert.AreEqual(3, range.Value.Length);
        }

        [TestMethod]
        public void Literal_ReturnsNull_WhenNoMatch()
        {
            var range = TokenizerFunctions.Literal("abcdef", 2, "xyz");
            Assert.IsNull(range);
        }

        [TestMethod]
        public void MatchNot_ReturnsRange_WhenFunctionReturnsNull()
        {
            var func = new MockTokenFunction((text, start) => null);
            var range = TokenizerFunctions.MatchNot("abc", 0, func);
            Assert.IsNotNull(range);
            Assert.AreEqual(0, range.Value.Start);
            Assert.AreEqual(0, range.Value.Length);
        }

        [TestMethod]
        public void MatchNot_ReturnsNull_WhenFunctionReturnsAnnotation()
        {
            var func = new MockTokenFunction((text, start) =>
                new Annotation(AnnotationCategory.Token, 1, new Range(start, 1)));
            var range = TokenizerFunctions.MatchNot("abc", 0, func);
            Assert.IsNull(range);
        }

        [TestMethod]
        public void MatchCount_ReturnsRange_WhenMinMet()
        {
            var func = new MockTokenFunction((text, start) =>
                start < text.Length
                    ? new Annotation(AnnotationCategory.Token, 1, new Range(start, 1))
                    : null);
            var range = TokenizerFunctions.MatchCount("abc", 0, func, 2, 3);
            Assert.IsNotNull(range);
            Assert.AreEqual(0, range.Value.Start);
            Assert.AreEqual(3, range.Value.Length);
        }

        [TestMethod]
        public void MatchCount_ReturnsNull_WhenMinNotMet()
        {
            var func = new MockTokenFunction((text, start) =>
                start < text.Length
                    ? new Annotation(AnnotationCategory.Token, 1, new Range(start, 1))
                    : null);
            var range = TokenizerFunctions.MatchCount("a", 0, func, 2, 3);
            Assert.IsNull(range);
        }

        [TestMethod]
        public void MatchCount_StopsOnError()
        {
            var func = new MockTokenFunction((text, start) =>
                start == 0
                    ? new Annotation(AnnotationCategory.Error, 1, new Range(start, 1))
                    : null);
            var range = TokenizerFunctions.MatchCount("abc", 0, func, 1, 3);
            Assert.IsNull(range);
        }

        [TestMethod]
        public void MatchOneOf_ReturnsFirstMatchingRange()
        {
            var func1 = new MockTokenFunction((text, start) => null);
            var func2 = new MockTokenFunction((text, start) =>
                new Annotation(AnnotationCategory.Token, 2, new Range(start, 2)));
            var range = TokenizerFunctions.MatchOneOf("abc", 0, func1, func2);
            Assert.IsNotNull(range);
            Assert.AreEqual(0, range.Value.Start);
            Assert.AreEqual(2, range.Value.Length);
        }

        [TestMethod]
        public void MatchOneOf_ReturnsNull_WhenNoMatch()
        {
            var func1 = new MockTokenFunction((text, start) => null);
            var func2 = new MockTokenFunction((text, start) => null);
            var range = TokenizerFunctions.MatchOneOf("abc", 0, func1, func2);
            Assert.IsNull(range);
        }

        [TestMethod]
        public void MatchSequence_ReturnsRange_WhenAllMatch()
        {
            var func1 = new MockTokenFunction((text, start) =>
                new Annotation(AnnotationCategory.Token, 1, new Range(start, 1)));
            var func2 = new MockTokenFunction((text, start) =>
                new Annotation(AnnotationCategory.Token, 2, new Range(start, 1)));
            var range = TokenizerFunctions.MatchSequence("ab", 0, func1, func2);
            Assert.IsNotNull(range);
            Assert.AreEqual(0, range.Value.Start);
            Assert.AreEqual(2, range.Value.Length);
        }

        [TestMethod]
        public void MatchSequence_ReturnsNull_WhenAnyFails()
        {
            var func1 = new MockTokenFunction((text, start) =>
                new Annotation(AnnotationCategory.Token, 1, new Range(start, 1)));
            var func2 = new MockTokenFunction((text, start) => null);
            var range = TokenizerFunctions.MatchSequence("ab", 0, func1, func2);
            Assert.IsNull(range);
        }
    }
}