namespace gg.parse.rulefunctions.datafunctions
{
    public class MatchDataSet<T> : RuleBase<T> where T : IComparable<T>
    {
        public T[] MatchingValues { get; init; }

        public MatchDataSet(string name, AnnotationProduct action, T[] matchingValues)
            : base(name, action)
        {
            MatchingValues = matchingValues;
        }

        public MatchDataSet(string name, T[] matchingValues)
            : base(name, AnnotationProduct.Annotation)
        {
            MatchingValues = matchingValues;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            if (start < input.Length)
            {
                if (MatchingValues.Contains(input[start]))
                {
                    return this.BuildDataRuleResult(new Range(start, 1));
                }
            }

            return ParseResult.Failure;
        }
    }
}
