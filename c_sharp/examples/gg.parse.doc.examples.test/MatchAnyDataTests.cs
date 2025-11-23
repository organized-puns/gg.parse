// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.rules;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.doc.examples.test
{
    /// <summary>
    /// Used in MatchAnyData documentation example.
    /// </summary>

    [TestClass]
    public sealed class MatchAnyDataTests
    {
        [TestMethod]
        public void MatchSimpleData_SuccessExample()
        {
            var rule = new MatchAnyData<char>("match_any_example");

            IsTrue(rule.Parse(['a'], 0));
            IsTrue(rule.Parse(['1'], 0));
            IsTrue(rule.Parse(['%'], 0));
        }

        [TestMethod]
        public void MatchSimpleData_FailureExample()
        {
            var rule = new MatchAnyData<char>("match_any_example");

            // input is empty
            IsFalse(rule.Parse([], 0));

            // start position is beyond the input
            IsFalse(rule.Parse(['1'], 1));
        }
    }
}
