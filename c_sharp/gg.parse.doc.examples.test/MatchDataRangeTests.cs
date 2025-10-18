using gg.parse.rules;
using gg.parse.script;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.doc.examples.test
{
    /// <summary>
    /// Used in MatchDataRange documentation example.
    /// </summary>

    [TestClass]
    public sealed class MatchDataRangeTests
    {
        [TestMethod]
        public void MatchSimpleData_SuccessExample()
        {
            var rule = new MatchDataRange<char>("matchLowercaseLetter", 'a', 'z');

            IsTrue(rule.Parse(['a'], 0));
            IsTrue(rule.Parse(['d'], 0));
            IsTrue(rule.Parse(['z'], 0));
        }

        [TestMethod]
        public void MatchSimpleData_FailureExample()
        {
            var rule = new MatchDataRange<char>("matchLowercaseLetter", 'a', 'z');

            // input is empty
            IsFalse(rule.Parse([], 0));

            // should only match lower case letters
            IsFalse(rule.Parse(['A'], 0));

            // should not match numbers
            IsFalse(rule.Parse(['1'], 0));
        }

        [TestMethod]
        public void MatchSimpleDataUsingScript()
        {
            var parser = new ParserBuilder().From("lower_case_letter = {'a'..'z'};");

            IsTrue(parser.Parse("a").tokens);

            IsTrue(parser.Parse("z").tokens);

            IsFalse(parser.Parse("A").tokens);

            IsFalse(parser.Parse("1").tokens);
        }
    }
}
