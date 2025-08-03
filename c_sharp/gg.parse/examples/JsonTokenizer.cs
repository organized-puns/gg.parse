using gg.parse.rulefunctions;
using System.Text;
using static gg.parse.rulefunctions.TokenNames;

namespace gg.parse.examples
{
    public class JsonTokenizer : BasicTokensTable
    {
        public RuleBase<char> Root { get; private set; }

        public JsonTokenizer()
        {
            var jsonTokens = 
                OneOf("#JsonTokens", AnnotationProduct.Transitive,
                    Float(),
                    Integer(),
                    // need to override otherwise the name will hold the delimiter which
                    // will interfere with the style lookup in html
                    String(TokenNames.String, AnnotationProduct.Annotation),
                    Boolean(),
                    Literal("{", ScopeStart),
                    Literal("}", ScopeEnd),
                    Literal("[", ArrayStart),
                    Literal("]", ArrayEnd),
                    Literal("null", Null),
                    Literal(",", CollectionSeparator),
                    Literal(":", KeyValueSeparator));

            var error = Error(UnknownToken, AnnotationProduct.Annotation,
                "Can't match the character at the given position to a token.", jsonTokens, 0);

            Root = ZeroOrMore("#JsonTokenizer", AnnotationProduct.Transitive,
                                OneOf("#WhiteSpaceTokenOrError", AnnotationProduct.Transitive, Whitespace(), jsonTokens, error));
        }

        public ParseResult Tokenize(string text) => Root.Parse(text.ToCharArray(), 0);

        public (ParseResult, string) ParseFile(string path)
        {
            var text = File.ReadAllText(path);
            return (Tokenize(text), text);
        }

        public Dictionary<string, string> CreateTokenStyleLookup()
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
                { TokenNames.UnknownToken, "background-color: #ff7080; color: #050305;" }
            };
        }

        public string AnnotateTextUsingHtml(
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
                    builder.Append($"<span class=\"{FindRule(annotation.FunctionId).Name}\">");
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
