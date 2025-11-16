// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun


namespace gg.parse.core
{
    public interface IRuleGraph<TData> : IEnumerable<IRule> 
    {
        IRule this[string name] { get; }

        IRule? Root { get; }

        IEnumerable<string> RuleNames { get; }
               
        ParseResult Parse(TData[] data, int start = 0);
        
        bool TryFindRule(string name, out IRule? result);
    }
}