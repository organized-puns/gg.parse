using gg.parse.core;
using gg.parse.script.common;
using System.Collections.Immutable;
using System.Text;

namespace gg.parse.json
{
    public static class AnnotationMarkup
    {
        public static Dictionary<string, string> CreateTokenStyleLookup()
        {
            return new Dictionary<string, string>
            {
                { CommonTokenNames.Integer, "color: #9075F0;" },
                
                { CommonTokenNames.DoubleQuotedString, "color: #b0e055;" },
                { CommonTokenNames.SingleQuotedString, "color: #b0e055;" },
                { CommonTokenNames.AnyCharacter, "color: #b0e055;" },

                { CommonTokenNames.Identifier, "color: #EEFEFF;" },
                { CommonTokenNames.Null, "color: #95a095; font-style: italic;" },
                { CommonTokenNames.Boolean, "color: #45aA30;" },

                { CommonTokenNames.ScopeStart, "color: #f7aedc;" },
                { CommonTokenNames.ScopeEnd, "color: #f7aedc;" },
                { CommonTokenNames.GroupStart, "color: #bcaef7;" },
                { CommonTokenNames.GroupEnd, "color: #bcaef7;" },

                { CommonTokenNames.CollectionSeparator, "color: #f7eeec;" },
                { CommonTokenNames.KeyValueSeparator, "color: #f78c6c;" },
                { CommonTokenNames.EndStatement, "color: #f79c7e;" },

                { CommonTokenNames.Assignment, "color: #F79FFe;" },
                { CommonTokenNames.Elipsis, "color: #c79FFe;" },
                { CommonTokenNames.ZeroOrMoreOperator, "color: #c79FFe;" },
                { CommonTokenNames.ZeroOrOneOperator, "color: #c79FFe;" },
                { CommonTokenNames.OneOrMoreOperator, "color: #c79FFe;" },
                { CommonTokenNames.OneOf, "color: #c79FFe;" },
                { CommonTokenNames.NotOperator, "color: #c79FFe;" },

                { CommonTokenNames.PruneAll, "color: #A0A3AA;" },
                { CommonTokenNames.PruneRoot, "color: #B0B3BA;" },

                { CommonTokenNames.SingleLineComment, "color: #15AA20;" },
                { CommonTokenNames.MultiLineComment, "color: #15AA20;" },

                { CommonTokenNames.UnknownToken, "background-color: #ff7080; color: #050305;" },
                { CommonTokenNames.EndOfLine, $"background-color: #6606BB;}} .{CommonTokenNames.EndOfLine}::before {{content: \"\\21A9\";" }
            };
        }

        public static string AnnotateTextUsingHtml(
            string sourceText,
            ImmutableList<Annotation> tokens,
            Dictionary<string, string> styleLookup) 
        {
            var builder = new StringBuilder();

            builder.AppendLine("<html>");
            builder.AppendLine("    <style>");
            builder.AppendLine("        body { white-space: pre; tab-size: 4; font-family: 'Fira Code', 'JetBrains Mono', 'Source Code Pro', 'Cascadia Code', monospace;  font-size: 14px; line-height: 1.6; background-color: #222823; color: #99a099; }");
            builder.AppendLine("        /* Tokens and their corresponding colors. */");

            foreach (var kvp in styleLookup)
            {
                builder.AppendLine($"        .{kvp.Key} {{ {kvp.Value} }}");
            }

            builder.AppendLine("    </style>");

            builder.AppendLine("    <body>");

            var outputIndex = 0;

            for (var i = 0; i < tokens.Count; i++)
            {
                var annotation = tokens[i];

                if (annotation.Range.Start > outputIndex)
                {
                    builder.Append(sourceText.AsSpan(outputIndex, annotation.Range.Start - outputIndex));
                    outputIndex = annotation.Range.Start;
                }

                builder.Append($"<span class=\"{annotation.Rule.Name}\">");

                builder.Append(sourceText.AsSpan(annotation.Range.Start, annotation.Range.Length));

                outputIndex += annotation.Range.Length;
                builder.Append("</span>");
            }

            builder.AppendLine("\n    </body>");
            builder.AppendLine("</html>");

            return builder.ToString();
        }
    }
}

