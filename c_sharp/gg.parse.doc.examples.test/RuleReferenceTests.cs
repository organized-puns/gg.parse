// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.rules;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.doc.examples.test
{
    /// <summary>
    /// Used in RuleReference documentation example.   
    /// </summary>

    [TestClass]
    public sealed class RuleReferenceTests
    {
        /// <summary>
        /// Exceptionally trivial example of using a rule reference to refer to another rule.
        /// </summary>
        [TestMethod]
        public void RuleReference_SuccessExample()
        {
            var fooRule = new MatchDataSequence<char>("match_foo", [.."foo"]);
            var matchFooByReference = new RuleReference<char>("ref_match_foo", AnnotationPruning.None, 0, "match_foo");

            // note when using references in a script, the compiler will automatically
            // resolve this
            matchFooByReference.MutateSubject(fooRule);
            
            IsTrue(matchFooByReference.Parse("foo"));
        }
    }
}
