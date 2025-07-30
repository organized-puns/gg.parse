
namespace gg.parse.basefunctions
{
    public class MatchFunctionCount<T>(
        string name, int id, ProductionEnum action, ParseFunctionBase<T> function, int min = 1, int max = 1)
        : ParseFunctionBase<T>(name, id, action)
        where T : IComparable<T>
    {
       
        public ParseFunctionBase<T> Function { get; } = function;
        
        public int Min { get; } = min;
        
        public int Max { get; } = max;
        

        public override AnnotationBase? Parse(T[] input, int start)
        {
            int count = 0;
            int index = start;

            while (index < input.Length && (Max <= 0 || count < Max))
            {
                var result = Function.Parse(input, index);
                if (result == null || result.Category == AnnotationDataCategory.Error)
                {
                    break;
                }
                count++;
                index += result.Range.Length;
            }

            if (Min <= 0 || count >= Min)
            {
                return new AnnotationBase(AnnotationDataCategory.Data, Id, new Range(start, index - start));
            }

            return null;
        }
    }
}
