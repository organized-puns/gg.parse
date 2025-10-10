


namespace gg.parse.rules
{
    [Flags]
    public enum LogLevel
    {
        Fatal = 1,
        Error = 2,
        Warning = 4,
        Info = 8,
        Debug = 16
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

    public class LogRule<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        private static readonly RuleBase<T>[] EmptyRuleComposition = [];

        public LogLevel Level { get; init; }

        /// <summary>
        /// Contains the log's text
        /// </summary>
        public string? Text { get; init; }

        private RuleBase<T> _condition;
        private RuleBase<T>[] _rulesCollection;

        public RuleBase<T>? Condition
        {
            get => _condition;
            init
            {
                if (value != null)
                {
                    _condition = value;
                    _rulesCollection = [_condition];
                }
                else
                {
                    _rulesCollection = EmptyRuleComposition;
                }
            }
        }

        public IEnumerable<RuleBase<T>> Rules => _rulesCollection;

        public LogRule(
            string name, 
            RuleOutput product, 
            string? text, 
            RuleBase<T>? condition = null, 
            LogLevel level = LogLevel.Info
        ) : base(name, product)
        {
            Text = text;
            Condition = condition;
            Level = level;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            if (Condition != null)
            {
                var conditionalResult = Condition.Parse(input, start);

                if (conditionalResult)
                {
                    if (Level == LogLevel.Fatal)
                    {
                        throw new FatalConditionException<T>(this);
                    }

                    return BuildDataRuleResult(new(start, conditionalResult.MatchLength));
                }

                return ParseResult.Failure;
            }

            if (Level == LogLevel.Fatal)
            {
                throw new FatalConditionException<T>(this);
            }

            return BuildDataRuleResult(new(start, 0));
        }
    }
}
