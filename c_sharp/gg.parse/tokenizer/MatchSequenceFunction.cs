namespace gg.parse.tokenizer
{
    public class MatchSequenceFunction(string name, int id, TokenFunction[] sequence, ProductionEnum action = ProductionEnum.ProduceItem)
        : TokenFunction(name, id, action)
    {
        public TokenFunction[] Sequence { get; init; } = sequence;

        public override Annotation? Parse(string text, int start)
        {
            var range = TokenizerFunctions.MatchSequence(text, start, Sequence);
            return range.HasValue
                ? new Annotation(AnnotationCategory.Token, Id, range.Value)
                : null;
        }
    }
}
