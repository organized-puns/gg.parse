namespace gg.parse.basefunctions
{
    public class MatchNotFunction<T>(string name, int id, ProductionEnum action, ParseFunctionBase<T> function) 
        : ParseFunctionBase<T>(name, id, action)
        where T : IComparable<T>
    {
        public ParseFunctionBase<T> Function { get; } = function;

        public override Annotation? Parse(T[] input, int start)
        {
            var result = Function.Parse(input, start);

            if (result == null)
            {
                return new Annotation(AnnotationDataCategory.Data, Id, new (start, 0));
            }

            return null;
        }
    }
}
