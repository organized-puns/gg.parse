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


        /// Note: this will only return true because of the current assumption that the product character
        /// will always start at 0 and defaults to AnnotationProduct.Annotation. Should this change in the future
        /// we can more easily revert.
        public static bool TryGetOutput(this string name, out RuleOutput output, out int start, out int length)
        {
            output = RuleOutput.Self;
            length = 0;

            start = name.IndexOf(ChildrenToken);

            if (start == 0)
            {
                output = RuleOutput.Children;
                length = ChildrenToken.Length;
                return true;
            }

            start = name.IndexOf(VoidToken);

            if (start == 0)
            {
                output = RuleOutput.Void;
                length = VoidToken.Length;

                return true;
            }

            start = 0;

            return true;
        }

    }
}
