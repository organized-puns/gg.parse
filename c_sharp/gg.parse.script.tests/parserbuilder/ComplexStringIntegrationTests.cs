// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.rules;

namespace gg.parse.script.tests.parserbuilder
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
            var parser = new ParserBuilder().From(StringTokenizationText).Build();
            var stringRule = parser.Tokens["string"];

            var validStrings = new string[]
            {
                "\"foo\"",
                "\"_123#\"",
                "\"\""
            };

            foreach (var validString in validStrings)
            {
                var result = parser.Tokenize(validString);

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
            var parser = new ParserBuilder().From(StringTokenizationText).Build();
            var errEOF = parser.Tokens["err_string_eof"];

            var invalidStrings = new string[]
            {
                "\"foo",
                "\"",
            };

            foreach (var testString in invalidStrings)
            {
                // turn off exception throwing for this test so we can test the error annotations
                var result = parser.Tokenize(testString, throwExceptions: false);

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
            var parser = new ParserBuilder().From(StringTokenizationText).Build();
            var errEOLN = parser.Tokens["log_err_string_eoln"];

            var testConfigurations = new (string input, int expectedPosition, int expectedLength) []
            {
                ("\"foo\n\"", 0, 5),
                ("\"bar\r\n\"", 0, 6),
            };

            foreach (var (input, expectedPosition, expectedLength) in testConfigurations)
            {
                // turn off exception throwing for this test so we can test the error annotations
                var result = parser.Tokenize(input, throwExceptions: false);

                Assert.IsTrue(result.FoundMatch);
                Assert.IsTrue(result.Annotations != null);
                Assert.IsTrue(result.Annotations[0].Rule == errEOLN);
                Assert.IsTrue(result.Annotations[0].Start == expectedPosition);
                Assert.IsTrue(result.Annotations[0].Length == expectedLength);
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
            var builder = new ParserBuilder().From(StringTokenizationText);
            
            // modify the root to expect one or more strings
            builder.TokenGraph.Root = builder.TokenGraph.Register(
                new MatchCount<char>(
                    "#string_list",
                    AnnotationPruning.Root,
                    0,
                    builder.TokenGraph.Root, 
                    min: 1, 
                    max: 0
                )
            );

            var parser = builder.Build();
            var stringRule = parser.Tokens["string"];
            var errEOLN = parser.Tokens["log_err_string_eoln"];
            var errEOF = parser.Tokens["err_string_eof"];


            var testConfigurations = new (string input, int[] functionIds) []
            {
                ("\"foo\"", [stringRule.Id]),
                ("\"foo\n\"", [errEOLN.Id]),
                ("\"foo\n\"\"\"bar", [errEOLN.Id, stringRule.Id, errEOF.Id]),
            };

            foreach (var (input, functionIds) in testConfigurations)
            {
                // turn off exception throwing for this test so we can test the error annotations
                var result = parser.Tokenize(input, throwExceptions: false);

                Assert.IsTrue(result.FoundMatch);
                Assert.IsTrue(result.Annotations != null);
                Assert.IsTrue(result.Annotations.Count == functionIds.Length);

                Assert.IsTrue(result.Annotations.Select(a => a.Rule.Id)
                                .SequenceEqual(functionIds));
            }
        }
    }
}


