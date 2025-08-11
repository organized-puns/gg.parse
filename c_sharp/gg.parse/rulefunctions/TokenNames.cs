namespace gg.parse.rulefunctions
{
    public static class TokenNames
    {
        public static readonly string NoProductPrefix = "~";
        public static readonly string NoProductSelector= "NoProductSelector";        

        public static readonly string TransitiveProductPrefix = "#";
        public static readonly string TransitiveSelector= "TransitiveSelector";

        public static readonly string AnyCharacter = "AnyCharacter";

        public static readonly string ArrayStart = "ArrayStart";

        public static readonly string ArrayEnd = "ArrayEnd";

        public static readonly string Boolean = "Boolean";

        public static readonly string CollectionSeparator = "CollectionSeparator";

        public static readonly string DataSequence = "DataSequence";

        public static readonly string Digit = "Digit";

        public static readonly string DigitSequence = "DigitSequence";

        public static readonly string FunctionSequence = "FunctionSequence";

        public static readonly string Float = "Float";

        public static readonly string Integer = "Integer";

        public static readonly string KeyValueSeparator = "KeyValueSeparator";

        public static readonly string Literal = "Literal";

        public static readonly string Not = "Not";

        public static readonly string Null = "Null";

        public static readonly string OneOf = "OneOf";

        public static readonly string OneOrMore = "OneOrMore";

        public static readonly string ScopeStart = "ScopeStart";

        public static readonly string ScopeEnd = "ScopeEnd";

        public static readonly string Set = "Set";

        public static readonly string Sign = "Sign";

        public static readonly string String = "String";

        public static readonly string UnknownToken = "UnknownToken";

        public static readonly string Whitespace = "Whitespace";

        public static readonly string ZeroOrMore = "ZeroOrMore";

        public static readonly string ZeroOrOne = "ZeroOrOne";

        public static readonly string LowerCaseLetter = "LowerCaseLetter";

        public static readonly string UpperCaseLetter = "UpperCaseLetter";
        
        public static readonly string Identifier = "Identifier";
        public static readonly string DoubleQuotedString = "DoubleQuotedString";
        public static readonly string SingleQuotedString = "SingleQuotedString";
        public static readonly string Assignment = "Assignment";
        public static readonly string EndStatement = "EndStatement";
        public static readonly string Elipsis = "Elipsis";
        public static readonly string Option = "Option";
        public static readonly string GroupStart = "GroupStart";
        public static readonly string GroupEnd = "GroupEnd";
        public static readonly string ZeroOrOneOperator = "ZeroOrOneOperator";
        public static readonly string ZeroOrMoreOperator = "ZeroOrMoreOperator";
        public static readonly string OneOrMoreOperator = "OneOrMoreOperator";
        public static readonly string NotOperator = "NotOperator";

        public static readonly string EndOfLine = "EOL";

        public static readonly string SingleLineComment = "SingleLineComment";
        public static readonly string MultiLineComment = "MultiLineComment";

        public static readonly string MarkError = "Error";

        public static string GetPrefix(this AnnotationProduct production)
        {
            return production switch
            {
                AnnotationProduct.Annotation => string.Empty,
                AnnotationProduct.Transitive => TransitiveProductPrefix,
                AnnotationProduct.None => NoProductPrefix,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
