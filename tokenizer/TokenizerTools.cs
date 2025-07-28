using System.Text;

namespace gg.parse.tokenizer
{
    public static class TokenizerTools
    {
        public static Tokenizer CreateJsonTokenizer()
        {
            return null; // Implementation of JSON tokenizer creation should be here.
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

                builder.Append($"<span class=\"{tokenizer[annotation.FunctionId].Name}\">");
                builder.Append(AddHtmlWhitespace(text.Substring(annotation.Range.Start, annotation.Range.Length)));

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
