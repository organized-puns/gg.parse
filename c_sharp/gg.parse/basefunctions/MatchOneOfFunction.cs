namespace gg.parse.basefunctions
{
    public class MatchOneOfFunction<T>(string name, int id, ProductionEnum action, params ParseFunctionBase<T>[] options) 
        : ParseFunctionBase<T>(name, id, action)
        where T : IComparable<T>
    {
        public ParseFunctionBase<T>[] Options { get; } = options;

        public override AnnotationBase? Parse(T[] input, int start)
        {
            foreach (var option in Options)
            {
                var result = option.Parse(input, start);
                if (result != null && result.Category == AnnotationDataCategory.Data)
                {
                    return new AnnotationBase(AnnotationDataCategory.Data, Id, result.Range);
                }
            }
            return null;
        }
        
        public override string ToString() => Name;
    }
}
