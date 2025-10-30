// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.util;

using Range = gg.parse.util.Range;

namespace gg.parse.rules
{
    public sealed class MatchRuleSequence<T> : RuleBase<T>, IRuleComposition<T>  where T : IComparable<T>
    {
        private RuleBase<T>[] _rules;

        public RuleBase<T>? this[int index] => _rules[index];
        
        public int Count => _rules.Length;

        public IEnumerable<RuleBase<T>> Rules => _rules;

        public MatchRuleSequence(
            string name, 
            AnnotationPruning pruning, 
            int precedence = 0,
            params RuleBase<T>[] rules
        ) : base(name, pruning, precedence) 
        {
            _rules = rules;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            var index = start;
            List<Annotation>? children = null;

            foreach (var rule in Rules)
            {
                var result = rule.Parse(input, index);
                
                if (!result.FoundMatch)
                {
                    return ParseResult.Failure;
                }

                if (result.Annotations != null && result.Annotations.Count > 0 &&
                   (Prune == AnnotationPruning.None || Prune == AnnotationPruning.Root))
                {
                    children ??= [];
                    children.AddRange(result.Annotations);
                }

                index += result.MatchLength;
            }

            return BuildResult(new Range(start, index - start), children == null ? null : [.. children]);
        }

        public IRuleComposition<T> CloneWithComposition(IEnumerable<RuleBase<T>> composition) =>
            new MatchRuleSequence<T>(Name, Prune, Precedence, [..composition]);

        public void MutateComposition(IEnumerable<RuleBase<T>> composition)
        {
            Assertions.RequiresNotNull(composition);
            Assertions.Requires(!composition.Any(r => r == null));

            _rules = [.. composition];
        }
    }
}
