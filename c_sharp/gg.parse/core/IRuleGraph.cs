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

    public interface IMutableRuleGraph<TData> : IRuleGraph<TData>, ICollection<IRule>
    {
        new IRule? Root { get; set; }

        TRule Register<TRule>(TRule rule) where TRule : IRule;

        TRule FindOrRegisterRuleAndSubRules<TRule>(TRule rule) where TRule : IRule;

        TRule ReplaceRule<TRule>(IRule original, TRule replacement) where TRule : IRule;
    }
}