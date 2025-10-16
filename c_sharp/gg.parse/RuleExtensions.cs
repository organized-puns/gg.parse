// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse
{
    public static class RuleExtensions
    {
        public static readonly string SelfToken = "";

        public static readonly string ChildrenToken = "#";

        public static readonly string VoidToken = "~";

        public static ParseResult Parse(this RuleBase<int> rule, IEnumerable<Annotation> tokens, int start = 0) =>
            rule.Parse(tokens.SelectRuleIds(), start);

        public static ParseResult Parse(this RuleBase<char> rule, string data, int start = 0) =>
            rule.Parse(data.ToCharArray(), start);

        

        public static string GetToken(this RuleOutput output) =>
            output switch
            {
                RuleOutput.Self => SelfToken,
                RuleOutput.Children => ChildrenToken,
                RuleOutput.Void => VoidToken,
                _ => throw new NotImplementedException(),
            };

        public static (string outputName, RuleOutput product) SplitNameAndOutput(this string name)
        {
            var product = RuleOutput.Self;
            var start = name.IndexOf(ChildrenToken);
            var length = 0;

            if (start == 0)
            {
                product = RuleOutput.Children;
                length = ChildrenToken.Length;
            }
            else if ((start = name.IndexOf(VoidToken)) == 0)
            {
                product = RuleOutput.Void;
                length = VoidToken.Length;
            }

            // take the substring of the name minus the output token
            return (name.Substring(Math.Max(0, start) + length).Trim(), product);
        }
    }
}
