namespace gg.parse.rulefunctions
{
    public class MatchAnyData<T>(string name, AnnotationProduct production = AnnotationProduct.Annotation, int min = 1, int max = 1)
        : RuleBase<T>(name, production)
        where T : IComparable<T>
    {
        public int MinLength { get; } = min;

        public int MaxLength { get; } = max;

        public override ParseResult Parse(T[] input, int start)
        {
            var tokensLeft = input.Length - start;

            if (tokensLeft >= MinLength)
            {
                var tokensRead = MaxLength <= 0
                        ? tokensLeft
                        : Math.Min(tokensLeft, MaxLength);

                return this.BuildDataRuleResult(new Range(start, tokensRead));
            }

            return ParseResult.Failure;
        }
    }
}
