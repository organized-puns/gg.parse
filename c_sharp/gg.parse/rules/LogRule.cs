

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

    public class FatalConditionException<T> : Exception where T : IComparable<T>
    {
        public LogRule<T> Rule { get; init; }

        public FatalConditionException(LogRule<T> rule)
            : base($"Fatal condition encountered while parsing {rule.Name}, parsing terminates at this point. See exception / inner exception for more details.")
        {
            Rule = rule;
        }
    }

    public class LogRule<T>(string name, AnnotationProduct product, string? description, RuleBase<T>? condition = null, LogLevel level = LogLevel.Info)
        : RuleBase<T>(name, product), IRuleComposition<T>
        where T : IComparable<T>
    {
        public LogLevel Level { get; init; } = level;

        /// <summary>
        /// Message describing the nature of the error
        /// </summary>
        public string? Message { get; } = description;

        public RuleBase<T>? Condition { get; set; } = condition;

        // xxx deal with nullable case
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
                        if (Level == LogLevel.Fatal)
                        {
                            throw new FatalConditionException<T>(this);
                        }

                        return BuildDataRuleResult(new(start, conditionalResult.MatchedLength));
                    }
                }

                return ParseResult.Failure;
            }

            return BuildDataRuleResult(new(start, 0));
        }
    }
}
