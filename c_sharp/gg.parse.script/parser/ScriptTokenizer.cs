
using static gg.parse.script.common.CommonTokenNames;
using static gg.parse.script.common.CommonRules;

namespace gg.parse.script.parser
{
    public class ScriptTokenizer : RuleGraph<char>
    {
        public ScriptTokenizer(bool dropComments = true)
        {
            var endOfKeyword = this.OneOf(
                this.Whitespace(),
                this.Not(this.IdentifierCharacter()),
                this.Not(this.Any())
            );

            var ebnfTokens =
                this.OneOf("#EbnfTokens", AnnotationProduct.Transitive,
                    this.EndOfLine(product: AnnotationProduct.None),
                    
                    // make sure keywords are before the identifier
                    this.Keyword(LogFatal, AnnotationProduct.Annotation, LogFatal),
                    this.Keyword(LogError, AnnotationProduct.Annotation, LogError),
                    this.Keyword(LogWarning, AnnotationProduct.Annotation, LogWarning),
                    this.Keyword(LogInfo, AnnotationProduct.Annotation, LogInfo),
                    this.Keyword(LogDebug, AnnotationProduct.Annotation, LogDebug),
                    this.Keyword(If, AnnotationProduct.Annotation, If),

                    // try match and shorthand
                    MapNameToToken(TryMatchOperatorShortHand, ">"),
                    this.Sequence(TryMatchOperator, AnnotationProduct.Annotation,
                        this.Literal("TryMatchKeyword", AnnotationProduct.Transitive, "try".ToArray()),
                        endOfKeyword
                    ),

                    // include
                    this.Sequence(Include, AnnotationProduct.Annotation,
                        this.Literal("IncludeKeyword", AnnotationProduct.Transitive, "include".ToArray()),
                        endOfKeyword
                    ),

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
                    this.MultiLineComment(product: dropComments ? AnnotationProduct.None : AnnotationProduct.Annotation),

                    // should be below comments, because conflicts with /* or //
                    MapNameToToken(OptionWithPrecedence, "/")
                );

            var noMatchFallback = 
                this.LogError(
                    UnknownToken,
                    AnnotationProduct.Annotation,
                    "Can't match the character at the given position to a token.",
                    this.Skip(stopCondition: ebnfTokens, failOnEoF: false)
                );

            Root = this.ZeroOrMore(
                    "#EbnfTokenizer", 
                    AnnotationProduct.Transitive,
                    this.OneOf(
                        "#TokenWhiteSpaceOrNoMatchFallback", 
                        AnnotationProduct.Transitive, 
                        ebnfTokens, 
                        this.Whitespace(), 
                        noMatchFallback
                    )
            );
        }
        
        public ParseResult Tokenize(string text) => Root!.Parse(text.ToCharArray(), 0);

        private RuleBase<char> MapNameToToken(string name, string token) =>
            this.Literal(name, AnnotationProduct.Annotation, token.ToCharArray());
    }
}

