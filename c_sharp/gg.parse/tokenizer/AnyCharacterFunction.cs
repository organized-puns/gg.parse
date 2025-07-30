namespace gg.parse.tokenizer
{
    public class AnyCharacterFunction(string name, int id, int min = 0, int max = 1, ProductionEnum action = ProductionEnum.ProduceItem)
    : TokenFunction(name, id, action)
    {
        public int Min { get; init; } = min;

        public int Max { get; init; } = max;

        public override Annotation? Parse(string text, int start)
        {
            var range = TokenizerFunctions.AnyCharacter(text, start, Min, Max);
            return range.HasValue
                ? new Annotation(AnnotationCategory.Token, Id, range.Value)
                : null;
        }
    }
}
