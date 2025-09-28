using gg.parse.rules;

namespace gg.parse.script.tests.integration
{
    /// <summary>
    /// Various test based on a somewhat complex string tokenization script, specifically
    /// working towards checking if errors are picked up as expected.
    /// </summary>
    [TestClass]
    public class ComplexStringIntegrationTests
    {
        private static readonly string StringTokenizationText = File.ReadAllText("assets/string.tokens");

        [TestMethod]
        public void CreateParse_ParseValidStrings_ExpectNoErrors()
        {
            var parser = new ParserBuilder().From(StringTokenizationText);
            var stringRule = parser.Tokenizer.FindRule("string");

            var validStrings = new string[]
            {
                "\"foo\"",
                "\"_123#\"",
                "\"\""
            };

            foreach (var validString in validStrings)
            {
                var (result, _) = parser.Parse(validString);

                Assert.IsTrue(result.FoundMatch);
                Assert.IsTrue(result.Annotations != null);
                Assert.IsTrue(result.Annotations[0].Rule == stringRule);
                Assert.IsTrue(result.Annotations[0].Start == 0);
                Assert.IsTrue(result.Annotations[0].End == validString.Length);
                Assert.IsTrue(result.Annotations[0].Length == validString.Length);
                Assert.IsTrue(result.Annotations[0].Children == null);
            }
        }

        /// <summary>
        /// Test invalid strings that have an opening delimiter but then
        /// reach the end-of-file (EOF) before a closing delimiter is encountered.
        /// </summary>
        [TestMethod]
        public void InvalidEOFTerminatedString_Parse_ExpectErrors()
        {
            var parser = new ParserBuilder().From(StringTokenizationText);
            var errEOF = parser.Tokenizer.FindRule("err_string_eof");

            var invalidStrings = new string[]
            {
                "\"foo",
                "\"",
            };

            foreach (var testString in invalidStrings)
            {
                // turn off exception throwing for this test so we can test the error annotations
                var (result, _) = parser.Parse(testString, throwExceptionsOnError: false);

                Assert.IsTrue(result.FoundMatch);
                Assert.IsTrue(result.Annotations != null);
                Assert.IsTrue(result.Annotations[0].Rule == errEOF);
                Assert.IsTrue(result.Annotations[0].Start == 0);
                Assert.IsTrue(result.Annotations[0].End == testString.Length);
                Assert.IsTrue(result.Annotations[0].Children == null);
            }
        }

        /// <summary>
        /// Test invalid strings that have an opening delimiter but then
        /// reach the end-of-line (EOLN) before a closing delimiter is encountered.
        /// </summary>
        [TestMethod]
        public void InvalidEOLNTerminatedString_Parse_ExpectErrors()
        {
            var parser = new ParserBuilder().From(StringTokenizationText);
            var errEOLN = parser.Tokenizer.FindRule("log_err_string_eoln");

            var testConfigurations = new (string input, int expectedPosition, int expectedLength) []
            {
                ("\"foo\n\"", 0, 5),
                // 
                ("\"bar\r\n\"", 0, 6),
            };

            foreach (var testConfig in testConfigurations)
            {
                // turn off exception throwing for this test so we can test the error annotations
                var (result, _) = parser.Parse(testConfig.input, throwExceptionsOnError: false);

                Assert.IsTrue(result.FoundMatch);
                Assert.IsTrue(result.Annotations != null);
                Assert.IsTrue(result.Annotations[0].Rule == errEOLN);
                Assert.IsTrue(result.Annotations[0].Start == testConfig.expectedPosition);
                Assert.IsTrue(result.Annotations[0].Length == testConfig.expectedLength);
                Assert.IsTrue(result.Annotations[0].Children == null);
            }
        }

        /// <summary>
        /// Test is error recovery works, ie in an input with multiple strings, 
        /// errors and valid strings are found
        /// </summary>
        [TestMethod]
        public void MixOfValidAndInvalidStrings_Parse_ExpectFunctionIdsMatchExpectations()
        {
            var parser = new ParserBuilder().From(StringTokenizationText);
            var stringRule = parser.Tokenizer.FindRule("string");
            var errEOLN = parser.Tokenizer.FindRule("log_err_string_eoln");
            var errEOF = parser.Tokenizer.FindRule("err_string_eof");

            // modify the root to expect one or more strings
            parser.Tokenizer.Root = parser.Tokenizer.RegisterRule(
                new MatchCount<char>(
                    "#string_list", 
                    parser.Tokenizer.Root, 
                    production: IRule.Output.Children, 
                    min: 1, 
                    max: 0
                )
            );

            var testConfigurations = new (string input, int[] functionIds) []
            {
                ("\"foo\"", [stringRule.Id]),
                ("\"foo\n\"", [errEOLN.Id]),
                ("\"foo\n\"\"\"bar", [errEOLN.Id, stringRule.Id, errEOF.Id]),
            };

            foreach (var testConfig in testConfigurations)
            {
                // turn off exception throwing for this test so we can test the error annotations
                var (result, _) = parser.Parse(testConfig.input, throwExceptionsOnError: false);

                Assert.IsTrue(result.FoundMatch);
                Assert.IsTrue(result.Annotations != null);
                Assert.IsTrue(result.Annotations.Count == testConfig.functionIds.Length);

                Assert.IsTrue(result.Annotations.Select(a => a.Rule.Id)
                                .SequenceEqual(testConfig.functionIds));
            }
        }
    }
}


