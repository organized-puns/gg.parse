
namespace gg.parse.rulefunctions
{
    public class MatchNotFunction<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        public RuleBase<T> Rule { get; init; }

        public IEnumerable<RuleBase<T>> SubRules => [Rule];

        public MatchNotFunction(string name, AnnotationProduct production, RuleBase<T> rule)
            : base(name, production)
        {
            Rule = rule;
        }

        public MatchNotFunction(string name, RuleBase<T> rule)
            : base(name, AnnotationProduct.Annotation)
        {
            Rule = rule;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            var result = Rule.Parse(input, start);

            if (!result.IsSuccess)
            {
                return this.BuildFunctionRuleResult(new Range(start, 0));
            }

            return ParseResult.Failure;
        }
    }
}
