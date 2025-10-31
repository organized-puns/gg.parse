// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Diagnostics;

using gg.parse.rules;
using gg.parse.script;
using gg.parse.script.pipeline;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.doc.examples.test
{
    /// <summary>
    /// Used in MatchCondition documentation example.
    /// </summary>

    [TestClass]
    public sealed class MatchConditionTests
    {
        [TestMethod]
        public void MatchCondition_SuccessExample()
        {
            var fooRule = new MatchDataSequence<char>("match_foo", [.."foo"]);
            var isFooRule = new MatchCondition<char>("match_foo_condition", fooRule);

            IsTrue(isFooRule.Parse("foo", 0));
            // look-ahead does not return a length
            IsTrue(isFooRule.Parse("foo", 0).MatchLength == 0);
        }

        [TestMethod]
        public void MatchCondition_FailureExample()
        {
            var fooRule = new MatchDataSequence<char>("match_foo", [.."foo"]);
            var isFooRule = new MatchCondition<char>("match_foo_condition", fooRule);

            // input is empty
            IsFalse(isFooRule.Parse([], 0));

            // start position is beyond the input
            IsFalse(isFooRule.Parse("foo", 1));

            // bar is not foo
            IsFalse(isFooRule.Parse("bar", 1));
        }

        [TestMethod]
        public void MatchCondition_ScriptExample()
        {
            var builder = new ParserBuilder();
            var logger = new ScriptLogger()
            {
                Out = (level, message) => Debug.WriteLine($"[{level}]: {message}")
            };

            var tokenizer = builder.FromFile("assets/condition_example.tokens", logger: logger);
            
            // will output "foo found" 
            IsTrue(tokenizer.Tokenize("foo", processLogsOnResult: true));

            // will output "foo not found" 
            IsTrue(tokenizer.Tokenize("bar", processLogsOnResult: true));
        }
    }
}
