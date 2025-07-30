using System.Text;

using gg.parse.basefunctions;

using static gg.parse.parser.BaseTokenizerFunctions;
using static gg.parse.parser.TokenNames;

namespace gg.parse.parser
{
    public static class JsonExample
    {
        public static BasicParser<char> CreateTokenizer() =>
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
                { BasicParser<char>.ErrNoMatch, "background-color: #ff7080; color: #050305;" }
            };
        }

        public static string AnnotateTextUsingHtml(this BasicParser<char> tokenizer,
            string text,
            List<AnnotationBase> annotations,
            Dictionary<string, string> styleLookup)
        {
            var builder = new StringBuilder();

            builder.AppendLine("<html>");
            builder.AppendLine("    <style>");
            builder.AppendLine("        body { font-family: 'Fira Code', 'JetBrains Mono', 'Source Code Pro', 'Cascadia Code', monospace;  font-size: 14px; line-height: 1.6; background-color: #222823 }");
            builder.AppendLine("        .indent { white-space: pre; }");
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
                    builder.Append(AddHtmlWhitespace(text.Substring(outputIndex, annotation.Range.Start - outputIndex)));
                    outputIndex = annotation.Range.Start;
                }

                if (annotation.Category == AnnotationDataCategory.Data)
                {
                    builder.Append($"<span class=\"{tokenizer.FindFunctionBase(annotation.FunctionId).Name}\">");
                }
                else
                {
                    builder.Append($"<span class=\"{tokenizer.FindErrorBase(annotation.FunctionId).Name}\">");
                }

                builder.Append(text.AsSpan(annotation.Range.Start, annotation.Range.Length));

                outputIndex += annotation.Range.Length;
                builder.Append("</span>");
            }

            builder.AppendLine("\n    </body>");
            builder.AppendLine("</html>");

            return builder.ToString();
        }

        private static string AddHtmlWhitespace(string text)
        {
            return "<span class=\"indent\">"
                    + text.Replace("\n", "<br/>\n").Replace("\r", "").Replace("\t", "   ")
                    + "</span>";
        }
    }
}
