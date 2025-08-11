
using gg.parse.rulefunctions;

using static gg.parse.rulefunctions.CommonTokenNames;
using static gg.parse.rulefunctions.CommonRules;

namespace gg.parse.ebnf
{
    public class EbnfTokenizer : RuleGraph<char>
    {
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

            // error = error "message" skip_rule

            var ebnfTokens =
                this.OneOf("#EbnfTokens", AnnotationProduct.Transitive,
                    this.EndOfLine(product: AnnotationProduct.None),
                    // make sure keywords are before the identifier
                    this.Keyword(MarkError, AnnotationProduct.Annotation, "error"),
                    this.Identifier(),
                    this.Integer(),
                    this.String(DoubleQuotedString, AnnotationProduct.Annotation, '"'),
                    this.String(SingleQuotedString, AnnotationProduct.Annotation, '\''),
                    MapNameToToken(Assignment, "="),
                    MapNameToToken(ScopeStart, "{"),
                    MapNameToToken(ScopeEnd, "}"),
                    MapNameToToken(EndStatement, ";"),
                    MapNameToToken(Elipsis, ".."),
                    // needs to be behind elipsis, elipsis being the more specific one
                    MapNameToToken(AnyCharacter, "."),
                    MapNameToToken(Option, "|"),
                    MapNameToToken(GroupStart, "("),
                    MapNameToToken(GroupEnd, ")"),
                    MapNameToToken(CollectionSeparator, ","),
                    MapNameToToken(ZeroOrOneOperator, "?"),
                    MapNameToToken(ZeroOrMoreOperator, "*"),
                    MapNameToToken(OneOrMoreOperator, "+"),
                    MapNameToToken(ArrayStart, "["),
                    MapNameToToken(ArrayEnd, "]"),
                    MapNameToToken(NotOperator, "!"),
                    MapNameToToken(TransitiveSelector, "#"),
                    MapNameToToken(NoProductSelector, "~"),
                    this.SingleLineComment(product: dropComments? AnnotationProduct.None : AnnotationProduct.Annotation),
                    this.MultiLineComment(product: dropComments ? AnnotationProduct.None : AnnotationProduct.Annotation)
                );

            var error = this.Error(UnknownToken, AnnotationProduct.Annotation,
                "Can't match the character at the given position to a token.", ebnfTokens, 0);

            Root = this.ZeroOrMore("#EbnfTokenizer", AnnotationProduct.Transitive,
                                this.OneOf("#WhiteSpaceTokenOrError", AnnotationProduct.Transitive, ebnfTokens, this.Whitespace(), error));
        }
        
        public ParseResult Tokenize(string text) => Root.Parse(text.ToCharArray(), 0);

        private RuleBase<char> MapNameToToken(string name, string token) =>
            this.Literal(name, AnnotationProduct.Annotation, token.ToCharArray());
    }
}

