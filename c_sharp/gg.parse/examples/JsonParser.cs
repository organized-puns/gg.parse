using gg.parse.rulefunctions;

namespace gg.parse.examples
{
    public class JsonParser : RuleTable<int>
    {
        public JsonTokenizer Tokenizer { get; init; }

        public RuleBase<int> Root { get; private set; }

        public JsonParser(JsonTokenizer tokenizer)
        {
            Tokenizer = tokenizer;

            var key = Token("Key", AnnotationProduct.Annotation, TokenId(TokenNames.String));
            var stringValue = Token("String", AnnotationProduct.Annotation, TokenId(TokenNames.String));
            var intValue = Token("Int", AnnotationProduct.Annotation, TokenId(TokenNames.Integer));
            var floatValue = Token("Float", AnnotationProduct.Annotation, TokenId(TokenNames.Float));
            var boolValue = Token("Bool", AnnotationProduct.Annotation, TokenId(TokenNames.Boolean));
            var nullValue = Token("Null", AnnotationProduct.Annotation, TokenId(TokenNames.Null));

            var value = OneOf("Value", AnnotationProduct.Annotation, stringValue, intValue, floatValue, boolValue, nullValue);
            var keyValueSeparator = Token(TokenNames.KeyValueSeparator);
            var objectStart = Token(TokenNames.ScopeStart);
            var objectEnd = Token(TokenNames.ScopeEnd);
            var comma = Token(TokenNames.CollectionSeparator);

            var keyValue = Sequence("KeyValue", AnnotationProduct.Annotation, key, keyValueSeparator, value);
            var nextKeyValue = Sequence("NextKeyValue", AnnotationProduct.Transitive, comma, keyValue);
            var keyValueList = Sequence("KeyValueList", AnnotationProduct.Transitive, keyValue,
                ZeroOrMore("KeyValueListRest", AnnotationProduct.Transitive, nextKeyValue));

            var jsonObject = Sequence("Object", AnnotationProduct.Annotation,
                objectStart, ZeroOrMore("ObjectProperties", AnnotationProduct.Transitive, keyValueList), objectEnd);

            value.Options = [.. value.Options, jsonObject];

            // todo add array, root and error

            Root = jsonObject;
        }

        public int TokenId(string name) => Tokenizer.FindRule(name).Id;

        public int[] TokenIds(params string[] names) => names.Select(n => TokenId(n)).ToArray();

        public RuleBase<int> Token(string tokenName) => Token(tokenName, AnnotationProduct.None);
        
        public RuleBase<int> Token(string tokenName, AnnotationProduct product)
        {
            var rule = Tokenizer.FindRule(tokenName);
            return Token($"{product.GetPrefix()}Token({rule.Name})", product, rule.Id);
        }

        public RuleBase<int> Token(string name, AnnotationProduct product, int tokenId) =>
            TryFindRule(name, out MatchSingleData<int>? existingRule)
                 ? existingRule!
                 : RegisterRule(new MatchSingleData<int>(name, tokenId, product));

        public ParseResult Parse(List<basefunctions.Annotation> tokens)
        {
            return Root.Parse(tokens.Select(t => t.FunctionId).ToArray(), 0);
        }

    }
}
