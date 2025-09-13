namespace gg.parse
{
    public static class RuleExtensions
    {
        public static ParseResult Parse(this RuleBase<int> rule, IEnumerable<Annotation> tokens, int start = 0) =>
            rule.Parse(tokens.SelectRuleIds(), start);
    }
}
