// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;

namespace gg.parse.rules
{
    public sealed class MatchOneOf<T> : RuleCompositionBase<T> where T : IComparable<T>
    {        
        public MatchOneOf(string name, AnnotationPruning pruning, int precedence, params IRule[] rules) 
            : base(name, pruning, precedence, rules)
        {
        }

        public override ParseResult Parse(T[] input, int start)
        {
            foreach (var option in _rules)
            {   
                var result = option.Parse(input, start);
                
                if (result)
                {                
                    return BuildResult(new(start, result.MatchLength), result.Annotations);
                }
            }

            return ParseResult.Failure;
        }

        public override MatchOneOf<T> CloneWithComposition(IEnumerable<IRule> composition) =>
            new (Name, Prune, Precedence, [..composition]);
    }
}
