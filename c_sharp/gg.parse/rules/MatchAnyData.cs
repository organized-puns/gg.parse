// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun


using Range = gg.parse.util.Range;

namespace gg.parse.rules
{
    public sealed class MatchAnyData<T>(
        string name, 
        AnnotationPruning output = AnnotationPruning.None, 
        int precedence = 0
     ) : RuleBase<T>(name, output, precedence) where T : IComparable<T>
    {
        public override ParseResult Parse(T[] input, int start)
        {
            if (start < input.Length)
            {
                return BuildDataRuleResult(new Range(start, 1));
            }

            return ParseResult.Failure;
        }
    }
}
