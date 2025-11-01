// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.util;

namespace gg.parse.core
{
    public abstract class RuleCompositionBase<T> : RuleBase<T>, IRuleComposition where T : IComparable<T>
    {
        protected IRule[] _rules;

        public IRule? this[int index] => _rules[index];

        public int Count => _rules.Length;

        public IEnumerable<IRule> Rules => _rules;

        public RuleCompositionBase(string name, AnnotationPruning pruning, int precedence, params IRule[] rules)
            : base(name, pruning, precedence)
        {
            _rules = rules;
        }

        public abstract IRuleComposition CloneWithComposition(IEnumerable<IRule> composition);
            
        public void MutateComposition(IEnumerable<IRule> composition)
        {
            Assertions.RequiresNotNull(composition);
            Assertions.Requires(!composition.Any(r => r == null));

            _rules = [.. composition];
        }


    }
}
