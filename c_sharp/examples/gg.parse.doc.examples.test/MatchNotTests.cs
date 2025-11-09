// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Diagnostics;

using gg.parse.core;
using gg.parse.rules;
using gg.parse.script;
using gg.parse.script.parser;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.doc.examples.test
{
    /// <summary>
    /// Used in MatchNot documentation example.
    /// </summary>

    [TestClass]
    public sealed class MatchNotTests
    {
        [TestMethod]
        public void MatchNot_SuccessExample()
        {
            var fooRule = new MatchDataSequence<char>("match_foo", [.."foo"]);
            var isNotFooRule = new MatchNot<char>("match_foo_condition", AnnotationPruning.None, 0, fooRule);
            
            IsTrue(isNotFooRule.Parse("bar", 0));
            // look-ahead does not return a length
            IsTrue(isNotFooRule.Parse("bar", 0).MatchLength == 0);
        }

        [TestMethod]
        public void MatchNot_FailureExample()
        {
            var fooRule = new MatchDataSequence<char>("match_foo", [.. "foo"]);
            var isNotFooRule = new MatchNot<char>("match_foo_condition", AnnotationPruning.None, 0, fooRule);

            IsFalse(isNotFooRule.Parse("foo", 0));
        }

        [TestMethod]
        public void MatchNot_ScriptExample()
        {
            var logger = new ScriptLogger(output: (level, message) => Debug.WriteLine($"[{level}]: {message}"));
            var tokenizer = new ParserBuilder().From("root = if !'foo', info 'not foo';", logger: logger);
            
            // will output "not foo " 
            IsTrue(tokenizer.Tokenize("bar", processLogsOnResult: true));

            // will output nothing
            IsFalse(tokenizer.Tokenize("foo", processLogsOnResult: true));
        }
    }
}
