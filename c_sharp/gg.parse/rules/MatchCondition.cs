
using gg.parse.util;

using Range = gg.parse.util.Range;


namespace gg.parse.rules
{
    public class MatchCondition<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        public RuleBase<T> Rule { get; private set; }

        public IEnumerable<RuleBase<T>> Rules => [Rule];

        public MatchCondition(
            string name, 
            IRule.Output production, 
            int precedence,
            RuleBase<T> rule
        ) : base(name, production, precedence)
        {
            Assertions.RequiresNotNull(rule);

            Rule = rule;
        }

        public MatchCondition(string name, RuleBase<T> rule)
            : base(name, IRule.Output.Self)
        {
            Assertions.RequiresNotNull(rule);

            Rule = rule;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            var result = Rule.Parse(input, start);

            if (result)
            {
                return BuildResult(new Range(start, 0), result.Annotations);
            }

            return ParseResult.Failure;
        }
    }
}
