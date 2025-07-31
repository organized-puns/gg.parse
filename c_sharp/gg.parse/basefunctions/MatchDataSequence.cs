namespace gg.parse.basefunctions
{
    public class MatchDataSequence<T>(string name, int id, ProductionEnum action, T[] dataArray) 
        : ParseFunctionBase<T>(name, id, action)
        where T : IComparable<T>
    {

        public T[] DataArray { get; } = dataArray;

        public override Annotation? Parse(T[] input, int start)
        {
            var index = start;

            for (var i = 0; i < DataArray.Length; i++)
            {
                if (index >= input.Length || input[index].CompareTo(DataArray[i]) != 0)
                {
                    return null;
                }
                index++;
            }

            return new Annotation(AnnotationDataCategory.Data, Id, new(start, index - start));
        }
    }
}
