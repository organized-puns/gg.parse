using gg.parse.script.common;
using gg.parse.script.parser;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.script.tests.integration
{
    // xxx create common test function / currently contains a lot of copy paste
    [TestClass]
    public class ScriptTokenizerTests
    {
        [TestMethod]
        public void CreateLogLevelTokens_Parse_ExpectLogLevelsFound()
        {
            var tokenizer = new ScriptTokenizer();
            var testText = "fatal error warning info debug";
            var result = tokenizer.Root!.Parse(testText);

            IsTrue(result.FoundMatch);
            IsTrue(result.MatchLength == testText.Length);

            var expectedNames = new string[]
            {
                CommonTokenNames.LogFatal,
                CommonTokenNames.LogError,
                CommonTokenNames.LogWarning,
                CommonTokenNames.LogInfo,
                CommonTokenNames.LogDebug
            };

            for (var i = 0; i < expectedNames.Length; i++)
            {
                var name = result.Annotations![i].Rule!.Name;
                IsTrue(name == expectedNames[i]);
            }
        }

        [TestMethod]
        public void CreateIfTokens_Parse_ExpectLogIfFound()
        {
            var tokenizer = new ScriptTokenizer();
            var testText = "if";
            var result = tokenizer.Root!.Parse(testText.ToCharArray(), 0);

            IsTrue(result.FoundMatch);
            IsTrue(result.MatchLength == testText.Length);

            var expectedNames = new string[]
            {
                CommonTokenNames.If,
            };

            for (var i = 0; i < expectedNames.Length; i++)
            {
                var name = result.Annotations![i].Rule!.Name;
                IsTrue(name.IndexOf(expectedNames[i]) >= 0);
            }
        }

        [TestMethod]
        public void CreateInputWithUnknownToken_Parse_ExpectError()
        {
            var tokenizer = new ScriptTokenizer();
            var testText = "^ valid_token";
            var result = tokenizer.Root!.Parse(testText.ToCharArray(), 0);

            IsTrue(result.FoundMatch);
            IsTrue(result.MatchLength == testText.Length);

            var expectedNames = new string[]
            {
                CommonTokenNames.UnknownToken,
                CommonTokenNames.Identifier,
            };

            for (var i = 0; i < expectedNames.Length; i++)
            {
                var name = result.Annotations![i].Rule!.Name;
                IsTrue(name == expectedNames[i]);
            }
        }

        [TestMethod]
        public void CreateInputWithFindToken_Parse_ExpectFindSkipFound()
        {
            var tokenizer = new ScriptTokenizer();
            var testText = ">> >>>";
            var result = tokenizer.Root!.Parse(testText);

            IsTrue(result.FoundMatch);
            IsTrue(result.MatchLength == testText.Length);

            var expectedNames = new string[]
            {
                CommonTokenNames.Find,
                CommonTokenNames.Skip
            };

            for (var i = 0; i < expectedNames.Length; i++)
            {
                var name = result.Annotations![i].Rule!.Name;
                IsTrue(name == expectedNames[i]);
            }
        }
    }
}
