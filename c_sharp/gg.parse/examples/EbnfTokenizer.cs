using gg.parse.rulefunctions;
using static gg.parse.rulefunctions.TokenNames;

namespace gg.parse.examples
{
    public class EbnfTokenizer : BasicTokensTable
    {
        public RuleBase<char> Root { get; private set; }

        public EbnfTokenizer(bool dropComments = true)
        {
            // lit_dq_rule   = "literal";
            // lit_sq_rule   = 'literal';
            // set_rule      = {'s' 'e' 't'};
            // range_rule    = {'s'..'t'};
            // rule_one_of   = 'a' | 'b';
            // rule_sequnece = 'a', 'b';
            // group_rule    = ('a', 'b');
            // zero_or_one   = ?'b';
            // zero_or_more  = *'b';
            // one_or_more   = +'b';
            // n_to_m        = 1..2 'b';
            // n             = 3'b';
            // not_rule      = !'c';

            // #transitive_rule = '#';
            // ~none_rule       = '~';
            // single_line_comment = // all the way until the end of the line
            // multi_line_comment = /* bla */ 


            var ebnfTokens =
                OneOf("#EbnfTokens", AnnotationProduct.Transitive,
                    Identifier(),
                    Integer(),
                    String(DoubleQuotedString, AnnotationProduct.Annotation, '"'),
                    String(SingleQuotedString, AnnotationProduct.Annotation, '\''),
                    Literal("=", Assignment),
                    Literal("{", ScopeStart),
                    Literal("}", ScopeEnd),
                    Literal(";", EndStatement),
                    Literal("..", Elipsis),
                    Literal("|", Option),
                    Literal("(", GroupStart),
                    Literal(")", GroupEnd),
                    Literal(",", CollectionSeparator),
                    Literal("?", ZeroOrOneOperator),
                    Literal("*", ZeroOrMoreOperator),
                    Literal("+", OneOrMoreOperator),
                    Literal("[", ArrayStart),
                    Literal("]", ArrayEnd),
                    Literal("!", NotOperator),
                    Literal("#", TransitiveSelector),
                    Literal("~", NoProductSelector),
                    SingleLineComment(product: dropComments? AnnotationProduct.None : AnnotationProduct.Annotation),
                    MultiLineComment(product: dropComments ? AnnotationProduct.None : AnnotationProduct.Annotation),
                    EndOfLine()
                );

            var error = Error(UnknownToken, AnnotationProduct.Annotation,
                "Can't match the character at the given position to a token.", ebnfTokens, 0);

            Root = ZeroOrMore("#EbnfTokenizer", AnnotationProduct.Transitive,
                                OneOf("#WhiteSpaceTokenOrError", AnnotationProduct.Transitive, ebnfTokens, Whitespace(), error));
        }

        public ParseResult Tokenize(string text) => Root.Parse(text.ToCharArray(), 0);

        public (ParseResult, string) ParseFile(string path)
        {
            var text = File.ReadAllText(path);
            return (Tokenize(text), text);
        }
    }
}

