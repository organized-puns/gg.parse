using gg.parse.ebnf;
using gg.parse.rulefunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.tests.ebnf
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
                var name = tokenizer.FindRule(result.Annotations![i].RuleId)!.Name;
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
                var name = tokenizer.FindRule(result.Annotations![i].RuleId)!.Name;
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
                var name = tokenizer.FindRule(result.Annotations![i].RuleId)!.Name;
                IsTrue(name == expectedNames[i]);
            }
        }      
    }
}
