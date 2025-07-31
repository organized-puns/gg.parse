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
            var table = new RuleTable<char>();
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
            var table = new RuleTable<char>();
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
    }
}
