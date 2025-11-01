// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.util;
using Range = gg.parse.util.Range;

namespace gg.parse.rules
{
    public sealed class MatchNot<T> : MetaRuleBase<T> where T : IComparable<T>
    {
        public MatchNot(string name, AnnotationPruning prune, int precedence, IRule rule)
            : base(name, prune, precedence, rule)
        {
        }

        public override ParseResult Parse(T[] input, int start)
        {
            Assertions.RequiresNotNull(Subject);

            return !Subject.Parse(input, start)
                ? BuildResult(new Range(start, 0))
                : ParseResult.Failure;
        }

        public override MatchNot<T> CloneWithSubject(IRule subject) =>
            new (Name, Prune, Precedence, subject);
    }
}
