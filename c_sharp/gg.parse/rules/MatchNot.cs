// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.util;
using Range = gg.parse.util.Range;

namespace gg.parse.rules
{
    public sealed class MatchNot<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        public RuleBase<T> Rule { get; private set; }

        public IEnumerable<RuleBase<T>> Rules => [Rule];

        public int Count => 1;

        public RuleBase<T>? this[int index] => Rule;


        public MatchNot(string name, AnnotationPruning prune, int precedence, RuleBase<T> rule)
            : base(name, prune, precedence)
        {
            Rule = rule;
        }

        public MatchNot(string name, RuleBase<T> rule)
            : base(name, AnnotationPruning.None)
        {
            Rule = rule;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            var subRuleResult = Rule.Parse(input, start);

            if (!subRuleResult)
            {
                return BuildResult(new Range(start, 0));
            }

            return ParseResult.Failure;
        }

        public IRuleComposition<T> CloneWithComposition(IEnumerable<RuleBase<T>> composition) =>
            new MatchNot<T>(Name, Prune, Precedence, composition.First());

        public void MutateComposition(IEnumerable<RuleBase<T>> composition)
        {
            Assertions.RequiresNotNull(composition);
            Assertions.RequiresNotNull(composition.Count() == 1);

            Rule = composition.First();
        }
    }
}
