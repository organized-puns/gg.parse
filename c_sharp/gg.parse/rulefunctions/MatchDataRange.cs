namespace gg.parse.rulefunctions
{
    public class MatchDataRange<T>(string name, T minDataValue, T maxDataValue, AnnotationProduction production = AnnotationProduction.Annotation)
        : RuleBase<T>(name, production)
        where T : IComparable<T>
    {
        public T MinDataValue { get; } = minDataValue;

        public T MaxDataValue { get; } = maxDataValue;

        public override ParseResult Parse(T[] input, int start)
        {
            if (start < input.Length)
            {
                if (input[start].CompareTo(MinDataValue) >= 0 && input[start].CompareTo(MaxDataValue) <= 0)
                {
                    return this.BuildDataRuleResult(new(start, 1));
                }
            }

            return ParseResult.Failure;
        }
    }
}
