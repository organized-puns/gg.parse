using gg.parse.util;
using Range = gg.parse.util.Range;

namespace gg.parse.rules
{
    public class MatchNot<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        public RuleBase<T> Rule { get; private set; }

        public IEnumerable<RuleBase<T>> Rules => [Rule];

        public int Count => 1;

        public RuleBase<T> this[int index]
        {
            get => Rule;
            set => Rule = value;
        }
        public MatchNot(string name, RuleOutput output, int precedence, RuleBase<T> rule)
            : base(name, output, precedence)
        {
            Rule = rule;
        }

        public MatchNot(string name, RuleBase<T> rule)
            : base(name, RuleOutput.Self)
        {
            Rule = rule;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            var subRuleResult = Rule.Parse(input, start);

            if (!subRuleResult)
            {
                return BuildResult(new Range(start, 0));
            }

            return ParseResult.Failure;
        }
    }
}
