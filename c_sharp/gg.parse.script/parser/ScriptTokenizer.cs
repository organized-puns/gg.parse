
using gg.parse.rules;
using gg.parse.script.common;

using static gg.parse.script.common.CommonTokenNames;
using static gg.parse.script.common.CommonRules;

namespace gg.parse.script.parser
{
    public class ScriptTokenizer : CommonTokenizer
    {
        public ScriptTokenizer(bool dropComments = true)
        {
            var scriptTokens =
                oneOf(
                    "#scriptTokens",

                    // make sure keywords are matched before the Identifier
                    // otherwise no keywords will be found as Identifier will also match them
                    MatchScriptKeyword(),                  

                    Identifier(CommonTokenNames.Identifier),
                    Integer(CommonTokenNames.Integer),

                    MatchString(CommonTokenNames.DoubleQuotedString, '"'),
                    MatchString(CommonTokenNames.SingleQuotedString, '\''),

                    this.SingleLineComment(product: dropComments ? AnnotationProduct.None : AnnotationProduct.Annotation),
                    this.MultiLineComment(product: dropComments ? AnnotationProduct.None : AnnotationProduct.Annotation),

                    MatchScriptLiteral()  
                );

            var noMatchFallback = 
                this.LogError(
                    UnknownToken,
                    AnnotationProduct.Annotation,
                    "Can't match the character at the given position to a token.",
                    this.Skip(stopCondition: scriptTokens, failOnEoF: false)
                );

            Root = this.ZeroOrMore(
                    "#EbnfTokenizer", 
                    AnnotationProduct.Transitive,
                    this.OneOf(
                        "#TokenWhiteSpaceOrNoMatchFallback", 
                        AnnotationProduct.Transitive, 
                        scriptTokens, 
                        this.Whitespace(), 
                        noMatchFallback
                    )
            );
        }
        
        public ParseResult Tokenize(string text) => Root!.Parse(text.ToCharArray(), 0);

        private MatchFunctionSequence<char> MatchScriptKeyword() =>
            sequence(
                    "#matchKeyword",
                    ifMatches(LowerCaseLetter()),
                    oneOf(
                        "#keywordList",

                        Keyword(CommonTokenNames.LogFatal, "fatal"),
                        Keyword(CommonTokenNames.LogError, "error"),
                        Keyword(CommonTokenNames.LogWarning, "warning"),
                        Keyword(CommonTokenNames.LogInfo, "info"),
                        Keyword(CommonTokenNames.LogDebug, "debug"),
                        Keyword(CommonTokenNames.If, "if"),
                        Keyword(CommonTokenNames.TryMatchOperator, "try"),
                        Keyword(CommonTokenNames.Include, "include")
                    )
                );

        private MatchOneOfFunction<char> MatchScriptLiteral() =>
            oneOf(
                "#matchToken",
                Literal(CommonTokenNames.TryMatchOperatorShortHand, ">"),
                Literal(CommonTokenNames.Assignment, "="),
                Literal(CommonTokenNames.ScopeStart, "{"),
                Literal(CommonTokenNames.ScopeEnd, "}"),
                Literal(CommonTokenNames.EndStatement, ";"),
                Literal(CommonTokenNames.Elipsis, ".."),
                // needs to be behind elipsis, elipsis being the more specific one
                Literal(CommonTokenNames.AnyCharacter, "."),
                Literal(CommonTokenNames.Option, "|"),
                Literal(CommonTokenNames.GroupStart, "("),
                Literal(CommonTokenNames.GroupEnd, ")"),
                Literal(CommonTokenNames.CollectionSeparator, ","),
                Literal(CommonTokenNames.ZeroOrOneOperator, "?"),
                Literal(CommonTokenNames.ZeroOrMoreOperator, "*"),
                Literal(CommonTokenNames.OneOrMoreOperator, "+"),
                Literal(CommonTokenNames.ArrayStart, "["),
                Literal(CommonTokenNames.ArrayEnd, "]"),
                Literal(CommonTokenNames.NotOperator, "!"),
                Literal(CommonTokenNames.TransitiveSelector, "#"),
                Literal(CommonTokenNames.NoProductSelector, "~"),
                Literal(CommonTokenNames.OptionWithPrecedence, "/")
            );
     }
}

