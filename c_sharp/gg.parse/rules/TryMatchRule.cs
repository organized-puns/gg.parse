namespace gg.parse.rules
{
    public class TryMatchRule<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        public RuleBase<T> Rule { get; private set; }

        public IEnumerable<RuleBase<T>> Rules => [Rule];

        public TryMatchRule(
            string name, 
            AnnotationProduct production, 
            RuleBase<T> rule, 
            int precedence = 0
        ) : base(name, production, precedence)
        {
            Assertions.RequiresNotNull(rule);

            Rule = rule;
        }

        public TryMatchRule(string name, RuleBase<T> rule)
            : base(name, AnnotationProduct.Annotation)
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
