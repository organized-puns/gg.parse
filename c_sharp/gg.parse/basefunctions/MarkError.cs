namespace gg.parse.basefunctions
{
    public class MarkError<T>(string name, int id, string message, ParseFunctionBase<T> testFunction, int maxLength = 0)
        : ParseFunctionBase<T>(name, id, ProductionEnum.ProduceError ) 
        where T : IComparable<T>
    {
        public string Message { get; } = message;

        public ParseFunctionBase<T> TestFunction { get; } = testFunction;
        
        public override Annotation? Parse(T[] input, int start)
        {
            var index = start;

            do
            {
                index++;
            } while (index < input.Length 
                && (maxLength <= 0 || (index - start) < maxLength)
                && TestFunction.Parse(input, index) == null);
            

            return new Annotation(AnnotationDataCategory.Error, Id, new Range(start, index - start));
        }
    }
}
