namespace gg.parse.tokenizer
{
    public class CharacterRangeFunction(string name, int id, char min = 'a', char max = 'Z', ProductionEnum action = ProductionEnum.ProduceItem)
        : TokenFunction(name, id, action)
    {
        public char Min { get; init; } = min;

        public char Max { get; init; } = max;

        public override Annotation? Parse(string text, int start)
        {
            var range = TokenizerFunctions.InCharacterRange(text, start, Min, Max);
            return range.HasValue
                ? new Annotation(AnnotationCategory.Token, Id, range.Value)
                : null;
        }
    }
}
