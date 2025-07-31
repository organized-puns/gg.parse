namespace gg.parse.basefunctions
{
    public class MatchAnyData<T>(string name, int id, ProductionEnum action, int min = 1, int max = 1)
        : ParseFunctionBase<T>(name, id, action)
        where T : IComparable<T>
    {
        public int MinLength { get; } = min;

        public int MaxLength { get; } = max;

        public override Annotation? Parse(T[] input, int start)
        {
            var tokensLeft = input.Length - start;

            if (tokensLeft >= MinLength)
            {
                var tokensRead = MaxLength <= 0
                        ? tokensLeft
                        : Math.Min(tokensLeft, MaxLength);

                return new Annotation(AnnotationDataCategory.Data, Id, new(start, tokensRead));
            }

            return null;
        }
    }
}
