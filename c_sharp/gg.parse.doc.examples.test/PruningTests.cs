using gg.parse.rules;
using gg.parse.script;
using System.Diagnostics;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.doc.examples.test
{
    [TestClass]
    public class PruningTests
    {
        [TestMethod]
        public void SetupTopLevelRule_Parse_ExpectTopLevelAndChildrenInTokens()
        {
            var text = "a";
            var tokens = new ParserBuilder()
                            .From("top_level_example = 'a' | 'b' | 'c';")
                            .Tokenize(text);

            IsTrue(tokens && tokens.Count == 1);
            IsTrue(tokens[0] == "top_level_example");
            IsTrue(tokens[0].Rule is MatchOneOf<char>);
            IsTrue(tokens[0][0].Rule is MatchDataSequence<char>);

            Debug.WriteLine(ScriptUtils.PrettyPrintTokens(text, tokens.Annotations));
        }

        [TestMethod]
        public void SetupInlineRule_Parse_ExpectTopLevelAndLeavesInTokens()
        {
            var text = "ab";
            var tokens = new ParserBuilder()
                            .From("inline_example = ('a', 'b') | ('b', 'c');")
                            .Tokenize(text);

            IsTrue(tokens && tokens.Count == 1);
            IsTrue(tokens[0] == "inline_example");
            IsTrue(tokens[0].Rule is MatchOneOf<char>);
            // line / anonymous rules
            IsTrue(tokens[0][0].Rule is MatchDataSequence<char>);
            IsTrue(tokens[0][1].Rule is MatchDataSequence<char>);

            Debug.WriteLine(ScriptUtils.PrettyPrintTokens(text, tokens.Annotations));
        }
    }
}
