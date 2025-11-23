// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.util;

namespace gg.parse.tests.core
{
    [TestClass]
    public class StringExtensionsTest
    {
        [TestMethod]
        public void CreateStrings_CallSplitByCapital_ExpectMatchingOutcome()
        {
            var testCases = new[] {
                ("", ""),
                ("Abc", "abc"),
                ("AbcDef", "abc_def"),
                ("AbcDefGhi", "abc_def_ghi"),
                ("AbcDefGhiJ", "abc_def_ghi_j")
            };

            foreach (var (input, expectedOutput) in testCases)
            {
                var output = input.SplitOnCapitals(toLowerCase: true);
                Assert.IsTrue(output == expectedOutput);
            }
        }
    }
}
