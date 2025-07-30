using System.Text;

namespace gg.parse.tokenizer
{
    public static class TokenizerTools
    {
        public static class TokenNames
        {
            public static readonly string ObjectDelimiter = "ObjectDelimiter";
            public static readonly string ArrayDelimiter = "ArrayDelimiter";
            public static readonly string NullKeyword = "Null";
            public static readonly string CollectionSeparator = "CollectionSeparator";
            public static readonly string ArraySeparator = "ArraySeparator";
            public static readonly string KeyValueSeparator = "KeyValueSeparator";
        }

        public static Tokenizer CreateJsonTokenizer()
        {
            return new Tokenizer()
                    .Float()
                    .Integer()
                    .String()
                    .Boolean()
                    .Delimiter(["{", "}"], TokenNames.ObjectDelimiter)
                    .Delimiter(["[", "]"], TokenNames.ArrayDelimiter)
                    .Literal(TokenNames.NullKeyword, "null")
                    .Literal(TokenNames.CollectionSeparator, ",")
                    .Literal(TokenNames.KeyValueSeparator, ":")
                    .Whitespace();
                    
        }

        public static Dictionary<string, string> CreateJsonStyleLookup()
        {
            return new Dictionary<string, string>
            {
                { BasicTokenizerFunctions.TokenNames.Float, "color: #7065E0;" },
                { BasicTokenizerFunctions.TokenNames.Integer, "color: #9075F0;" },
                { BasicTokenizerFunctions.TokenNames.Boolean, "color: #45aA30;" },
                { BasicTokenizerFunctions.TokenNames.String, "color: #b0e055;" },
                { TokenNames.NullKeyword, "color: #95a095; font-style: italic;" },
                { TokenNames.ObjectDelimiter, "color: #f7aedc;" },
                { TokenNames.ArrayDelimiter, "color: #bcaef7;" },
                { TokenNames.CollectionSeparator, "color: #f7eeec;" },
                { TokenNames.KeyValueSeparator, "color: #f78c6c;" },
                { Tokenizer.ErrNoMatch, "background-color: #ff7080; color: #050305;" }
            };
        }

        public static string AnnotateTextUsingHtml(this Tokenizer tokenizer,
            string text, 
            List<Annotation> annotations,
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

                if (annotation.Category == AnnotationCategory.Token)
                {
                    builder.Append($"<span class=\"{tokenizer[annotation.ReferenceId].Name}\">");
                }
                else
                {
                    builder.Append($"<span class=\"{tokenizer.FindError(annotation.ReferenceId).Name}\">");
                }

                builder.Append(text.Substring(annotation.Range.Start, annotation.Range.Length));

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
