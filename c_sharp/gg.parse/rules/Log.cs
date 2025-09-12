

namespace gg.parse.rulefunctions
{
    public enum LogLevel
    {
        Fatal       = 1,
        Error       = 2,
        Warning     = 3,
        Info        = 4,
        Debug       = 5
    }

    public class Log<T>(string name, AnnotationProduct product, string? description, RuleBase<T>? condition = null, LogLevel level = LogLevel.Info)
        : RuleBase<T>(name, product), IRuleComposition<T>
        where T : IComparable<T>
    {
        public LogLevel Level { get; init; } = level;

        /// <summary>
        /// Message describing the nature of the error
        /// </summary>
        public string? Message { get; } = description;

        public RuleBase<T>? Condition { get; set; } = condition;

        public IEnumerable<RuleBase<T>> SubRules => [Condition];

        public override ParseResult Parse(T[] input, int start)
        {
            if (Condition != null)
            {
                if (start < input.Length)
                {
                    var conditionalResult = Condition.Parse(input, start);

                    if (conditionalResult.FoundMatch)
                    {
                        return BuildDataRuleResult(new(start, conditionalResult.MatchedLength));
                    }
                }

                return ParseResult.Failure;
            }

            return BuildDataRuleResult(new(start, 0));
        }
    }
}
