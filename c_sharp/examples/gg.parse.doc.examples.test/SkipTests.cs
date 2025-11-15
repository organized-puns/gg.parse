// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Diagnostics;
using gg.parse.core;
using gg.parse.rules;
using gg.parse.script;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.doc.examples.test
{
    /// <summary>
    /// Used in Skip documentation example.
    /// </summary>

    [TestClass]
    public sealed class SkipTests
    {
        [TestMethod]
        public void FindStopAtDifferences_Examples()
        {
            var fooRule = new MatchDataSequence<char>("match_foo", [.."foo"]);
            var findFoo = new SkipRule<char>("find_foo", AnnotationPruning.None, 0, 
                                                fooRule, failOnEof: true, stopBeforeCondition: true);
            var stopAtFoo = new SkipRule<char>("stopAt_foo", AnnotationPruning.None, 0,
                                                fooRule, failOnEof: false, stopBeforeCondition: true);


            var result = findFoo.Parse("barfoo", 0);

            IsTrue(result);

            // skipped until we find 'foo' at position 3
            IsTrue(result.MatchLength == 3);

           result = stopAtFoo.Parse("barfoo", 0);

            // same result as findFoo
            IsTrue(result);
            IsTrue(result.MatchLength == 3);

            result = findFoo.Parse("bar", 0);

            // no foo found in this bar
            IsFalse(result);

            result = stopAtFoo.Parse("bar", 0);

            // no foo found but we do not fail on eof
            IsTrue(result);
            IsTrue(result.MatchLength == 3);
        }

        [TestMethod]
        public void FindStopAtDifferences_ScriptExamples()
        {
            var script = 
                "find_foo = find 'foo';"
                + "stop_at_foo = stop_at 'foo';"
                + "stop_after_foo = stop_after 'foo';";

            var tokenizer = new ParserBuilder().From(script).TokenGraph;

            var result = tokenizer["find_foo"].Parse("barfoo", 0);
            
            IsTrue(result);
            IsTrue(result.MatchLength == 3);
            IsTrue(result.Annotations[0].Children == null);

            result = tokenizer["stop_at_foo"].Parse("barfoo", 0);

            IsTrue(result);
            IsTrue(result.MatchLength == 3);
            IsTrue(result.Annotations[0].Children == null);

            result = tokenizer["stop_after_foo"].Parse("barfoo", 0);

            IsTrue(result);
            IsTrue(result.MatchLength == 6);
            // stop after with will have captured the 'foo' match as well
            // note this will ONLY happen if the rule is a top level rule
            // inline rules will prune all
            IsTrue(result.Annotations[0].Children != null);
            IsTrue(result[0][0].Rule is MatchDataSequence<char>);
        }
    }
}
