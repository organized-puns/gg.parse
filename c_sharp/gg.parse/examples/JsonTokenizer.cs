using gg.parse.rulefunctions;
using System.Text;
using static gg.parse.rulefunctions.TokenNames;

namespace gg.parse.examples
{
    public class JsonTokenizer : BasicTokensTable
    {
        public RuleBase<char> Root { get; private set; }

        public JsonTokenizer()
        {
            var jsonTokens = 
                OneOf("#JsonTokens", AnnotationProduct.Transitive,
                    Float(),
                    Integer(),
                    // need to override otherwise the name will hold the delimiter which
                    // will interfere with the style lookup in html
                    String(TokenNames.String, AnnotationProduct.Annotation),
                    Boolean(),
                    Literal("{", ScopeStart),
                    Literal("}", ScopeEnd),
                    Literal("[", ArrayStart),
                    Literal("]", ArrayEnd),
                    Literal("null", Null),
                    Literal(",", CollectionSeparator),
                    Literal(":", KeyValueSeparator));

            var error = Error(UnknownToken, AnnotationProduct.Annotation,
                "Can't match the character at the given position to a token.", jsonTokens, 0);

            Root = ZeroOrMore("#JsonTokenizer", AnnotationProduct.Transitive,
                                OneOf("#WhiteSpaceTokenOrError", AnnotationProduct.Transitive, Whitespace(), jsonTokens, error));
        }

        public ParseResult Tokenize(string text) => Root.Parse(text.ToCharArray(), 0);

        public (ParseResult, string) ParseFile(string path)
        {
            var text = File.ReadAllText(path);
            return (Tokenize(text), text);
        }

        
    }
}
