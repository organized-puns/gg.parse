namespace gg.parse.tokenizer
{
    public class MatchOneOfFunction(string name, int id, TokenFunction[] options, ProductionEnum action = ProductionEnum.ProduceItem)
        : TokenFunction(name, id, action)
    {
        public TokenFunction[] Options { get; init; } = options;

        public override Annotation? Parse(string text, int start)
        {
            var range = TokenizerFunctions.MatchOneOf(text, start, Options);
            return range.HasValue
                ? new Annotation(AnnotationCategory.Token, Id, range.Value)
                : null;
        }
    }
}
