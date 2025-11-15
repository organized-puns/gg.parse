// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections;
using System.Collections.Immutable;

namespace gg.parse.core
{
    /// <summary>
    /// Immutable verion of the MutableRuleGraph
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class RuleGraph<T> : IRuleGraph<T>
    {
        private readonly ImmutableDictionary<string, IRule> _rules;
        
        public IRule this[string name] => _rules[name];

        public int Count => _rules.Count;

        public IRule Root
        {
            get;
            init;
        }

        public RuleGraph(IRule root, ImmutableDictionary<string, IRule> rules )
        {
            Root = root;
            _rules = rules;
        }

        public IEnumerable<string> RuleNames => _rules.Keys;

        public IEnumerator<IRule> GetEnumerator() =>
            _rules.Values.GetEnumerator();
        
        public ParseResult Parse(T[] data, int start = 0) =>
            Root.Parse(data, start);

        public bool TryFindRule(string name, out IRule? result) =>
            _rules.TryGetValue(name, out result);

        IEnumerator IEnumerable.GetEnumerator() =>
        
            GetEnumerator();
        
    }
}
