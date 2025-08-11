using gg.parse.rulefunctions;
using System.Text;

namespace gg.parse.examples
{
    public static class AnnotationMarkup
    {
        public static Dictionary<string, string> CreateTokenStyleLookup()
        {
            return new Dictionary<string, string>
            {
                { TokenNames.Integer, "color: #9075F0;" },
                
                { TokenNames.DoubleQuotedString, "color: #b0e055;" },
                { TokenNames.SingleQuotedString, "color: #b0e055;" },
                { TokenNames.AnyCharacter, "color: #b0e055;" },

                { TokenNames.Identifier, "color: #EEFEFF;" },
                { TokenNames.Null, "color: #95a095; font-style: italic;" },
                { TokenNames.Boolean, "color: #45aA30;" },

                { TokenNames.ScopeStart, "color: #f7aedc;" },
                { TokenNames.ScopeEnd, "color: #f7aedc;" },
                { TokenNames.GroupStart, "color: #bcaef7;" },
                { TokenNames.GroupEnd, "color: #bcaef7;" },

                { TokenNames.CollectionSeparator, "color: #f7eeec;" },
                { TokenNames.KeyValueSeparator, "color: #f78c6c;" },
                { TokenNames.EndStatement, "color: #f79c7e;" },

                { TokenNames.Assignment, "color: #F79FFe;" },
                { TokenNames.Elipsis, "color: #c79FFe;" },
                { TokenNames.ZeroOrMoreOperator, "color: #c79FFe;" },
                { TokenNames.ZeroOrOneOperator, "color: #c79FFe;" },
                { TokenNames.OneOrMoreOperator, "color: #c79FFe;" },
                { TokenNames.Option, "color: #c79FFe;" },
                { TokenNames.NotOperator, "color: #c79FFe;" },

                { TokenNames.NoProductSelector, "color: #A0A3AA;" },
                { TokenNames.TransitiveSelector, "color: #B0B3BA;" },

                { TokenNames.SingleLineComment, "color: #15AA20;" },
                { TokenNames.MultiLineComment, "color: #15AA20;" },

                { TokenNames.UnknownToken, "background-color: #ff7080; color: #050305;" },
                { TokenNames.EndOfLine, $"background-color: #6606BB;}} .{TokenNames.EndOfLine}::before {{content: \"\\21A9\";" }
            };
        }

        public static string AnnotateTextUsingHtml(
            this RuleGraph<char> ruleTable,
            string sourceText,
            List<Annotation> tokens,
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
                    builder.Append(sourceText.Substring(outputIndex, annotation.Range.Start - outputIndex));
                    outputIndex = annotation.Range.Start;
                }

                builder.Append($"<span class=\"{ruleTable.FindRule(annotation.FunctionId).Name}\">");

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

