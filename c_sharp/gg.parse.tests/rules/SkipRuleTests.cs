// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.rules;

using Range = gg.parse.util.Range;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.tests.rules
{
    [TestClass]
    public class SkipRuleTests
    {
        [TestMethod]
        public void CreateSkipUntilFoo_Parse_ExpectSkippedUntilFoo()
        {
            var isFoo = new MatchDataSequence<char>("isFoo", "foo".ToCharArray());
            var skipRule = new SkipRule<char>("testSkip", AnnotationPruning.None, 0, isFoo);
            var result = skipRule.Parse("abcfoo");

            IsTrue(result.FoundMatch);
            IsTrue(result.MatchLength == 3);
            IsTrue(result[0]!.Rule == skipRule);
            IsTrue(result[0]!.Range.Equals(new Range(0, 3)));
        }

        [TestMethod]
        public void CreateSkipUntilFoo_Parse_ExpectFailureBecauseEoF()
        {
            var isFoo = new MatchDataSequence<char>("isFoo", "foo".ToCharArray());
            var skipRule = new SkipRule<char>("testSkip", AnnotationPruning.None, 0, isFoo);
            var result = skipRule.Parse("abc");

            IsFalse(result.FoundMatch);
        }

        [TestMethod]
        public void CreateSkipUntilFoo_Parse_ExpectSucceedDespiteEof()
        {
            var isFoo = new MatchDataSequence<char>("isFoo", "foo".ToCharArray());
            var skipRule = new SkipRule<char>("testSkip", AnnotationPruning.None, 0, isFoo, failOnEof: false);
            var result = skipRule.Parse("abcfo");

            IsTrue(result.FoundMatch);
            IsTrue(result.MatchLength == 5);
            IsTrue(result[0]!.Rule == skipRule);
            IsTrue(result[0]!.Range.Equals(new Range(0, 5)));
        }

        [TestMethod]
        public void CreateSkipUntilFooStopAfterCondition_Parse_ExpectDataPointerAfterCondition()
        {
            var text = "abcfoo";
            var isFoo = new MatchDataSequence<char>("isFoo", "foo".ToCharArray());
            var skipRule = new SkipRule<char>("testSkip", AnnotationPruning.None, 0, isFoo, stopBeforeCondition: false);
            var result = skipRule.Parse(text);

            IsTrue(result.FoundMatch);
            IsTrue(result.MatchLength == text.Length);
            IsTrue(result[0]!.Rule == skipRule);
            IsTrue(result[0]!.Range.Equals(new Range(0, text.Length)));
        }
    }
}
