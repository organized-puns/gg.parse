using gg.parse.script;
using gg.parse.script.common;
using gg.parse.script.pipeline;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.argparser.tests
{
    
    /// <summary>
    /// Verify tokens are correctly parsed
    /// </summary>
    [TestClass]
    public class ArgsTokeninzerTest
    {
        static readonly string TokenFileName = "assets/args.tokens";
        static readonly string GrammarFileName = "assets/args.grammar";

        static readonly string TokenText = File.ReadAllText(TokenFileName);
        static readonly string GrammarText = File.ReadAllText(TokenFileName);

        [TestMethod]
        public void SetupAllTokens_Parse_ExpectAllTokensFound()
        {
            var testCases = new (string input, string expectedRuleName)[]
            {
                ("--", "verbose_switch"),
                ("-", "shorthand_switch"),
                ("http://bla.com:8080/param?a&b&c", "other_value"),
                // spot check to see if properties are included
                ("123.0", "float"),
                ("-123", "int")
            };

            var logger = new PipelineLogger();

            var builder = new ParserBuilder();

            try
            {
                builder.FromFile(TokenFileName, logger: logger);
            }
            catch (Exception ex)
            {
                Fail();
            }

            foreach (var (input, ruleName) in testCases)
            {
                var result = builder.TokenGraph.TokenizeText(input);

                IsTrue(result);
                IsTrue(result[0].Rule.Name == ruleName);
            }
        }

    }
}
