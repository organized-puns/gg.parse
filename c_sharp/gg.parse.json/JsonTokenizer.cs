
using gg.parse.core;
using gg.parse.script.common;

using static gg.parse.script.common.CommonTokenNames;

namespace gg.parse.json
{
    public class JsonTokenizer : CommonTokenizer
    {     
        public JsonTokenizer()
        {
            var jsonTokens = OneOf(
                    "-r jsonTokens",

                    Float(CommonTokenNames.Float),
                    Integer(CommonTokenNames.Integer),
                    // need to override otherwise the name will hold the delimiter which
                    // will interfere with the style lookup in html
                    MatchString(CommonTokenNames.String),
                    Boolean(CommonTokenNames.Boolean),
                    Literal(CommonTokenNames.ScopeStart, "{"),
                    Literal(CommonTokenNames.ScopeEnd, "}"),
                    Literal(CommonTokenNames.ArrayStart, "["),
                    Literal(CommonTokenNames.ArrayEnd, "]"),
                    Literal(CommonTokenNames.Null, "null"),
                    Literal(CommonTokenNames.CollectionSeparator, ","),
                    Literal(CommonTokenNames.KeyValueSeparator, ":")
                );

            var error = 
                Error(
                    UnknownToken, 
                    "Can't match the character at the given position to a token.", 
                    Skip(jsonTokens, failOnEoF: false)
                );

            Root = ZeroOrMore("-r jsonTokenizer", OneOf("-r whiteSpaceTokenOrError", Whitespace(), jsonTokens, error));
        }

        public ParseResult Tokenize(string text) => Root!.Parse(text);

        public (ParseResult, string) ParseFile(string path)
        {
            var text = File.ReadAllText(path);
            return (Tokenize(text), text);
        }
    }
}
