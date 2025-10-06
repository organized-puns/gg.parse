using Range = gg.parse.util.Range;

namespace gg.parse.rules
{
    public class MatchSingleData<T>(string name, T data, IRule.Output production = IRule.Output.Self)
        : RuleBase<T>(name, production)
        where T : IComparable<T>
    {
        public T Data { get; } = data;

        public override ParseResult Parse(T[] input, int start)
        {
            if (start < input.Length)
            {
                if (input[start].CompareTo(Data) != 0)
                {
                    return ParseResult.Failure;
                }

                return BuildDataRuleResult(new Range(start, 1));
            }

            return ParseResult.Failure;
        }
    }
}
