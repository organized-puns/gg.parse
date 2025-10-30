// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.rules
{
    [Flags]
    public enum LogLevel
    {
        Fatal = 1,
        Error = 2,
        Warning = 4,
        Info = 8,
        Debug = 16,
        Any = Fatal | Error | Warning | Info | Debug
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
        public LogLevel Level { get; init; }

        /// <summary>
        /// Contains the log's text
        /// </summary>
        public string? Text { get; init; }

        private RuleBase<T>? _condition;
        
        /// <summary>
        /// Condition to capture this log. Tangentially also a way to correctly indicate
        /// the range where an error/warning applies to.
        /// </summary>
        public RuleBase<T>? Condition
        {
            get => _condition;
            init => _condition = value;
        }

        public IEnumerable<RuleBase<T>>? Rules => _condition == null ? null : [_condition];

        public int Count => _condition == null ? 0 : 1;

        public RuleBase<T>? this[int index]
        {
            get => _condition;
            set => _condition = value;
        }

        public LogRule(
            string name, 
            AnnotationPruning pruning, 
            string? text, 
            RuleBase<T>? condition = null, 
            LogLevel level = LogLevel.Info
        ) : base(name, pruning)
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

        public IRuleComposition<T> CloneWithComposition(IEnumerable<RuleBase<T>> composition)
        {
            return new LogRule<T>(
                Name, 
                Prune, 
                Text, 
                composition.FirstOrDefault(), 
                Level
            );
        }
    }
}
