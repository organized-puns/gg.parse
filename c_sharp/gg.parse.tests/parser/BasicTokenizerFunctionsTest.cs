using gg.parse.basefunctions;
using gg.parse.parser;

namespace gg.parse.tests.parser
{
    [TestClass]
    public class BasicTokenizerFunctionsTest
    {
        [TestMethod]
        public void Digit_MatchesDigitCharacter()
        {
            var func = BaseTokenizerFunctions.Digit();
            var input = new[] { '5' };
            var result = func.Parse(input, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(AnnotationDataCategory.Data, result.Category);
            Assert.AreEqual(0, result.Range.Start);
            Assert.AreEqual(1, result.Range.Length);
        }

        [TestMethod]
        public void Digit_DoesNotMatchNonDigitCharacter()
        {
            var func = BaseTokenizerFunctions.Digit();
            var input = new[] { 'a' };
            var result = func.Parse(input, 0);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Digit_DoesNotMatchEmptyInput()
        {
            var func = BaseTokenizerFunctions.Digit();
            var input = Array.Empty<char>();
            var result = func.Parse(input, 0);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void String_MatchesQuotedString()
        {
            var func = BaseTokenizerFunctions.String();
            var input = "\"hello\"".ToCharArray();
            var result = func.Parse(input, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Range.Start);
            Assert.AreEqual(input.Length, result.Range.Length);
        }

        [TestMethod]
        public void String_MatchesEscapedQuote()
        {
            var func = BaseTokenizerFunctions.String();
            var input = "\"he\\\"llo\"".ToCharArray();
            var result = func.Parse(input, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Range.Start);
            Assert.AreEqual(input.Length, result.Range.Length);
        }

        [TestMethod]
        public void String_DoesNotMatchUnclosedString()
        {
            var func = BaseTokenizerFunctions.String();
            var input = "\"hello".ToCharArray();
            var result = func.Parse(input, 0);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Integer_MatchesPositiveInteger()
        {
            var func = BaseTokenizerFunctions.Integer();
            var input = "123".ToCharArray();
            var result = func.Parse(input, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Range.Start);
            Assert.AreEqual(3, result.Range.Length);
        }

        [TestMethod]
        public void Integer_MatchesNegativeInteger()
        {
            var func = BaseTokenizerFunctions.Integer();
            var input = "-456".ToCharArray();
            var result = func.Parse(input, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Range.Start);
            Assert.AreEqual(4, result.Range.Length);
        }

        [TestMethod]
        public void Integer_MatchesPositiveSignInteger()
        {
            var func = BaseTokenizerFunctions.Integer();
            var input = "+789".ToCharArray();
            var result = func.Parse(input, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Range.Start);
            Assert.AreEqual(4, result.Range.Length);
        }

        [TestMethod]
        public void Integer_DoesNotMatchNonInteger()
        {
            var func = BaseTokenizerFunctions.Integer();
            var input = "abc".ToCharArray();
            var result = func.Parse(input, 0);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Integer_DoesNotMatchSignOnly()
        {
            var func = BaseTokenizerFunctions.Integer();
            var input = "+".ToCharArray();
            var result = func.Parse(input, 0);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Float_MatchesSimpleFloat()
        {
            var func = BaseTokenizerFunctions.Float();
            var input = "3.14".ToCharArray();
            var result = func.Parse(input, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Range.Start);
            Assert.AreEqual(input.Length, result.Range.Length);
        }

        [TestMethod]
        public void Float_MatchesNegativeFloat()
        {
            var func = BaseTokenizerFunctions.Float();
            var input = "-2.718".ToCharArray();
            var result = func.Parse(input, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Range.Start);
            Assert.AreEqual(input.Length, result.Range.Length);
        }

        [TestMethod]
        public void Float_MatchesPositiveSignFloat()
        {
            var func = BaseTokenizerFunctions.Float();
            var input = "+0.001".ToCharArray();
            var result = func.Parse(input, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Range.Start);
            Assert.AreEqual(input.Length, result.Range.Length);
        }

        [TestMethod]
        public void Float_MatchesFloatWithExponent()
        {
            var func = BaseTokenizerFunctions.Float();
            var input = "6.022e23".ToCharArray();
            var result = func.Parse(input, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Range.Start);
            Assert.AreEqual(input.Length, result.Range.Length);
        }

        [TestMethod]
        public void Float_MatchesFloatWithExponentAndSign()
        {
            var func = BaseTokenizerFunctions.Float();
            var input = "-1.6E-19".ToCharArray();
            var result = func.Parse(input, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Range.Start);
            Assert.AreEqual(input.Length, result.Range.Length);
        }

        [TestMethod]
        public void Float_DoesNotMatchInteger()
        {
            var func = BaseTokenizerFunctions.Float();
            var input = "12345".ToCharArray();
            var result = func.Parse(input, 0);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Float_DoesNotMatchMissingFraction()
        {
            var func = BaseTokenizerFunctions.Float();
            var input = "3.".ToCharArray();
            var result = func.Parse(input, 0);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Float_DoesNotMatchMissingDot()
        {
            var func = BaseTokenizerFunctions.Float();
            var input = "314".ToCharArray();
            var result = func.Parse(input, 0);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Float_DoesNotMatchMissingIntegerPart()
        {
            var func = BaseTokenizerFunctions.Float();
            var input = ".123".ToCharArray();
            var result = func.Parse(input, 0);

            Assert.IsNull(result);
        }
    }
}
