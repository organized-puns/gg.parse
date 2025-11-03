// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.rules;
using gg.parse.script.common;

namespace gg.parse.script.parser
{
    public class ScriptTokenizer : CommonTokenizer
    {        
        public ScriptTokenizer(bool dropComments = true)
        {
            var scriptTokens =
                OneOf(
                    "-r scriptTokens",

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
                    "-r root",

                    OneOf(
                        "-r rootOptions", 

                        scriptTokens,

                        // Whitespace should not be in the tokens as the following error
                        // will have to ignore it as well.
                        Whitespace(),

                        // else we found a token we can't handle, raise an error and skip
                        // characters until we find another valid script token or the eof.
                        Error(
                            CommonTokenNames.UnknownToken,
                            "Can't match the character at the given position to a token.",
                            Skip(stopCondition: scriptTokens, failOnEoF: false)
                        )
                    )
            );
        }

        public ParseResult Tokenize(string text) => Root!.Parse(text);

        // -- private methods -----------------------------------------------------------------------------------------

        private MatchRuleSequence<char> MatchScriptKeyword() =>
            Sequence(
                    "-r matchKeyword",
                    // early rejection
                    IfMatch("-r is_keyword", LowerCaseLetter()),
                    OneOf(
                        "-r keywordList",

                        Keyword(CommonTokenNames.LogFatal, "fatal"),
                        Keyword(CommonTokenNames.LogError, "error"),
                        Keyword(CommonTokenNames.LogWarning, "warning"),
                        Keyword(CommonTokenNames.LogInfo, "info"),
                        Keyword(CommonTokenNames.LogDebug, "debug"),
                        Keyword(CommonTokenNames.If, "if"),
                        Keyword(CommonTokenNames.Include, "include"),
                        Keyword(CommonTokenNames.FindOperator, "find"),
                        Keyword(CommonTokenNames.StopAfter, "stop_after"),
                        Keyword(CommonTokenNames.StopAt, "stop_at")
                        
                    )
                );

        private MatchOneOf<char> MatchScriptLiteral() =>
            OneOf(
                "-r matchToken",

                Literal(CommonTokenNames.Assignment, "="),
                Literal(CommonTokenNames.ScopeStart, "{"),
                Literal(CommonTokenNames.ScopeEnd, "}"),
                Literal(CommonTokenNames.EndStatement, ";"),
                Literal(CommonTokenNames.Elipsis, ".."),
                // needs to be behind elipsis, elipsis being the more specific one
                Literal(CommonTokenNames.AnyCharacter, "."),
                Literal(CommonTokenNames.OneOf, "|"),
                Literal(CommonTokenNames.GroupStart, "("),
                Literal(CommonTokenNames.GroupEnd, ")"),
                Literal(CommonTokenNames.CollectionSeparator, ","),
                Literal(CommonTokenNames.ZeroOrOneOperator, "?"),
                Literal(CommonTokenNames.ZeroOrMoreOperator, "*"),
                Literal(CommonTokenNames.OneOrMoreOperator, "+"),
                Literal(CommonTokenNames.ArrayStart, "["),
                Literal(CommonTokenNames.ArrayEnd, "]"),
                Literal(CommonTokenNames.NotOperator, "!"),
                Literal(CommonTokenNames.PruneRoot, AnnotationPruningToken.Root),
                Literal(CommonTokenNames.PruneAll, AnnotationPruningToken.All),
                Literal(CommonTokenNames.PruneChildren, AnnotationPruningToken.Children),
                Literal(CommonTokenNames.OptionWithPrecedence, "/")
            );
     }
}

