namespace gg.parse.rulefunctions.datafunctions
{
    public class MatchSingleData<T>(string name, T data, AnnotationProduct production = AnnotationProduct.Annotation)
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

                return this.BuildDataRuleResult(new Range(start, 1));
            }

            return ParseResult.Failure;
        }
    }
}
