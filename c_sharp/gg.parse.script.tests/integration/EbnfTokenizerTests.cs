#nullable disable

using gg.parse.rulefunctions;
using gg.parse.script.parsing;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.script.tests.integration
{
    [TestClass]
    public class EbnfTokenizerTests
    {
        [TestMethod]
        public void CreateLogLevelTokens_Parse_ExpectLogLevelsFound()
        {
            var tokenizer = new EbnfTokenizer();
            var testText = "fatal error warning info debug";
            var result = tokenizer.Root!.Parse(testText.ToCharArray(), 0);

            IsTrue(result.FoundMatch);
            IsTrue(result.MatchedLength == testText.Length);

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
            var tokenizer = new EbnfTokenizer();
            var testText = "if";
            var result = tokenizer.Root!.Parse(testText.ToCharArray(), 0);

            IsTrue(result.FoundMatch);
            IsTrue(result.MatchedLength == testText.Length);

            var expectedNames = new string[]
            {
                CommonTokenNames.If,
            };

            for (var i = 0; i < expectedNames.Length; i++)
            {
                var name = result.Annotations![i].Rule!.Name;
                IsTrue(name == expectedNames[i]);
            }
        }

        [TestMethod]
        public void CreateInputWithUnknownToken_Parse_ExpectError()
        {
            var tokenizer = new EbnfTokenizer();
            var testText = "^ valid_token";
            var result = tokenizer.Root!.Parse(testText.ToCharArray(), 0);

            IsTrue(result.FoundMatch);
            IsTrue(result.MatchedLength == testText.Length);

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
    }
}
