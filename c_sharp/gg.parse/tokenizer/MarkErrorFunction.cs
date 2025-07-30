namespace gg.parse.tokenizer
{
    public class MarkErrorFunction(string name, int id, string message, int skipCharacters = 0, ProductionEnum action = ProductionEnum.ProduceError)
        : TokenFunction(name, id, action)
    {
        public string Message { get; init; } = message;

        public int SkipCharacters { get; init; } = skipCharacters;

        public override Annotation? Parse(string text, int start)
        {
            return new Annotation(AnnotationCategory.Error, Id, new Range(start, SkipCharacters));
        }
    }
}
