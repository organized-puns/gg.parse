// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.util;

namespace gg.parse.rules
{
    public sealed class MatchOneOf<T> : RuleBase<T>, IRuleComposition where T : IComparable<T>
    {
        private IRule[] _options;

        public IRule? this[int index] => _options[index];
        
        public int Count => _options.Length;

        public IEnumerable<IRule> Rules => _options;

        public MatchOneOf(
            string name, 
            AnnotationPruning pruning, 
            int precedence, 
            params IRule[] rules
        ) : base(name, pruning, precedence)
        {
            _options = rules;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            foreach (var option in _options)
            {   
                var result = option.Parse(input, start);
                
                if (result)
                {                
                    return BuildResult(new(start, result.MatchLength), result.Annotations);
                }
            }

            return ParseResult.Failure;
        }

        public IRuleComposition CloneWithComposition(IEnumerable<IRule> composition) =>
            new MatchOneOf<T>(Name, Prune, Precedence, [..composition]);

        public void MutateComposition(IEnumerable<IRule> composition)
        {
            Assertions.RequiresNotNull(composition);
            Assertions.Requires(!composition.Any( r => r == null));

            _options = [.. composition];
        }
    }
}
