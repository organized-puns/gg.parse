namespace gg.parse.tokenizer
{
    public class MatchCountFunction(string name, int id, TokenFunction function, int min = 0, int max = 0, ProductionEnum action = ProductionEnum.ProduceItem)
        : TokenFunction(name, id, action)
    {
        public TokenFunction Function { get; init; } = function;
        
        public int Min { get; init; } = min;

        public int Max { get; init; } = max;

        public override Annotation? Parse(string text, int start)
        {
            var range = TokenizerFunctions.MatchCount(text, start, Function, Min, Max);
            return range.HasValue
                ? new Annotation(AnnotationCategory.Token, Id, range.Value)
                : null;
        }
    }
}
