namespace gg.parse.rules
{
    public class MatchNot<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        public RuleBase<T> Rule { get; private set; }

        public IEnumerable<RuleBase<T>> Rules => [Rule];

        public MatchNot(string name, IRule.Output production, int precedence, RuleBase<T> rule)
            : base(name, production, precedence)
        {
            Rule = rule;
        }

        public MatchNot(string name, RuleBase<T> rule)
            : base(name, IRule.Output.Self)
        {
            Rule = rule;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            var subRuleResult = Rule.Parse(input, start);

            if (!subRuleResult)
            {
                return BuildResult(new Range(start, 0));
            }

            return ParseResult.Failure;
        }
    }
}
