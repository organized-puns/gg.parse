namespace gg.parse.tokenizer
{
    public class CharacterSetFunction(string name, int id, string set, ProductionEnum action = ProductionEnum.ProduceItem)
        : TokenFunction(name, id, action)
    {
        public string Set { get; init; } = set;

        public override Annotation? Parse(string text, int start)
        {
            var range = TokenizerFunctions.InCharacterSet(text, start, Set);
            return range.HasValue
                ? new Annotation(AnnotationCategory.Token, Id, range.Value)
                : null;
        }      
    }
}
