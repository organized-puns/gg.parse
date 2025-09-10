

namespace gg.parse.rulefunctions
{
    public class MatchError<T>(string name, AnnotationProduct product, string? description, RuleBase<T> condition)
        : RuleBase<T>(name, product), IRuleComposition<T>
        where T : IComparable<T>
    {
        /// <summary>
        /// Message describing the nature of the error
        /// </summary>
        public string? Message { get; } = description;

        public RuleBase<T> Condition { get; set; } = condition;

        public IEnumerable<RuleBase<T>> SubRules => [Condition];

        public override ParseResult Parse(T[] input, int start)
        {
            if (start < input.Length)
            {
                var conditionalResult = Condition.Parse(input, start);

                if (conditionalResult.FoundMatch)
                {
                    return this.BuildDataRuleResult(new(start, conditionalResult.MatchedLength));
                }
            }

            return ParseResult.Failure;
        }
    }
}
