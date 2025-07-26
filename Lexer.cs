using System.Text;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace gg.parse
{
    public enum TokenAction
    {
        GenerateToken,
        IgnoreToken,
        Error
    }

    public class LexerError
    {
        public int Offset { get; set; }

        public int Length { get; set; }

        public string Message { get; set; } = string.Empty;
    }


    public class Lexer
    {
        private List<(IRule rule, TokenAction action)> _tokens = [];

        public void AddRule(IRule rule, TokenAction action = TokenAction.GenerateToken)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule), "Rule cannot be null");
            }

            _tokens.Add((rule, action));
        }

        public (List<ParseResult> tokens, List<LexerError> errors) Tokenize(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text), "Text cannot be null");
            }
            List<ParseResult> results = [];
            List<LexerError> errors = [];
            int offset = 0;
            int errorStart = -1;
            while (offset < text.Length)
            {
                var token = TryParse(text, offset);

                if (token.HasValue)
                {
                    if (errorStart >= 0)
                    {
                        errors.Add(new LexerError
                        {
                            Offset = errorStart,
                            Length = offset - errorStart,
                            Message = $"Unexpected characters at position {errorStart}"
                        });

                        errorStart = -1;
                    }
                    
                    if (token.Value.action == TokenAction.GenerateToken)
                    {
                        results.Add(token.Value.parseResult);
                    }

                    offset += token.Value.parseResult.Length;
                }
                else
                {
                    if (errorStart < 0)
                    {
                        errorStart = offset;
                    }
                    offset++;
                }
            }

            // are we still in an error state?
            if (errorStart >= 0)
            {
                errors.Add(new LexerError
                {
                    Offset = errorStart,
                    Length = offset - errorStart,
                    Message = $"Unexpected characters at position {errorStart}"
                });
            }

            return (results, errors);
        }

        public (ParseResult parseResult, TokenAction action)? TryParse(string text, int offset )
        {
            foreach (var (rule, action) in _tokens)
            {
                var result = rule.Parse(text, offset);

                if (result != null && result.ResultCode == ParseResult.Code.Success)
                {
                    return (result, action);
                }
            }

            return null;
        }

        public static readonly string ErrorTokenName = "Error";

        private void AppendNTimes(StringBuilder builder, string text, int count)
        {
            for (int i = 0; i < count; i++)
            {
                builder.Append(text);
            }
        }

        public string AnnotateTextUsingHtml(string text, List<ParseResult> tokens, List<LexerError> errors, 
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

            var tokenIndex = 0;
            var errorIndex = 0;
            var outputIndex = 0;

            while (tokenIndex < tokens.Count || errorIndex < errors.Count)
            {
                var obj = TakeNext(tokens, tokenIndex, errors, errorIndex);

                if (obj is ParseResult parseResult)
                {
                    if (parseResult.Offset > outputIndex)
                    {
                        builder.Append(AddHtmlWhitespace(text.Substring(outputIndex, parseResult.Offset - outputIndex)));
                        outputIndex = parseResult.Offset;
                    }

                    builder.Append($"<span class=\"{parseResult.Rule.Name}\">");
                    builder.Append(AddHtmlWhitespace(text.Substring(parseResult.Offset, parseResult.Length)));

                    tokenIndex++;
                    outputIndex += parseResult.Length; 
                }
                else if (obj is LexerError error)
                {
                    if (error.Offset > outputIndex)
                    {
                        builder.Append(AddHtmlWhitespace(text.Substring(outputIndex, error.Offset - outputIndex)));
                        outputIndex = error.Offset;
                    }

                    builder.Append($"<span class=\"{ErrorTokenName}\">");
                    builder.Append(AddHtmlWhitespace(text.Substring(error.Offset, error.Length)));
                    errorIndex++;
                    outputIndex += error.Length;
                }

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

        private object TakeNext(List<ParseResult> tokens, int tokenIndex, List<LexerError> errors, int errorIndex)
        {
            if (tokenIndex < tokens.Count && errorIndex < errors.Count)
            {
                if (tokens[tokenIndex].Offset < errors[errorIndex].Offset)
                {
                    return tokens[tokenIndex];
                }
                else
                {
                    return errors[errorIndex];
                }
            }
            else if (tokenIndex < tokens.Count)
            {
                return tokens[tokenIndex];
            }
            else
            {
                return errors[errorIndex];
            }
        }
    }
}
