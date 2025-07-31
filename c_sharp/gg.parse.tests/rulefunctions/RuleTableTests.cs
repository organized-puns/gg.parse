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
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.MatchedLength);
            // Check if the rule fails for a non-digit character
            input = ['a'];
            result = digitRule.Parse(input, 0);
            Assert.IsFalse(result.IsSuccess);
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
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(3, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == digitSequenceRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            // Check if the rule fails for a non-digit character
            input = ['a'];
            result = digitSequenceRule.Parse(input, 0);
            Assert.IsFalse(result.IsSuccess);
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
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(3, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == intRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = [.. "-1234"];
            result = intRule.Parse(input, 0);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(5, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == intRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = [.. "+56789"];
            result = intRule.Parse(input, 0);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(6, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == intRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            // Check if the rule fails for a non-digit character
            input = ['a'];
            result = intRule.Parse(input, 0);
            Assert.IsFalse(result.IsSuccess);
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
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(7, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == floatRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = "-123.345e2".ToArray();
            result = floatRule.Parse(input, 0);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(10, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == floatRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            input = "0.345E-43".ToArray();
            result = floatRule.Parse(input, 0);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(9, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == floatRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            Assert.IsFalse(floatRule.Parse("123".ToArray(), 0).IsSuccess);
            Assert.IsFalse(floatRule.Parse("123.".ToArray(), 0).IsSuccess);
            Assert.IsFalse(floatRule.Parse(".123".ToArray(), 0).IsSuccess);
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
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(3, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == fooRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            Assert.IsFalse(fooRule.Parse(input, 1).IsSuccess);
            Assert.IsFalse(barRule.Parse(input, 0).IsSuccess);

            input = [.. "_bar_"];
            result = barRule.Parse(input, 1);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(3, result.MatchedLength);
            Assert.IsTrue(result.Annotations!.Count == 1);
            Assert.IsTrue(result.Annotations[0].FunctionId == barRule.Id);
            Assert.IsTrue(result.Annotations[0].Children == null);

            Assert.IsFalse(fooRule.Parse(input, 1).IsSuccess);
        }
    }
}
