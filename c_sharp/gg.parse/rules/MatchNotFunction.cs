namespace gg.parse.rules
{
    public class MatchNotFunction<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        public RuleBase<T> Rule { get; private set; }

        public IEnumerable<RuleBase<T>> Rules => [Rule];

        public MatchNotFunction(string name, IRule.Output production, RuleBase<T> rule, int precedence = 0)
            : base(name, production, precedence)
        {
            Rule = rule;
        }

        public MatchNotFunction(string name, RuleBase<T> rule)
            : base(name, IRule.Output.Self)
        {
            Rule = rule;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            var result = Rule.Parse(input, start);

            if (!result.FoundMatch)
            {
                return BuildFunctionRuleResult(new Range(start, 0));
            }

            return ParseResult.Failure;
        }
    }
}
