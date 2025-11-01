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
    /// Used in MatchAnyData documentation example.   
    /// </summary>

    [TestClass]
    public sealed class MatchCountTests
    {
        [TestMethod]
        public void MatchSimpleData_SuccessExample()
        {
            var fooRule = new MatchDataSequence<char>("match_foo", [.."foo"]);
            var match3Foos = new MatchCount<char>("match_three_foos", AnnotationPruning.None, 0, fooRule, min:3, max:3);

            // must get 3 foos on the input
            IsTrue(match3Foos.Parse("foofoofoo"));

            // not less
            IsFalse(match3Foos.Parse("foofoo"));
        }

        [TestMethod]
        public void MatchZeroOrMore_SuccessExample()
        {
            var fooRule = new MatchDataSequence<char>("match_foo", [.. "foo"]);
            var matchZeroOrMoreFoos = new MatchCount<char>("match_*_foos", AnnotationPruning.None, 0, fooRule, min: 0, max: 0);

            // basically anything is fine for matchZeroOrMoreFoos, it will always succeed
            // the only thing that will differ is the match length
            IsTrue(matchZeroOrMoreFoos.Parse(""));
            IsTrue(matchZeroOrMoreFoos.Parse("").MatchLength == 0);

            IsTrue(matchZeroOrMoreFoos.Parse("bar"));
            IsTrue(matchZeroOrMoreFoos.Parse("bar").MatchLength == 0);

            IsTrue(matchZeroOrMoreFoos.Parse("foo"));
            IsTrue(matchZeroOrMoreFoos.Parse("foo").MatchLength == 3);

            IsTrue(matchZeroOrMoreFoos.Parse("foofoo"));
            IsTrue(matchZeroOrMoreFoos.Parse("foofoo").MatchLength == 6);
        }

        [TestMethod]
        public void MatchOneOrMore_SuccessExample()
        {
            var fooRule = new MatchDataSequence<char>("match_foo", [.. "foo"]);
            var matchOneOrMoreFoos = new MatchCount<char>("match_+_foos", AnnotationPruning.None, 0, fooRule, min: 1, max: 0);

            // one or more must have at least one foo on the input to succeed
            IsFalse(matchOneOrMoreFoos.Parse(""));
            
            IsTrue(matchOneOrMoreFoos.Parse("foo"));
            IsTrue(matchOneOrMoreFoos.Parse("foo").MatchLength == 3);

            IsTrue(matchOneOrMoreFoos.Parse("foofoo"));
            IsTrue(matchOneOrMoreFoos.Parse("foofoo").MatchLength == 6);
        }

        [TestMethod]
        public void MatchCount_ScriptExample()
        {
            var logger = new ScriptLogger((level, message) => Debug.WriteLine($"[{level}]: {message}"));
            var tokenizer = new ParserBuilder()
                            .FromFile("assets/match_count_example.tokens", logger: logger);

            // will output "zero or more foos found"
            IsTrue(tokenizer.Tokenize("foofoo", usingRule: "match_zero_or_more_foos", processLogsOnResult: true));

            // will also output "zero or more foos found"
            IsTrue(tokenizer.Tokenize(
                "barbazqazquad", 
                usingRule: "match_zero_or_more_foos", 
                processLogsOnResult: true
            ));

            // will output "zero or one foo found"
            IsTrue(tokenizer.Tokenize("foofoo", usingRule: "match_zero_or_one_foo", processLogsOnResult: true));

            // will output "one or more foos found"
            IsTrue(tokenizer.Tokenize("foo", usingRule: "match_one_or_more_foos", processLogsOnResult: true));

            // will output "no foos found :("
            IsTrue(tokenizer.Tokenize("bar", usingRule: "match_one_or_more_foos", processLogsOnResult: true));
        }
    }
}
