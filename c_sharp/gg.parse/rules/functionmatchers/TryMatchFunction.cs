using gg.core.util;

namespace gg.parse.rulefunctions.rulefunctions
{
    public class TryMatchFunction<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        public RuleBase<T> Rule { get; private set; }

        public IEnumerable<RuleBase<T>> SubRules => [Rule];

        public TryMatchFunction(string name, AnnotationProduct production, RuleBase<T> rule)
            : base(name, production)
        {
            Rule = rule;
        }

        public TryMatchFunction(string name, RuleBase<T> rule)
            : base(name, AnnotationProduct.Annotation)
        {
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

        public void ReplaceSubRule(RuleBase<T> subRule, RuleBase<T> replacement)
        {
            Contract.Requires(subRule == Rule);
            Rule = replacement;
        }
    }
}
