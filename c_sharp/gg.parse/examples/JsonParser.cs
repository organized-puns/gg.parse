using gg.parse.basefunctions;
using gg.parse.rulefunctions;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace gg.parse.examples
{
    public static class ParserNames
    {
        public static readonly string Key = "Key";
        public static readonly string String = "String";
        public static readonly string Integer = "Int";
        internal static readonly string Float = "Float";
        internal static readonly string Boolean = "Bool";
        internal static readonly string Null = "Null";
        internal static readonly string Value = "Value";
        internal static readonly string KeyValuePair = "KeyValuePair";
        internal static readonly string Object = "Object";
        internal static readonly string Array = "Array";
    }

    public class JsonParser : RuleTable<int>
    {
        
        public JsonTokenizer Tokenizer { get; init; }

        public RuleBase<int> Root { get; private set; }

        public JsonParser() 
            : this(new JsonTokenizer())
        {
        }

        public JsonParser(JsonTokenizer tokenizer)
        {
            Tokenizer = tokenizer;

            var key = Token(ParserNames.Key, AnnotationProduct.Annotation, TokenId(TokenNames.String));
            var stringValue = Token(ParserNames.String, AnnotationProduct.Annotation, TokenId(TokenNames.String));
            var intValue = Token(ParserNames.Integer, AnnotationProduct.Annotation, TokenId(TokenNames.Integer));
            var floatValue = Token(ParserNames.Float, AnnotationProduct.Annotation, TokenId(TokenNames.Float));
            var boolValue = Token(ParserNames.Boolean, AnnotationProduct.Annotation, TokenId(TokenNames.Boolean));
            var nullValue = Token(ParserNames.Null, AnnotationProduct.Annotation, TokenId(TokenNames.Null));

            var value = OneOf(ParserNames.Value, AnnotationProduct.Annotation, stringValue, intValue, floatValue, boolValue, nullValue);
            var keyValueSeparator = Token(TokenNames.KeyValueSeparator);
            var objectStart = Token(TokenNames.ScopeStart);
            var objectEnd = Token(TokenNames.ScopeEnd);
            var arrayStart = Token(TokenNames.ArrayStart);
            var arrayEnd = Token(TokenNames.ArrayEnd);
            var comma = Token(TokenNames.CollectionSeparator);

            var keyValue = Sequence(ParserNames.KeyValuePair, AnnotationProduct.Annotation, key, keyValueSeparator, value);
            var nextKeyValue = Sequence("~NextKeyValue", AnnotationProduct.Transitive, comma, keyValue);
            var keyValueList = Sequence("~KeyValueList", AnnotationProduct.Transitive, keyValue,
                ZeroOrMore("~KeyValueListRest", AnnotationProduct.Transitive, nextKeyValue));

            var jsonObject = Sequence(ParserNames.Object, AnnotationProduct.Annotation,
                objectStart, ZeroOrMore("~ObjectProperties", AnnotationProduct.Transitive, keyValueList), objectEnd);

            var nextValue = Sequence("~NextValue", AnnotationProduct.Transitive, comma, value);
            var valueList = Sequence("~ValueList", AnnotationProduct.Transitive, value,
                ZeroOrMore("~ValueListRest", AnnotationProduct.Transitive, nextValue));
            var jsonArray = Sequence(ParserNames.Array, AnnotationProduct.Annotation,
                arrayStart, ZeroOrMore("~ArrayValues", AnnotationProduct.Transitive, valueList), arrayEnd);

            value.Options = [.. value.Options, jsonObject, jsonArray];

            // todo error(s)

            Root = OneOf("~JsonRoot", AnnotationProduct.Transitive, jsonObject, jsonArray);
        }

        public int TokenId(string name) => Tokenizer.FindRule(name).Id;

        public int[] TokenIds(params string[] names) => names.Select(n => TokenId(n)).ToArray();

        public RuleBase<int> Token(string tokenName) => Token(tokenName, AnnotationProduct.None);
        
        public RuleBase<int> Token(string tokenName, AnnotationProduct product)
        {
            var rule = Tokenizer.FindRule(tokenName);
            return Token($"{product.GetPrefix()}Token({rule.Name})", product, rule.Id);
        }

        public RuleBase<int> Token(string name, AnnotationProduct product, int tokenId) =>
            TryFindRule(name, out MatchSingleData<int>? existingRule)
                 ? existingRule!
                 : RegisterRule(new MatchSingleData<int>(name, tokenId, product));

        public ParseResult Tokenize(string text) => Tokenizer.Tokenize(text);

        public (List<Annotation> tokens, List<Annotation> astNodes, string text) ParseFile(string path)
        {
            var text = File.ReadAllText(path);
            var (tokens, astNodes) = Parse(text);
            return (tokens, astNodes, text);
        }
            

        public (List<Annotation> tokens, List<Annotation> astNodes) Parse(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var tokenResults = Tokenizer.Tokenize(text);

                if (tokenResults.IsSuccess)
                {
                    if (tokenResults.Annotations != null)
                    {
                        var astResults = Parse(tokenResults.Annotations);

                        if (astResults.IsSuccess)
                        {
                            return (tokenResults.Annotations, astResults.Annotations);
                        }
                    }
                    else
                    {
                        return ([], []);
                    }
                }
            }
            else
            {
                return ([], []);
            }

            throw new ArgumentException("Invalid input");
        }

        public Dictionary<string, string> CreateAstStyleLookup()
        {
            return new Dictionary<string, string>
            {
                { ParserNames.Float, "background-color: #D9ABEF; display: inline; margin: 2px;" },
                { ParserNames.Integer, "background-color: #D9ABEF; display: inline; margin: 2px;" },
                { ParserNames.Boolean, "background-color: #D9EFA8; display: inline; margin: 2px;"  },
                { ParserNames.String, "background-color: #CFEEEE; display: inline; margin: 2px;" },
                { ParserNames.Null, "background-color: #A1A6AA; display: inline; margin: 2px;" },
                { ParserNames.KeyValuePair, "background-color: #8877A0; display: inline; padding: 2px;" },
                { ParserNames.Key, "background-color: #EFEBB9; display: inline; margin: 2px;" },
                { ParserNames.Value, "background-color: #EEABD9; display: inline; padding: 2px;" },
                { ParserNames.Object, "background-color: #AABBC9; display: inline; padding: 3px;"},
                { ParserNames.Array, "background-color: #AFC0CF; display: inline; padding: 3px;" },
            };
        }

        public ParseResult Parse(List<basefunctions.Annotation> tokens)
        {
            return Root.Parse(tokens.Select(t => t.FunctionId).ToArray(), 0);
        }

        public string AnnotateTextUsingHtml(
            string text,
            List<Annotation> tokens,
            List<Annotation> astNodes,
            Dictionary<string, string> styleLookup)
        {
            var builder = new StringBuilder();

            builder.AppendLine("<html>");
            builder.AppendLine("    <style>");
            builder.AppendLine("        body { white-space: pre; font-family: 'Fira Code', 'JetBrains Mono', 'Source Code Pro', 'Cascadia Code', monospace;  font-size: 14px; line-height: 1.6; background-color: #CCDDEA }");
            builder.AppendLine("        /* Tokens and their corresponding colors. */");

            foreach (var kvp in styleLookup)
            {
                builder.AppendLine($"        .{kvp.Key} {{ {kvp.Value} }}");
            }

            builder.AppendLine("    </style>");

            builder.AppendLine("    <body>");

            var outputIndex = 0;

            for (var i = 0; i < astNodes.Count; i++)
            {
                outputIndex = AppendAnnotation(text, builder, astNodes[i], outputIndex, tokens);
            }

            builder.AppendLine("\n    </body>");
            builder.AppendLine("</html>");

            return builder.ToString();
        }

        private int AppendAnnotation(string text, StringBuilder builder, Annotation annotation, int writePosition, List<Annotation> tokens)
        {
            var textStart = tokens[annotation.Range.Start].Start;

            if (textStart > writePosition)
            {
                builder.Append(text.Substring(writePosition, textStart - writePosition));
                writePosition = textStart;
            }

            if (annotation.Category == AnnotationDataCategory.Data || annotation.Category == AnnotationDataCategory.Error)
            {
                builder.Append($"<div class=\"{FindRule(annotation.FunctionId).Name}\">");
            }

            if (annotation.Children == null || annotation.Children.Count == 0)
            {
                var textLength = 0;

                for (var i  = annotation.Range.Start; i < annotation.Range.Start + annotation.Range.Length; i++)
                {
                    textLength += tokens[i].Length;
                }

                builder.Append(text.AsSpan(textStart, textLength));
                writePosition += textLength;
            }
            else
            {
                var lastReadToken = annotation.Range.Start;
                foreach (var child in annotation.Children)
                {
                    lastReadToken = child.Range.Start + child.Range.Length;
                    writePosition = AppendAnnotation(text, builder, child, writePosition, tokens);
                }

                if (lastReadToken < annotation.End)
                {
                    // there are tokens in this node which were not tagged or part of the children,
                    // add the text of these remaining tokens to the html
                    // eg the closing } in an json object will generally be not annotated as an ast
                    // but should appear in the the text
                    for (var tokenIndex = lastReadToken; tokenIndex < annotation.End; tokenIndex++)
                    {
                        var token = tokens[tokenIndex];
                        builder.Append(text.Substring(writePosition, token.End - writePosition));
                        writePosition = token.End;
                    }
                }
            }

            builder.Append("</div>");

            return writePosition;
        }
    }
}
