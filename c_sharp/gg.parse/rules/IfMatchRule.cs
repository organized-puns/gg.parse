namespace gg.parse.rules
{
    public class IfMatchRule<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        public RuleBase<T> Rule { get; private set; }

        public IEnumerable<RuleBase<T>> Rules => [Rule];

        public IfMatchRule(
            string name, 
            IRule.Output production, 
            RuleBase<T> rule, 
            int precedence = 0
        ) : base(name, production, precedence)
        {
            Assertions.RequiresNotNull(rule);

            Rule = rule;
        }

        public IfMatchRule(string name, RuleBase<T> rule)
            : base(name, IRule.Output.Self)
        {
            Assertions.RequiresNotNull(rule);

            Rule = rule;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            var result = Rule.Parse(input, start);

            if (result.FoundMatch)
            {
                return BuildFunctionRuleResult(new Range(start, 0), result.Annotations);
            }

            return ParseResult.Failure;
        }
    }
}
