using gg.parse.rulefunctions;

using static gg.parse.rulefunctions.CommonTokenNames;

namespace gg.parse.instances.json
{
    public class JsonTokenizer : RuleGraph<char>
    {     
        public JsonTokenizer()
        {
            var jsonTokens =
                this.OneOf("#JsonTokens", AnnotationProduct.Transitive,
                    this.Float(),
                    this.Integer(),
                    // need to override otherwise the name will hold the delimiter which
                    // will interfere with the style lookup in html
                    this.String(CommonTokenNames.String, AnnotationProduct.Annotation),
                    this.Boolean(),
                    Literal("{", ScopeStart),
                    Literal("}", ScopeEnd),
                    Literal("[", ArrayStart),
                    Literal("]", ArrayEnd),
                    Literal("null", Null),
                    Literal(",", CollectionSeparator),
                    Literal(":", KeyValueSeparator));

            var error = this.Error(UnknownToken, AnnotationProduct.Annotation,
                "Can't match the character at the given position to a token.", jsonTokens, 0);

            Root = this.ZeroOrMore("#JsonTokenizer", AnnotationProduct.Transitive,
                                this.OneOf("#WhiteSpaceTokenOrError", AnnotationProduct.Transitive, this.Whitespace(), jsonTokens, error));
        }

        public RuleBase<char> Literal(string token, string name) =>
            CommonRules.Literal(this, name, AnnotationProduct.Annotation, token.ToCharArray());

        public ParseResult Tokenize(string text) => Root.Parse(text.ToCharArray(), 0);

        public (ParseResult, string) ParseFile(string path)
        {
            var text = File.ReadAllText(path);
            return (Tokenize(text), text);
        }
    }
}
