// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.util;
using Range = gg.parse.util.Range;

namespace gg.parse.rules
{
    public sealed class MatchCount<T> : MetaRuleBase<T> where T : IComparable<T>
    {
        public int Min { get; init; }
        
        public int Max { get; init; } 
                
        public MatchCount(string name,
            AnnotationPruning pruning,
            int precedence,
            IRule subject,
            int min = 1,
            int max = 1)
            : base(name, pruning, precedence, subject)
        {
            Assertions.RequiresNotNull(subject);

            Min = min;
            Max = max;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            int count = 0;
            int index = start;
            List<Annotation>? children = null;

            while (index < input.Length && (Max <= 0 || count < Max))
            {
                var result = Subject!.Parse(input, index);
                
                if (!result)
                {
                    break;
                }
        
                if (result.MatchLength == 0 && Max <= 0)
                {
                    throw new InvalidProgramException($"Rule {Name} detected an infinite loop with its subrule {Subject.Name}.");
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
                ? BuildResult(new Range(start, index - start), children == null ? null : [..children])
                : ParseResult.Failure;
        }

        public override MatchCount<T> CloneWithSubject(IRule subject) =>
            new(Name, Prune, Precedence, subject, Min, Max);
    }
}
