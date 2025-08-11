using System.Text;
using gg.parse.rulefunctions;

using static gg.parse.rulefunctions.CommonRuleTableRules;

namespace gg.parse.examples
{
    public static class JsonNodeNames
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

    public class JsonParser : RuleGraph<int>
    {
        
        public JsonTokenizer Tokenizer { get; init; }
     

        private AnnotationProduct _defaultProduct = AnnotationProduct.Annotation;

        public JsonParser() 
            : this(new JsonTokenizer())
        {
        }

        public JsonParser(JsonTokenizer tokenizer)
        {
            Tokenizer = tokenizer;

            _defaultProduct = AnnotationProduct.Annotation;

            var key = Token(JsonNodeNames.Key, TokenId(TokenNames.String));
            var stringValue = Token(JsonNodeNames.String, TokenId(TokenNames.String));
            var intValue = Token(JsonNodeNames.Integer, TokenId(TokenNames.Integer));
            var floatValue = Token(JsonNodeNames.Float, TokenId(TokenNames.Float));
            var boolValue = Token(JsonNodeNames.Boolean, TokenId(TokenNames.Boolean));
            var nullValue = Token(JsonNodeNames.Null, TokenId(TokenNames.Null));

            // value = string | int | float | bool | null
            var value = this.OneOf(JsonNodeNames.Value, AnnotationProduct.Annotation, stringValue, intValue, floatValue, boolValue, nullValue);

            _defaultProduct = AnnotationProduct.None;
            
            var keyValueSeparator = Token(TokenNames.KeyValueSeparator);
            var objectStart = Token(TokenNames.ScopeStart);
            var objectEnd = Token(TokenNames.ScopeEnd);
            var arrayStart = Token(TokenNames.ArrayStart);
            var arrayEnd = Token(TokenNames.ArrayEnd);
            var comma = Token(TokenNames.CollectionSeparator);

            
            // kv = string kv_separator value
            // kv_list = kv *(collection_separator kv)
            // example of recovery
            var keyValueMatch = this.Sequence(JsonNodeNames.KeyValuePair, AnnotationProduct.Annotation, key, keyValueSeparator, value);
            var objRecovery = this.OneOf(value, comma, objectEnd);
            var errorValueMissing = RegisterRule(new MarkError<int>("err_missing_value", AnnotationProduct.Annotation, testFunction: objRecovery));
            var valueMissingMatch = this.Sequence("#value_missing", AnnotationProduct.Transitive, key, keyValueSeparator, errorValueMissing);
            var separatorMissingMatch = this.Sequence("#kv_sep_missing", AnnotationProduct.Transitive, key, errorValueMissing);
            var keyValue = this.OneOf("#kvp_with_recovery", AnnotationProduct.Transitive, keyValueMatch, valueMissingMatch, separatorMissingMatch);

            var nextKeyValue = this.Sequence("#NextKeyValue", AnnotationProduct.Transitive, comma, keyValue);
            var keyValueList = this.Sequence("#KeyValueList", AnnotationProduct.Transitive, keyValue,
                this.ZeroOrMore("#KeyValueListRest", AnnotationProduct.Transitive, nextKeyValue));
            
            // jsonObj = scope_start ?(kv_list) scope_end
            var jsonObject = this.Sequence(JsonNodeNames.Object, AnnotationProduct.Annotation,
                objectStart, this.ZeroOrOne("#ObjectProperties", AnnotationProduct.Transitive, keyValueList), objectEnd);

            // jsonArray = array_start ?(value *(collection_separator value)) array_end
            var nextValue = this.Sequence("#NextValue", AnnotationProduct.Transitive, comma, value);
            var valueList = this.Sequence("#ValueList", AnnotationProduct.Transitive, value,
                this.ZeroOrMore("#ValueListRest", AnnotationProduct.Transitive, nextValue));
            var jsonArray = this.Sequence(JsonNodeNames.Array, AnnotationProduct.Annotation,
                arrayStart, this.ZeroOrOne("#ArrayValues", AnnotationProduct.Transitive, valueList), arrayEnd);

            value.RuleOptions = [.. value.RuleOptions, jsonObject, jsonArray];

            // todo error(s)

            Root = this.OneOf("~JsonRoot", AnnotationProduct.Transitive, jsonObject, jsonArray);
        }

        public int TokenId(string name) => Tokenizer.FindRule(name).Id;

        public RuleBase<int> Token(string tokenName) => Token(tokenName, _defaultProduct);
        
        public RuleBase<int> Token(string tokenName, AnnotationProduct product)
        {
            var rule = Tokenizer.FindRule(tokenName);
            return Single($"{product.GetPrefix()}Token({rule.Name})", product, rule.Id);
        }

        public RuleBase<int> Token(string name, int tokenId) => Single(name, _defaultProduct, tokenId);

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

                if (tokenResults.FoundMatch)
                {
                    if (tokenResults.Annotations != null)
                    {
                        var astResults = Parse(tokenResults.Annotations);

                        if (astResults.FoundMatch)
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
                { JsonNodeNames.Float, "background-color: #D9ABEF; display: inline; margin: 2px;" },
                { JsonNodeNames.Integer, "background-color: #D9ABEF; display: inline; margin: 2px;" },
                { JsonNodeNames.Boolean, "background-color: #D9EFA8; display: inline; margin: 2px;"  },
                { JsonNodeNames.String, "background-color: #CFEEEE; display: inline; margin: 2px;" },
                { JsonNodeNames.Null, "background-color: #A1A6AA; display: inline; margin: 2px;" },
                { JsonNodeNames.KeyValuePair, "background-color: #8877A0; display: inline; padding: 2px;" },
                { JsonNodeNames.Key, "background-color: #EFEBB9; display: inline; margin: 2px;" },
                { JsonNodeNames.Value, "background-color: #EEABD9; display: inline; padding: 2px;" },
                { JsonNodeNames.Object, "background-color: #AABBC9; display: inline; padding: 3px;"},
                { JsonNodeNames.Array, "background-color: #AFC0CF; display: inline; padding: 3px;" },
                { TokenNames.UnknownToken, "background-color: #FFE0DA; display: inline; padding: 3px;" },
            };
        }

        public ParseResult Parse(List<Annotation> tokens)
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

            builder.Append($"<div class=\"{FindRule(annotation.FunctionId).Name}\">");

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
