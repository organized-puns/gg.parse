namespace gg.parse.rules
{
    public class MatchDataSet<T> : RuleBase<T> where T : IComparable<T>
    {
        public T[] MatchingValues { get; init; }

        public MatchDataSet(string name, IRule.Output production, T[] setValues, int precedence = 0)
            : base(name, production, precedence)
        {
            MatchingValues = setValues;
        }

        public MatchDataSet(string name, T[] matchingValues, int precedence = 0)
            : base(name, IRule.Output.Self, precedence)
        {
            MatchingValues = matchingValues;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            if (start < input.Length)
            {
                if (MatchingValues.Contains(input[start]))
                {
                    return BuildDataRuleResult(new Range(start, 1));
                }
            }

            return ParseResult.Failure;
        }
    }
}
