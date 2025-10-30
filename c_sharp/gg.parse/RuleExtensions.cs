// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;

namespace gg.parse
{
    public static class RuleExtensions
    {
        public static ParseResult Parse(this RuleBase<int> rule, ImmutableList<Annotation> tokens, int start = 0) =>
            rule.Parse(tokens.SelectRuleIds(), start);

        public static ParseResult Parse(this RuleBase<char> rule, string data, int start = 0) =>
            rule.Parse(data.ToCharArray(), start);
    }
}
