
using gg.parse.rules;
using gg.parse.script.common;

using static gg.parse.script.common.CommonTokenNames;

namespace gg.parse.script.parser
{
    public class ScriptTokenizer : CommonTokenizer
    {        
        public ScriptTokenizer(bool dropComments = true)
        {
            var scriptTokens =
                OneOf(
                    "#scriptTokens",

                    // make sure keywords are matched before the Identifier
                    // otherwise no keywords will be found as Identifier will also match them
                    MatchScriptKeyword(),                  

                    Identifier(CommonTokenNames.Identifier),
                    Integer(CommonTokenNames.Integer),

                    MatchString(CommonTokenNames.DoubleQuotedString, '"'),
                    MatchString(CommonTokenNames.SingleQuotedString, '\''),

                    SingleLineComment(dropComments ? null : CommonTokenNames.SingleLineComment),
                    MultiLineComment(dropComments ? null : CommonTokenNames.MultiLineComment),

                    MatchScriptLiteral()  
                );

            Root = ZeroOrMore(
                    "#root", 

                    OneOf(
                        "#rootOptions", 

                        scriptTokens, 

                        // Whitespace should not be in the tokens as the following error
                        // will have to ignore it as well.
                        Whitespace(),
                        
                        // else we found a token we can't handle, raise an error and skip
                        // characters until we find another valid script token or the eof.
                        error(
                            UnknownToken,
                            "Can't match the character at the given position to a token.",
                            this.Skip(stopCondition: scriptTokens, failOnEoF: false)
                        )
                    )
            );
        }

        public ParseResult Tokenize(string text) => Root!.Parse(text);

        // -- private methods -----------------------------------------------------------------------------------------

        private MatchFunctionSequence<char> MatchScriptKeyword() =>
            Sequence(
                    "#matchKeyword",
                    ifMatches(LowerCaseLetter()),
                    OneOf(
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
            OneOf(
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

