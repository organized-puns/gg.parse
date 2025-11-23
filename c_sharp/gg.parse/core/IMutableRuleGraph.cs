// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.core
{
    public interface IMutableRuleGraph<TData> : IRuleGraph<TData>, ICollection<IRule>
    {
        new IRule? Root { get; set; }

        TRule Register<TRule>(TRule rule) where TRule : IRule;

        TRule FindOrRegisterRuleAndSubRules<TRule>(TRule rule) where TRule : IRule;

        TRule ReplaceRule<TRule>(IRule original, TRule replacement) where TRule : IRule;
    }
}