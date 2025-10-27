// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.rules
{
    public class MatchDataRange<T>(
        string name, 
        T minDataValue, 
        T maxDataValue, 
        AnnotationPruning output = AnnotationPruning.None,
        int precedence = 0
    )
        : RuleBase<T>(name, output, precedence) where T : IComparable<T>
    {
        public T MinDataValue { get; } = minDataValue;

        public T MaxDataValue { get; } = maxDataValue;

        public override ParseResult Parse(T[] input, int start)
        {
            if (start < input.Length)
            {
                if (input[start].CompareTo(MinDataValue) >= 0 && input[start].CompareTo(MaxDataValue) <= 0)
                {
                    return BuildDataRuleResult(new(start, 1));
                }
            }

            return ParseResult.Failure;
        }
    }
}
