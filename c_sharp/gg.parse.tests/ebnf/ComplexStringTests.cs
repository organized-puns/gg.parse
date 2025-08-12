using gg.parse.ebnf;
using gg.parse.rulefunctions;
using gg.parse.rulefunctions.rulefunctions;

namespace gg.parse.tests.ebnf
{
    /// <summary>
    /// Various test based on a somewhat complex string tokenization ebnf, specifically
    /// working towards checking if error
    /// </summary>
    [TestClass]
    public class ComplexStringTests
    {
        /// <summary>
        /// Read the string_tokenization.ebnf ebnf and create a tokenizer. Validate correctly 
        /// formed strings are parsed correctly
        /// that this 
        /// </summary>
        [TestMethod]
        public void ValidString_Parse_ExpectNoErrors()
        {
            var tokenizer = Setup_BuildStringTokenizerFromEbnfFile();
            var stringRule = tokenizer.FindRule("string");

            var validStrings = new string[]
            {
                "\"foo\"",
                "\"_123#\"",
                "\"\""
            };

            foreach (var validString in validStrings)
            {
                var result = tokenizer.Root.Parse(validString.ToArray(), 0);

                Assert.IsTrue(result.FoundMatch);
                Assert.IsTrue(result.Annotations != null);
                Assert.IsTrue(result.Annotations[0].FunctionId == stringRule.Id);
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
            var tokenizer   = Setup_BuildStringTokenizerFromEbnfFile();
            var errEOF      = tokenizer.FindRule("err_string_eof");

            var invalidStrings = new string[]
            {
                "\"foo",
                "\"",
            };

            foreach (var validString in invalidStrings)
            {
                var result = tokenizer.Root.Parse(validString.ToArray(), 0);

                Assert.IsTrue(result.FoundMatch);
                Assert.IsTrue(result.Annotations != null);
                Assert.IsTrue(result.Annotations[0].FunctionId == errEOF.Id);
                Assert.IsTrue(result.Annotations[0].Start == validString.Length);
                Assert.IsTrue(result.Annotations[0].End == validString.Length);
                Assert.IsTrue(result.Annotations[0].Length == 0);
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
            var tokenizer = Setup_BuildStringTokenizerFromEbnfFile();
            var errEOLN = tokenizer.FindRule("err_string_eoln");

            var testConfigurations = new (string input, int expectedPosition, int expectedLength) []
            {
                ("\"foo\n\"", 4, 2),
                // 
                ("\"bar\r\n\"", 4, 3),
                // expected length = 1 because there is a valid string ("", before bar) right there
                ("\"foo\n\"\"bar\"", 4, 1)
            };

            foreach (var testConfig in testConfigurations)
            {
                var result = tokenizer.Root.Parse(testConfig.input.ToArray(), 0);

                Assert.IsTrue(result.FoundMatch);
                Assert.IsTrue(result.Annotations != null);
                Assert.IsTrue(result.Annotations[0].FunctionId == errEOLN.Id);
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
            var tokenizer = Setup_BuildStringTokenizerFromEbnfFile();
            var stringRule = tokenizer.FindRule("string");
            var errEOLN = tokenizer.FindRule("err_string_eoln");
            var errEOF = tokenizer.FindRule("err_string_eof");

            // modify the root to expect one or more strings
            tokenizer.Root = tokenizer.RegisterRule(new MatchFunctionCount<char>("#string_list", tokenizer.Root, production: AnnotationProduct.Transitive, min: 1, max: 0));

            var testConfigurations = new (string input, int[] functionIds) []
            {
                ("\"foo\"", [stringRule.Id]),
                ("\"foo\n\"", [errEOLN.Id]),
                ("\"foo\n\"\"\"bar", [errEOLN.Id, stringRule.Id, errEOF.Id]),
            };

            foreach (var testConfig in testConfigurations)
            {
                var result = tokenizer.Root.Parse(testConfig.input.ToArray(), 0);

                Assert.IsTrue(result.FoundMatch);
                Assert.IsTrue(result.Annotations != null);
                Assert.IsTrue(result.Annotations.Count == testConfig.functionIds.Length);

                Assert.IsTrue(result.Annotations.Select(a => a.FunctionId)
                                .SequenceEqual(testConfig.functionIds));
            }
        }

        private static RuleGraph<char> Setup_BuildStringTokenizerFromEbnfFile()
        {
            var text = File.ReadAllText("assets/string_tokenization.ebnf");
            var tokenizer = new EbnfParser(text, null).EbnfTokenizer;

            // check everything has build correctly
            Assert.IsTrue(tokenizer != null);
            Assert.IsTrue(tokenizer.Root != null);

            // check all expected rules are there
            var stringOptionsRule = tokenizer.FindRule("string_options");
            Assert.IsTrue(stringOptionsRule != null);
            Assert.IsTrue(tokenizer.Root == stringOptionsRule);

            var stringRule = tokenizer.FindRule("string");
            Assert.IsTrue(stringRule != null);

            var errEOF = tokenizer.FindRule("err_string_eof") as MarkError<char>;
            Assert.IsTrue(errEOF != null);

            var errEOLN = tokenizer.FindRule("err_string_eoln") as MarkError<char>;
            Assert.IsTrue(errEOF != null);
        
            return tokenizer;
        }
    }
}


