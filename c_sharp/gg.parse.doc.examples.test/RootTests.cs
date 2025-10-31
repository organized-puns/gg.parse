using gg.parse.script;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.doc.examples.test
{
    [TestClass]
    public class RootTests
    {
        [TestMethod]
        public void CreateTokenizerWithAndWithoutExplicitRoot_Parse_ExpectFirstRuleToPass()
        {
            var tokenizerWithDefaultRoot = new ParserBuilder().From("foo = 'foo'; bar = 'bar', foo;");

            IsTrue(tokenizerWithDefaultRoot.Tokenize("foo"));
            IsFalse(tokenizerWithDefaultRoot.Tokenize("barfoo"));

            var tokenizerWithExplicitRoot = new ParserBuilder().From("foo = 'foo'; root = 'bar', foo;");

            IsFalse(tokenizerWithExplicitRoot.Tokenize("foo"));
            IsTrue(tokenizerWithExplicitRoot.Tokenize("barfoo"));

        }
    }
}
