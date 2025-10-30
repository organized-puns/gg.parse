// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.util;
using System.Collections.Immutable;

namespace gg.parse.rules
{
    public class MatchOneOf<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        private RuleBase<T>[] _options;

        public RuleBase<T>? this[int index] => _options[index];
        
        public int Count => _options.Length;

        public IEnumerable<RuleBase<T>> Rules => _options;

        public MatchOneOf(string name, params RuleBase<T>[] options)
            : base(name, AnnotationPruning.None)
        {
            _options = options;
        }

        public MatchOneOf(
            string name, 
            AnnotationPruning output, 
            int precedence = 0, 
            params RuleBase<T>[] rules
        ) : base(name, output, precedence)
        {
            _options = rules;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            foreach (var option in _options)
            {   
                var result = option.Parse(input, start);
                if (result.FoundMatch)
                {
                    //ImmutableList<Annotation>? children = result.Annotations == null || result.Annotations.Count == 0
                      //      ? null 
                            //: [..result.Annotations!];

                    return BuildResult(new(start, result.MatchLength), result.Annotations);
                }
            }
            return ParseResult.Failure;
        }

        public IRuleComposition<T> CloneWithComposition(IEnumerable<RuleBase<T>> composition) =>
            new MatchOneOf<T>(Name, Prune, Precedence, [..composition]);

        public void MutateComposition(IEnumerable<RuleBase<T>> composition)
        {
            Assertions.RequiresNotNull(composition);
            Assertions.Requires(!composition.Any( r => r == null));

            _options = [.. composition];
        }
    }
}
