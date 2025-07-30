using gg.parse.basefunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gg.parse.parser
{
    public static class JsonExample
    {
        public static BasicParser<char> CreateTokenizer() =>
            new BasicParser<char>(
                //BaseTokenizerFunctions.Float(),
                BaseTokenizerFunctions.Integer(),
                BaseTokenizerFunctions.String()
                //BaseTokenizerFunctions.Boolean(),
                //BaseTokenizerFunctions.Literal(TokenNames.ScopeStart, "{"),
                //BaseTokenizerFunctions.Literal(TokenNames.ScopeEnd, "}"),
                //BaseTokenizerFunctions.Literal(TokenNames.ArrayStart, "["),
                //BaseTokenizerFunctions.Literal(TokenNames.ArrayEnd, "]"),
                //BaseTokenizerFunctions.Literal(TokenNames.Null, "null"),
                //BaseTokenizerFunctions.Literal(TokenNames.CollectionSeparator, ","),
                //BaseTokenizerFunctions.Literal(TokenNames.KeyValueSeparator, ":"),
                //BaseTokenizerFunctions.Whitespace,
            );
    }
}
