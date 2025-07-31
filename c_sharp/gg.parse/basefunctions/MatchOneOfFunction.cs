namespace gg.parse.basefunctions
{
    public class MatchOneOfFunction<T>(string name, int id, ProductionEnum action, params ParseFunctionBase<T>[] options) 
        : ParseFunctionBase<T>(name, id, action)
        where T : IComparable<T>
    {
        public ParseFunctionBase<T>[] Options { get; } = options;

        public override Annotation? Parse(T[] input, int start)
        {
            foreach (var option in Options)
            {
                var result = option.Parse(input, start);
                if (result != null && result.Category == AnnotationDataCategory.Data)
                {
                    var children = option.ActionOnMatch == ProductionEnum.ProduceItem
                        ? result.Children
                        : null;

                    return new Annotation(AnnotationDataCategory.Data, Id, result.Range, children);
                }
            }
            return null;
        }
        
        public override string ToString() => Name;
    }
}
