// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;

using Range = gg.parse.util.Range;

namespace gg.parse.rules
{
    public sealed class MatchRuleSequence<T> : RuleCompositionBase<T> where T : IComparable<T>
    {
        public MatchRuleSequence(string name, AnnotationPruning pruning, int precedence, params IRule[] rules) 
            : base(name, pruning, precedence, rules) 
        {
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

        public override MatchRuleSequence<T> CloneWithComposition(IEnumerable<IRule> composition) =>
            new (Name, Prune, Precedence, [..composition]);
    }
}
