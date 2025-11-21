// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.util;

namespace gg.parse.rules
{
    

    public class FatalConditionException<T> : Exception where T : IComparable<T>
    {
        public LogRule<T> Rule { get; init; }

        public FatalConditionException(LogRule<T> rule)
            : base($"Fatal condition encountered while parsing {rule.Name}, parsing terminates at this point. See exception / inner exception for more details.")
        {
            Rule = rule;
        }
    }

    public sealed class LogRule<T> : MetaRuleBase<T> where T : IComparable<T>
    {
        public LogLevel Level { get; init; }

        /// <summary>
        /// Contains the log's text
        /// </summary>
        public string? Text { get; init; }

        public LogRule(
            string name, 
            AnnotationPruning pruning,
            IRule? condition,
            string? text, 
            LogLevel level = LogLevel.Info
        ) : base(name, pruning, 0, condition)
        {
            Text = text;
            Level = level;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            // subject is the condition to log on
            if (Subject != null)
            {
                var conditionalResult = Subject.Parse(input, start);

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

        public override LogRule<T> CloneWithSubject(IRule subject) =>
            new (Name, Prune, subject, Text, Level);
    }
}
