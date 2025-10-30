// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using Range = gg.parse.util.Range;

namespace gg.parse.rules
{
    public sealed class MatchDataSet<T> : RuleBase<T> where T : IComparable<T>
    {
        public T[] MatchingValues { get; init; }

        public MatchDataSet(string name, AnnotationPruning output, T[] setValues, int precedence = 0)
            : base(name, output, precedence)
        {
            MatchingValues = setValues;
        }

        public MatchDataSet(string name, T[] matchingValues, int precedence = 0)
            : base(name, AnnotationPruning.None, precedence)
        {
            MatchingValues = matchingValues;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            if (start < input.Length)
            {
                if (MatchingValues.Contains(input[start]))
                {
                    return BuildDataRuleResult(new Range(start, 1));
                }
            }

            return ParseResult.Failure;
        }
    }
}
