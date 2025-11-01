// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;
using System.Diagnostics;

namespace gg.parse.core
{
    public static class RuleExtensions
    {
        public static ParseResult Parse(this IRule rule, ImmutableList<Annotation> tokens, int start = 0) =>
            rule.Parse(tokens.SelectRuleIds(), start);

        public static ParseResult Parse(this RuleBase<char> rule, string data, int start = 0) =>
            rule.Parse(data.ToCharArray(), start);

        [DebuggerStepThrough]
        public static ParseResult Parse(this IRule rule, string data, int start = 0) =>
            rule.Parse(data.ToCharArray(), start);
    }
}
