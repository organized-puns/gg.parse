// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.util;
using Range = gg.parse.util.Range;

namespace gg.parse.rules
{
    public sealed class MatchCondition<T> : MetaRuleBase<T> where T : IComparable<T>
    {
        public MatchCondition(
            string name, 
            AnnotationPruning pruning, 
            int precedence,
            IRule subject
        ) : base(name, pruning, precedence, subject)
        {
        }

        public override ParseResult Parse(T[] input, int start)
        {
            Assertions.RequiresNotNull(Subject);

            var result = Subject.Parse(input, start);

            return result
                // is a lookahead, so length is always 0
                ? BuildResult(new Range(start, 0), result.Annotations)
                : ParseResult.Failure;
        }
        public override MatchCondition<T> CloneWithSubject(IRule subject) =>
            new(Name, Prune, Precedence, subject);
    }
}
