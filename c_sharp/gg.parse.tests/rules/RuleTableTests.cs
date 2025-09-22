#nullable disable

using gg.parse.script.common;

namespace gg.parse.tests.rulefunctions
{
    [TestClass]
    public class RuleTableTests
    {
        [TestMethod]
        public void DigitRuleTest()
        {
            var table = new CommonTokenizer();
            var digitRule = table.Digit(CommonTokenNames.Digit);
            
            Assert.IsNotNull(digitRule);
            Assert.AreEqual(CommonTokenNames.Digit, digitRule.Name);
            Assert.IsTrue(table.FindRule(CommonTokenNames.Digit) == digitRule);

            // calling digit a second time should return the same rule
            Assert.IsTrue(table.Digit(CommonTokenNames.Digit) == digitRule);

            // Check if the rule can parse a digit character
            var input = new[] { '5' };
            var result = digitRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(1, result.MatchedLength);
            // Check if the rule fails for a non-digit character
            input = ['a'];
            result = digitRule.Parse(input, 0);
            Assert.IsFalse(result.FoundMatch);
        }

        [TestMethod]
        public void DigitSequenceTest()
        {
            var table = new CommonTokenizer();
            var digitSequenceRule = table.DigitSequence(CommonTokenNames.DigitSequence);

            Assert.IsNotNull(digitSequenceRule);
            Assert.AreEqual(CommonTokenNames.DigitSequence, digitSequenceRule.Name);
            Assert.IsTrue(table.FindRule(CommonTokenNames.DigitSequence) == digitSequenceRule);

            // calling digit a second time should return the same rule
            Assert.IsTrue(table.DigitSequence(CommonTokenNames.DigitSequence) == digitSequenceRule);

            // Check if the rule can parse a digit character
            var input = "123".ToArray();
            var result = digitSequenceRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(3, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].Rule == digitSequenceRule);
            Assert.IsTrue(result.Annotations[0].Children == null);

            // Check if the rule fails for a non-digit character
            input = ['a'];
            result = digitSequenceRule.Parse(input, 0);
            Assert.IsFalse(result.FoundMatch);
        }

