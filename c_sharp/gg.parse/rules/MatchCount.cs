// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.util;
using Range = gg.parse.util.Range;

namespace gg.parse.rules
{
    public class MatchCount<T>(
        string name, 
        RuleBase<T> rule, 
        AnnotationPruning output = AnnotationPruning.None, 
        int min = 1, 
        int max = 1, 
        int precedence = 0
    ) : RuleBase<T>(name, output, precedence), IRuleComposition<T> where T : IComparable<T>
    {
        public RuleBase<T> Rule { get; private set; } = rule;
        
        public int Min { get; } = min;
        
        public int Max { get; } = max;

        public IEnumerable<RuleBase<T>> Rules => [Rule];

        public int Count => 1;

        public RuleBase<T>? this[int index]
        {
            get => Rule;
            /*set
            {
                Assertions.RequiresNotNull(value);
                Rule = value;
            }*/
        }

        public override ParseResult Parse(T[] input, int start)
        {
            int count = 0;
            int index = start;
            List<Annotation>? children = null;

            while (index < input.Length && (Max <= 0 || count < Max))
            {
                var result = Rule.Parse(input, index);
                
                if (!result)
                {
                    break;
                }
        
                if (result.MatchLength == 0 && Max <= 0)
                {
                    throw new InvalidProgramException($"Rule {Name} detected an infinite loop with its subrule {Rule.Name}.");
                }

                count++;
                index += result.MatchLength;

                if (result.Annotations != null && result.Annotations.Count > 0 &&
                    (Prune == AnnotationPruning.None || Prune == AnnotationPruning.Root))
                {
                    children ??= [];
                    children.AddRange(result.Annotations);
                }
            }

            return Min <= 0 || count >= Min
                ? BuildResult(new Range(start, index - start), children)
                : ParseResult.Failure;
        }

        public IRuleComposition<T> CloneWithComposition(IEnumerable<RuleBase<T>> composition)
        {
            return new MatchCount<T>(
                Name, 
                composition.First(), 
                Prune, 
                Min, 
                Max, 
                Precedence
            );
        }
    }
}
