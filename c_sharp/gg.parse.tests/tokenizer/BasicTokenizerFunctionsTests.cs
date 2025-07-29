using gg.parse.tokenizer;

namespace gg.parse.tests.tokenizer
{
    [TestClass]
    public class BasicTokenizerFunctionsTests
    {
        [TestMethod]
        public void CreateDigitFunction_Parse_SucceedsAndFails()
        {
            var func = BasicTokenizerFunctions.CreateDigitFunction();
            var success = func.Parse("5", 0);
            Assert.IsNotNull(success);
            Assert.AreEqual(0, success.Range.Start);
            Assert.AreEqual(1, success.Range.Length);

            var fail = func.Parse("a", 0);
            Assert.IsNull(fail);
        }

        [TestMethod]
        public void CreateSignFunction_Parse_SucceedsAndFails()
        {
            var func = BasicTokenizerFunctions.CreateSignFunction();
            var plus = func.Parse("+", 0);
            var minus = func.Parse("-", 0);
            Assert.IsNotNull(plus);
            Assert.IsNotNull(minus);
            Assert.AreEqual(1, plus.Range.Length);
            Assert.AreEqual(1, minus.Range.Length);

            var fail = func.Parse("a", 0);
            Assert.IsNull(fail);
        }

        [TestMethod]
        public void CreateZeroOrOneFunction_Parse_SucceedsAndFails()
        {
            var digit = BasicTokenizerFunctions.CreateDigitFunction();
            var func = BasicTokenizerFunctions.CreateZeroOrOneFunction(digit);

            var one = func.Parse("5", 0);
            Assert.IsNotNull(one);
            Assert.AreEqual(1, one.Range.Length);

            var zero = func.Parse("a", 0);
            Assert.IsNotNull(zero);
            Assert.AreEqual(0, zero.Range.Length);
        }

        [TestMethod]
        public void CreateZeroOrMoreFunction_Parse_SucceedsAndFails()
        {
            var digit = BasicTokenizerFunctions.CreateDigitFunction();
            var func = BasicTokenizerFunctions.CreateZeroOrMoreFunction(digit);

            var many = func.Parse("12345", 0);
            Assert.IsNotNull(many);
            Assert.AreEqual(5, many.Range.Length);

            var zero = func.Parse("abc", 0);
            Assert.IsNotNull(zero);
            Assert.AreEqual(0, zero.Range.Length);
        }

        [TestMethod]
        public void CreateOneOrMoreFunction_Parse_SucceedsAndFails()
        {
            var digit = BasicTokenizerFunctions.CreateDigitFunction();
            var func = BasicTokenizerFunctions.CreateOneOrMoreFunction(digit);

            var many = func.Parse("12345", 0);
            Assert.IsNotNull(many);
            Assert.AreEqual(5, many.Range.Length);

            var fail = func.Parse("abc", 0);
            Assert.IsNull(fail);
        }

        [TestMethod]
        public void CreateDigitString_Parse_SucceedsAndFails()
        {
            var func = BasicTokenizerFunctions.CreateDigitString();

            var digits = func.Parse("12345", 0);
            Assert.IsNotNull(digits);
            Assert.AreEqual(5, digits.Range.Length);

            var fail = func.Parse("abc", 0);
            Assert.IsNull(fail);
        }

        [TestMethod]
        public void CreateIntegerFunction_Parse_SucceedsAndFails()
        {
            var func = BasicTokenizerFunctions.CreateIntegerFunction();

            var pos = func.Parse("+123", 0);
            var neg = func.Parse("-456", 0);
            var noSign = func.Parse("789", 0);
            Assert.IsNotNull(pos);
            Assert.IsNotNull(neg);
            Assert.IsNotNull(noSign);
            Assert.AreEqual(4, pos.Range.Length);
            Assert.AreEqual(4, neg.Range.Length);
            Assert.AreEqual(3, noSign.Range.Length);

            var fail = func.Parse("abc", 0);
            Assert.IsNull(fail);
        }

        [TestMethod]
        public void CreateFloatFunction_Parse_SucceedsAndFails()
        {
            var func = BasicTokenizerFunctions.CreateFloatFunction();

            var simple = func.Parse("123.45", 0);
            var signed = func.Parse("-123.45", 0);
            var withExp = func.Parse("123.45e+6", 0);
            Assert.IsNotNull(simple);
            Assert.IsNotNull(signed);
            Assert.IsNotNull(withExp);
            Assert.IsTrue(simple.Range.Length >= 6);
            Assert.IsTrue(signed.Range.Length >= 7);
            Assert.IsTrue(withExp.Range.Length >= 9);

            var fail = func.Parse("abc", 0);
            Assert.IsNull(fail);
        }

        [TestMethod]
        public void CreateWhitespaceFunction_Parse_SucceedsAndFails()
        {
            var func = BasicTokenizerFunctions.CreateWhitespaceFunction();

            var ws = func.Parse(" \t\r\n", 0);
            Assert.IsNotNull(ws);
            Assert.AreEqual(1, ws.Range.Length);

            var fail = func.Parse("a", 0);
            Assert.IsNull(fail);
        }

        [TestMethod]
        public void CreateStringFunction_Parse_SucceedsAndFails()
        {
            var func = BasicTokenizerFunctions.CreateStringFunction();

            var quoted = func.Parse("\"hello\"", 0);
            Assert.IsNotNull(quoted);
            Assert.AreEqual(7, quoted.Range.Length);

            var empty = func.Parse("\"\"", 0);
            Assert.IsNotNull(empty);
            Assert.AreEqual(2, empty.Range.Length);

            var fail = func.Parse("hello", 0);
            Assert.IsNull(fail);
        }
    }
}