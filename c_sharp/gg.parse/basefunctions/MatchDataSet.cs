namespace gg.parse.basefunctions
{
    public class MatchDataSet<T>(string name, int id, ProductionEnum action, T[] matchingValues)
        : ParseFunctionBase<T>(name, id, action)
        where T : IComparable<T>
    {
        
        public T[] MatchingValues { get; } = matchingValues;

        public override AnnotationBase? Parse(T[] input, int start)
        {
            if (start < input.Length)
            {
                if (MatchingValues.Contains(input[start]))
                {
                    return new AnnotationBase(AnnotationDataCategory.Data, Id, new(start, 1));
                }
            }

            return null;
        }
    }
}
