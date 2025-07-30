namespace gg.parse.tokenizer
{
    public class LiteralFunction(string name, int id, string literal, ProductionEnum action = ProductionEnum.ProduceItem)
    : TokenFunction(name, id, action)
    {
        public string Literal { get; init; } = literal;

        public override Annotation? Parse(string text, int start)
        {
            var range = TokenizerFunctions.Literal(text, start, Literal);
            return range.HasValue
                ? new Annotation(AnnotationCategory.Token, Id, range.Value)
                : null;
        }
    }
}
