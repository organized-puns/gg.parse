using gg.parse.rulefunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gg.parse.tests.rulefunctions
{
    [TestClass]
    public class RuleTableTests
    {
        [TestMethod]
        public void DigitRuleTest()
        {
            var table = new BasicTokensTable();
            var digitRule = table.Digit();
            
            Assert.IsNotNull(digitRule);
            Assert.AreEqual(TokenNames.Digit, digitRule.Name);
            Assert.IsTrue(table.FindRule(TokenNames.Digit) == digitRule);

            // calling digit a second time should return the same rule
            Assert.IsTrue(table.Digit() == digitRule);

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
            var table = new BasicTokensTable();
            var digitSequenceRule = table.DigitSequence();

            Assert.IsNotNull(digitSequenceRule);
            Assert.AreEqual(TokenNames.DigitSequence, digitSequenceRule.Name);
            Assert.IsTrue(table.FindRule(TokenNames.DigitSequence) == digitSequenceRule);

            // calling digit a second time should return the same rule
            Assert.IsTrue(table.DigitSequence() == digitSequenceRule);

            // Check if the rule can parse a digit character
            var input = "123".ToArray();
            var result = digitSequenceRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(3, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == digitSequenceRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            // Check if the rule fails for a non-digit character
            input = ['a'];
            result = digitSequenceRule.Parse(input, 0);
            Assert.IsFalse(result.FoundMatch);
        }

        [TestMethod]
        public void IntegerTest()
        {
            var table = new BasicTokensTable();
            var intRule = table.Integer();

            Assert.IsNotNull(intRule);
            Assert.AreEqual(TokenNames.Integer, intRule.Name);
            Assert.IsTrue(table.FindRule(TokenNames.Integer) == intRule);

            // calling digit a second time should return the same rule
            Assert.IsTrue(table.Integer() == intRule);

            // Check if the rule can parse a digit character
            var input = "123".ToArray();
            var result = intRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(3, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == intRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = [.. "-1234"];
            result = intRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(5, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == intRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = [.. "+56789"];
            result = intRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(6, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == intRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            // Check if the rule fails for a non-digit character
            input = ['a'];
            result = intRule.Parse(input, 0);
            Assert.IsFalse(result.FoundMatch);
        }

        [TestMethod]
        public void FloatTest()
        {
            var table = new BasicTokensTable();
            var floatRule = table.Float();

            Assert.IsNotNull(floatRule);
            Assert.AreEqual(TokenNames.Float, floatRule.Name);
            Assert.IsTrue(table.FindRule(TokenNames.Float) == floatRule);

            Assert.IsTrue(table.Float() == floatRule);

            var input = "123.345".ToArray();
            var result = floatRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(7, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == floatRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = "-123.345e2".ToArray();
            result = floatRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(10, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == floatRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = "0.345E-43".ToArray();
            result = floatRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(9, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == floatRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            Assert.IsFalse(floatRule.Parse("123".ToArray(), 0).FoundMatch);
            Assert.IsFalse(floatRule.Parse("123.".ToArray(), 0).FoundMatch);
            Assert.IsFalse(floatRule.Parse(".123".ToArray(), 0).FoundMatch);
        }

        [TestMethod]
        public void LiteralTest()
        {
            var table = new BasicTokensTable();
            var fooRule = table.Literal("foo");
            var barRule = table.Literal("bar");
            
            Assert.IsNotNull(fooRule);
            Assert.AreEqual($"{TokenNames.Literal}(foo)", fooRule.Name);
            Assert.AreEqual($"{TokenNames.Literal}(bar)", barRule.Name);
            Assert.IsTrue(table.FindRule($"{TokenNames.Literal}(foo)") == fooRule);
            Assert.IsTrue(table.FindRule($"{TokenNames.Literal}(bar)") == barRule);

            // calling digit a second time should return the same rule
            Assert.IsTrue(table.Literal("foo") == fooRule);
            Assert.IsTrue(table.Literal("bar") == barRule);

            // Check if the rule can parse a digit character
            var input = "foo".ToArray();
            var result = fooRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(3, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == fooRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            Assert.IsFalse(fooRule.Parse(input, 1).FoundMatch);
            Assert.IsFalse(barRule.Parse(input, 0).FoundMatch);

            input = [.. "_bar_"];
            result = barRule.Parse(input, 1);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(3, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == barRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            Assert.IsFalse(fooRule.Parse(input, 1).FoundMatch);
        }

        [TestMethod]
        public void BooleanTest()
        {
            var table = new BasicTokensTable();
            var booleanRule = table.Boolean();

            Assert.IsNotNull(booleanRule);
            Assert.AreEqual($"{TokenNames.Boolean}", booleanRule.Name);
            
            Assert.IsTrue(table.FindRule($"{TokenNames.Boolean}") == booleanRule);
            
            // calling digit a second time should return the same rule
            Assert.IsTrue(table.Boolean() == booleanRule);


            var input = "true".ToArray();
            var result = booleanRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(4, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == booleanRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = "false".ToArray();
            result = booleanRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(5, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == booleanRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = "False".ToArray();
            result = booleanRule.Parse(input, 0);
            Assert.IsFalse(result.FoundMatch);
        }

        [TestMethod]
        public void StringTest()
        {
            var table = new BasicTokensTable();
            var doubleQuoteStringRule = table.String(delimiter: '"');
            var quoteStringRule = table.String(delimiter: '\'');

            Assert.IsNotNull(doubleQuoteStringRule);
            Assert.IsNotNull(quoteStringRule);
            Assert.AreEqual($"{TokenNames.String}(\")", doubleQuoteStringRule.Name);
            Assert.AreEqual($"{TokenNames.String}(')", quoteStringRule.Name);

            Assert.IsTrue(table.String(delimiter: '"') == doubleQuoteStringRule);
            Assert.IsTrue(table.String(delimiter: '\'') == quoteStringRule);

            var input = "'true'".ToArray();
            var result = quoteStringRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(6, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == quoteStringRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            Assert.IsFalse(quoteStringRule.Parse(input, 1).FoundMatch);
            Assert.IsFalse(quoteStringRule.Parse(input, 5).FoundMatch);
            Assert.IsFalse(doubleQuoteStringRule.Parse(input, 0).FoundMatch);

            input = "\"true\"".ToArray();
            result = doubleQuoteStringRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(6, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == doubleQuoteStringRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            Assert.IsFalse(doubleQuoteStringRule.Parse(input, 1).FoundMatch);
            Assert.IsFalse(doubleQuoteStringRule.Parse(input, 5).FoundMatch);
            Assert.IsFalse(quoteStringRule.Parse(input, 0).FoundMatch);


            input = "\"\\\"quote\\\"\"".ToArray();
            result = doubleQuoteStringRule.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(11, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == doubleQuoteStringRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            Assert.IsFalse(doubleQuoteStringRule.Parse(input, 1).FoundMatch);
            Assert.IsTrue(doubleQuoteStringRule.Parse(input, 2).FoundMatch);
            Assert.IsFalse(doubleQuoteStringRule.Parse(input, 1).FoundMatch);
            Assert.IsFalse(quoteStringRule.Parse(input, 0).FoundMatch);
        }

        [TestMethod]
        public void IdentifierTests()
        {
            var table = new BasicTokensTable();
            var identifier = table.Identifier();

            Assert.IsNotNull(identifier);
            Assert.AreEqual($"{TokenNames.Identifier}", identifier.Name);

            Assert.IsTrue(table.Identifier() == identifier);

            var input = "_foo".ToArray();
            var result = identifier.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(4, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == identifier.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = "Bar123".ToArray();
            result = identifier.Parse(input, 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.AreEqual(6, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == identifier.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = "1a2b3".ToArray();
            result = identifier.Parse(input, 0);
            Assert.IsFalse(result.FoundMatch);

        }
    }
}
