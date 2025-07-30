namespace gg.parse.basefunctions
{
    public class MarkError<T>(string name, int id, string message)
        : ParseFunctionBase<T>(name, id, ProductionEnum.ProduceError ) 
        where T : IComparable<T>
    {
        public string Message { get; } = message;

        public override AnnotationBase? Parse(T[] input, int start)
        {
            // This function is intended to mark an error in the parsing process, not to match any data.
            throw new NotImplementedException();
        }
    }
}
