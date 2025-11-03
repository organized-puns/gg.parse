// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.script.common
{
    public static class CommonTokenNames
    {
        public static readonly string AnyCharacter = "any_char";

        public static readonly string ArrayStart = "array_start";

        public static readonly string ArrayEnd = "array_end";

        public static readonly string Assignment = "assignment";

        public static readonly string Boolean = "bool";

        public static readonly string BreakKeyword = "break_keyword";

        public static readonly string Callback = "callback";

        public static readonly string CollectionSeparator = "collection_separator";

        public static readonly string DataSequence = "data_sequence";

        public static readonly string DataRange = "data_range";

        public static readonly string Digit = "digit";

        public static readonly string DigitSequence = "digit_sequence";

        public static readonly string DoubleQuotedString = "double_quoted_string";

        public static readonly string Elipsis = "elipsis";

        public static readonly string EndOfLine = "eol";

        public static readonly string EndStatement = "end_statement";

        public static readonly string FindOperator = "find_operator";

        public const           string FunctionSequence = "function_sequence";

        public static readonly string Float = "float";

        public static readonly string GroupEnd = "group_end";

        public static readonly string GroupStart = "group_start";

        public const           string Identifier = "identifier";

        public static readonly string IdentifierStartingCharacter = "identifier_start";

        public static readonly string IdentifierCharacter = "identifier_char";

        public const           string If = "_if";

        public static readonly string Include = "_include";

        public static readonly string Integer = "_int";

        public static readonly string KeyValueSeparator = "key_value_separator";

        public static readonly string Keyword = "keyword";

        public const           string Literal = "literal";

        public static readonly string LogDebug = "_debug";

        public static readonly string LogError = "_error";

        public static readonly string LogFatal = "_fatal";

        public static readonly string LogInfo = "_info";

        public static readonly string LogWarning = "_warning";

        public static readonly string LowerCaseLetter = "lower_case";

        public static readonly string MultiLineComment = "multiline_comment";

        public static readonly string PruneAll = "prune_all";

        public static readonly string PruneChildren = "prune_children";

        public static readonly string PruneRoot = "prune_root";     

        public const string           Not = "not";

        public static readonly string NotOperator = "not_operator";

        public static readonly string Null = "_null";

        public const string           OneOf = "one_of";

        public const string           OneOrMore = "one_or_more";

        public static readonly string OneOrMoreOperator = "one_or_more_operator";

        public const string           OptionWithPrecedence = "option_with_precedence";

        public static readonly string ScopeStart = "scope_start";

        public static readonly string ScopeEnd = "scope_end";

        public static readonly string Set = "_set";

        public static readonly string Sign = "sign";

        public static readonly string SingleData = "single_data";

        public static readonly string SingleLineComment = "singleline_comment";

        public static readonly string SingleQuotedString = "single_quoted_string";

        public static readonly string StopAfter = "stop_after_operator";

        public static readonly string StopAt = "stop_at_operator";

        public static readonly string String = "_string";

        public static readonly string Underscore = "underscore";

        public static readonly string UnknownToken = "unknown_token";

        public static readonly string UpperCaseLetter = "uppercase";

        public static readonly string Whitespace = "whitespace";

        public const string           ZeroOrMore = "zero_or_more";

        public static readonly string ZeroOrMoreOperator = "zero_or_more_operator";

        public const string           ZeroOrOne = "zero_or_one";

        public static readonly string ZeroOrOneOperator = "zero_or_one_operator";
    }
}
