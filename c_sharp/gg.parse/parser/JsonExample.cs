using System.Text;

using gg.parse.basefunctions;

using static gg.parse.parser.BaseTokenizerFunctions;
using static gg.parse.parser.TokenNames;

namespace gg.parse.parser
{
    public static class JsonExample
    {
        public static ParseFunctionBase<char> CreateJsonTokenizer()
        {
            var matchToken = OneOf("Token", ProductionEnum.ProduceItem,
                Float(),
                Integer(),
                String(),
                Boolean(),
                Literal("{", ScopeStart),
                Literal("}", ScopeEnd),
                Literal("[", ArrayStart),
                Literal("]", ArrayEnd),
                Literal("null", Null),
                Literal(",", CollectionSeparator),
                Literal(":", KeyValueSeparator),
                Whitespace()
            );

            var handleUnknownToken =
                new MarkError<char>("UnknownTokenError", 0, "Can't match the character at the given position.", matchToken);

            return ZeroOrMore("Root", ProductionEnum.ProduceItem,
                function: OneOf("TokenOrError", ProductionEnum.ProduceItem, matchToken, handleUnknownToken));
        }
            

        public static BasicTokenizer<char> CreateTokenizer() =>
            new(
                Float(),
                Integer(),
                String(),
                Boolean(),
                Literal("{", ScopeStart),
                Literal("}", ScopeEnd),
                Literal("[", ArrayStart),
                Literal("]", ArrayEnd),
                Literal("null", Null),
                Literal(",", CollectionSeparator),
                Literal(":", KeyValueSeparator),
                Whitespace()
            );

            

        public static Dictionary<string, string> CreateTokenStyleLookup()
        {
            return new Dictionary<string, string>
            {
                { TokenNames.Float, "color: #7065E0;" },
                { TokenNames.Integer, "color: #9075F0;" },
                { TokenNames.Boolean, "color: #45aA30;" },
                { TokenNames.String, "color: #b0e055;" },
                { TokenNames.Null, "color: #95a095; font-style: italic;" },
                { TokenNames.ScopeStart, "color: #f7aedc;" },
                { TokenNames.ScopeEnd, "color: #f7aedc;" },
                { TokenNames.ArrayStart, "color: #bcaef7;" },
                { TokenNames.ArrayEnd, "color: #bcaef7;" },
                { TokenNames.CollectionSeparator, "color: #f7eeec;" },
                { TokenNames.KeyValueSeparator, "color: #f78c6c;" },
                { BasicTokenizer<char>.ErrNoMatch, "background-color: #ff7080; color: #050305;" }
            };
        }

        public static BasicTokenizer<int> CreateJsonParser(BasicTokenizer<char> tokenizer)
        {
            var dict = tokenizer.GetTokenDictionary();
            
            var key = BasicParserFunctions.Token("Key", dict[TokenNames.String]);
            var stringValue = BasicParserFunctions.Token("String", dict[TokenNames.String]);
            var boolValue = BasicParserFunctions.Token("Boolean", dict[TokenNames.Boolean]);
            var intValue = BasicParserFunctions.Token("Integer", dict[TokenNames.Integer]);
            var floatValue = BasicParserFunctions.Token("Float", dict[TokenNames.Float]);
            var nullValue = BasicParserFunctions.Token("Null", dict[TokenNames.Null]);
            var kvpSeparator = BasicParserFunctions.Token("KvpSeparator", dict[TokenNames.KeyValueSeparator], ProductionEnum.Ignore);
            var value = new MatchOneOfFunction<int>("Value", -1, ProductionEnum.ProduceItem,
                stringValue, boolValue, intValue, floatValue, nullValue);

            var keyValuePair = new MatchFunctionSequence<int>("KeyPairValue", -1, ProductionEnum.ProduceItem,
                key, kvpSeparator, value);

            return new BasicTokenizer<int>(keyValuePair);
        }

        public static string AnnotateTextUsingHtml(this BasicTokenizer<char> tokenizer,
            string text,
            List<Annotation> annotations,
            Dictionary<string, string> styleLookup)
        {
            var builder = new StringBuilder();

            builder.AppendLine("<html>");
            builder.AppendLine("    <style>");
            builder.AppendLine("        body { white-space: pre; font-family: 'Fira Code', 'JetBrains Mono', 'Source Code Pro', 'Cascadia Code', monospace;  font-size: 14px; line-height: 1.6; background-color: #222823 }");
            builder.AppendLine("        /* Tokens and their corresponding colors. */");

            foreach (var kvp in styleLookup)
            {
                builder.AppendLine($"        .{kvp.Key} {{ {kvp.Value} }}");
            }

            builder.AppendLine("    </style>");

            builder.AppendLine("    <body>");

            var outputIndex = 0;

            for (var i = 0; i < annotations.Count; i++)
            {
                var annotation = annotations[i];

                if (annotation.Range.Start > outputIndex)
                {
                    builder.Append(text.Substring(outputIndex, annotation.Range.Start - outputIndex));
                    outputIndex = annotation.Range.Start;
                }

                if (annotation.Category == AnnotationDataCategory.Data || annotation.Category == AnnotationDataCategory.Error)
                {
                    builder.Append($"<span class=\"{tokenizer.FindFunctionBase(annotation.FunctionId).Name}\">");
                }
                

                builder.Append(text.AsSpan(annotation.Range.Start, annotation.Range.Length));

                outputIndex += annotation.Range.Length;
                builder.Append("</span>");
            }

            builder.AppendLine("\n    </body>");
            builder.AppendLine("</html>");

            return builder.ToString();
        }
    }
}
