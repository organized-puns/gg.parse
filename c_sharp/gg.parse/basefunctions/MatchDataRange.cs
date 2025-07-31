namespace gg.parse.basefunctions
{
    public class MatchDataRange<T>(string name, int id, ProductionEnum action, T minDataValue, T maxDataValue)
        : ParseFunctionBase<T>(name, id, action)
        where T : IComparable<T>
    {
        public T MinDataValue { get; } = minDataValue;

        public T MaxDataValue { get; } = maxDataValue;

        public override Annotation? Parse(T[] input, int start)
        {
            if (start < input.Length)
            {
                if (input[start].CompareTo(MinDataValue) >= 0 && input[start].CompareTo(MaxDataValue) <= 0)
                {
                    return new Annotation(AnnotationDataCategory.Data, Id, new(start, 1));
                }
            }

            return null;
        }
    }
}
