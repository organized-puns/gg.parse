namespace gg.parse.tokenizer
{
    public class MatchNotFunction(string name, int id, TokenFunction function, ProductionEnum action = ProductionEnum.ProduceItem)
        : TokenFunction(name, id, action)
    {
        public TokenFunction Function { get; init; } = function;
        
        public override Annotation? Parse(string text, int start)
        {
            var range = TokenizerFunctions.MatchNot(text, start, Function);
            return range.HasValue
                ? new Annotation(AnnotationCategory.Token, Id, range.Value)
                : null;
        }
    }
}
