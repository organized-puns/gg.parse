using gg.parse.rules;
using gg.parse.script;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.doc.examples.test
{
    /// <summary>
    /// Used in MatchDataRange documentation example.
    /// </summary>

    [TestClass]
    public sealed class MatchDataSequenceTests
    {
        [TestMethod]
        public void MatchSimpleData_SuccessExample()
        {
            var rule = new MatchDataSequence<char>("matchFoo", [.."foo"]);

            IsTrue(rule.Parse([.."foo"], 0));
            IsTrue(rule.Parse([.."barfoo"], 3));
        }

        [TestMethod]
        public void MatchSimpleData_FailureExample()
        {
            var rule = new MatchDataSequence<char>("matchFoo", [.. "foo"]);

            // input is empty
            IsFalse(rule.Parse([], 0));

            // should only match foo, not bar
            IsFalse(rule.Parse([.."bar"], 0));

            // should capitals
            IsFalse(rule.Parse([.."Foo"], 0));
        }

        [TestMethod]
        public void MatchSimpleDataUsingScript()
        {
            var parser = new ParserBuilder().From("is_foo = 'foo';");

            IsTrue(parser.Parse("foo").tokens);

            IsFalse(parser.Parse("bar").tokens);
        }
    }
}