        [TestMethod]
        public void IntegerTest()
        {
            var table = new CommonTokenizer();
            var intRule = table.Integer(CommonTokenNames.Integer);

            Assert.IsNotNull(intRule);
            Assert.AreEqual(CommonTokenNames.Integer, intRule.Name);
            Assert.IsTrue(table.FindRule(CommonTokenNames.Integer) == intRule);

            // calling digit a second time should return the same rule
            Assert.IsTrue(table.Integer(CommonTokenNames.Integer) == intRule);

            // Check if the rule can parse a digit character
            var input = "123".ToArray();
            var result = intRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(3, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].Rule == intRule);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = [.. "-1234"];
            result = intRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(5, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].Rule == intRule);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = [.. "+56789"];
            result = intRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(6, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].Rule == intRule);
            Assert.IsTrue(result.Annotations[0].Children == null);

            // Check if the rule fails for a non-digit character
            input = ['a'];
            result = intRule.Parse(input, 0);
            Assert.IsFalse(result.FoundMatch);
        }

        [TestMethod]
        public void FloatTest()
        {
            var table = new CommonTokenizer();
            var floatRule = table.Float(CommonTokenNames.Float);

            Assert.IsNotNull(floatRule);
            Assert.AreEqual(CommonTokenNames.Float, floatRule.Name);

            Assert.IsTrue(table.FindRule(CommonTokenNames.Float) == floatRule);
            Assert.IsTrue(table.Float(CommonTokenNames.Float) == floatRule);

            var input = "123.345".ToArray();
            var result = floatRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(7, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].Rule == floatRule);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = "-123.345e2".ToArray();
            result = floatRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(10, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].Rule == floatRule);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = "0.345E-43".ToArray();
            result = floatRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(9, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].Rule == floatRule);
            Assert.IsTrue(result.Annotations[0].Children == null);

            Assert.IsFalse(floatRule.Parse("123".ToArray(), 0).FoundMatch);
            Assert.IsFalse(floatRule.Parse("123.".ToArray(), 0).FoundMatch);
            Assert.IsFalse(floatRule.Parse(".123".ToArray(), 0).FoundMatch);
        }

        [TestMethod]
        public void LiteralTest()
        {
            var table = new CommonTokenizer();
            var fooRule = table.Literal("foo", "foo");
            var barRule = table.Literal("bar", "bar");
            
            Assert.IsNotNull(fooRule);
            Assert.AreEqual("foo", fooRule.Name);
            Assert.AreEqual("bar", barRule.Name);
            Assert.IsTrue(table.FindRule("foo") == fooRule);
            Assert.IsTrue(table.FindRule("bar") == barRule);

            // calling digit a second time should return the same rule
            Assert.IsTrue(table.Literal("foo", "foo") == fooRule);
            Assert.IsTrue(table.Literal("bar", "bar") == barRule);

            // Check if the rule can parse a digit character
            var input = "foo".ToArray();
            var result = fooRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(3, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].Rule == fooRule);
            Assert.IsTrue(result.Annotations[0].Children == null);

            Assert.IsFalse(fooRule.Parse(input, 1).FoundMatch);
            Assert.IsFalse(barRule.Parse(input, 0).FoundMatch);

            input = [.. "_bar_"];
            result = barRule.Parse(input, 1);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(3, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].Rule == barRule);
            Assert.IsTrue(result.Annotations[0].Children == null);

            Assert.IsFalse(fooRule.Parse(input, 1).FoundMatch);
        }

        [TestMethod]
        public void BooleanTest()
        {
            var table = new CommonTokenizer();
            var booleanRule = table.Boolean(CommonTokenNames.Boolean);

            Assert.IsNotNull(booleanRule);
            Assert.AreEqual($"{CommonTokenNames.Boolean}", booleanRule.Name);
            
            Assert.IsTrue(table.FindRule($"{CommonTokenNames.Boolean}") == booleanRule);
            
            // calling digit a second time should return the same rule
            Assert.IsTrue(table.Boolean(CommonTokenNames.Boolean) == booleanRule);


            var input = "true".ToArray();
            var result = booleanRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(4, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].Rule == booleanRule);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = "false".ToArray();
            result = booleanRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(5, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].Rule == booleanRule);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = "False".ToArray();
            result = booleanRule.Parse(input, 0);
            Assert.IsFalse(result.FoundMatch);
        }

        [TestMethod]
        public void StringTest()
        {
            var table = new CommonTokenizer();
            var doubleQuoteStringRule = table.MatchString($"{CommonTokenNames.String}(\")", delimiter: '"');
            var quoteStringRule = table.MatchString($"{CommonTokenNames.String}(')", delimiter: '\'');

            Assert.IsNotNull(doubleQuoteStringRule);
            Assert.IsNotNull(quoteStringRule);
            Assert.AreEqual($"{CommonTokenNames.String}(\")", doubleQuoteStringRule.Name);
            Assert.AreEqual($"{CommonTokenNames.String}(')", quoteStringRule.Name);

            Assert.IsTrue(table.MatchString($"{CommonTokenNames.String}(\")", delimiter: '"') == doubleQuoteStringRule);
            Assert.IsTrue(table.MatchString($"{CommonTokenNames.String}(')", delimiter: '\'') == quoteStringRule);

            var input = "'true'".ToArray();
            var result = quoteStringRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(6, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].Rule == quoteStringRule);
            Assert.IsTrue(result.Annotations[0].Children == null);

            Assert.IsFalse(quoteStringRule.Parse(input, 1).FoundMatch);
            Assert.IsFalse(quoteStringRule.Parse(input, 5).FoundMatch);
            Assert.IsFalse(doubleQuoteStringRule.Parse(input, 0).FoundMatch);

            input = "\"true\"".ToArray();
            result = doubleQuoteStringRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(6, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].Rule == doubleQuoteStringRule);
            Assert.IsTrue(result.Annotations[0].Children == null);

            Assert.IsFalse(doubleQuoteStringRule.Parse(input, 1).FoundMatch);
            Assert.IsFalse(doubleQuoteStringRule.Parse(input, 5).FoundMatch);
            Assert.IsFalse(quoteStringRule.Parse(input, 0).FoundMatch);


            input = "\"\\\"quote\\\"\"".ToArray();
            result = doubleQuoteStringRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(11, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].Rule == doubleQuoteStringRule);
            Assert.IsTrue(result.Annotations[0].Children == null);

            Assert.IsFalse(doubleQuoteStringRule.Parse(input, 1).FoundMatch);
            Assert.IsTrue(doubleQuoteStringRule.Parse(input, 2).FoundMatch);
            Assert.IsFalse(doubleQuoteStringRule.Parse(input, 1).FoundMatch);
            Assert.IsFalse(quoteStringRule.Parse(input, 0).FoundMatch);
        }

        [TestMethod]
        public void IdentifierTests()
        {
            var table = new CommonTokenizer();
            var identifier = table.Identifier(CommonTokenNames.Identifier);

            Assert.IsNotNull(identifier);
            Assert.AreEqual($"{CommonTokenNames.Identifier}", identifier.Name);

            Assert.IsTrue(table.Identifier(CommonTokenNames.Identifier) == identifier);

            var input = "_foo".ToArray();
            var result = identifier.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(4, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].Rule == identifier);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = "Bar123".ToArray();
            result = identifier.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(6, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].Rule == identifier);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = "1a2b3".ToArray();
            result = identifier.Parse(input, 0);
            Assert.IsFalse(result.FoundMatch);

        }
    }
}
