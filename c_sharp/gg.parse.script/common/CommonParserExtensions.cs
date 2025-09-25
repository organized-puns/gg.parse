using gg.parse.script.parser;

using ParseOutput = (
    System.Collections.Generic.List<gg.parse.Annotation> tokens,
    System.Collections.Generic.List<gg.parse.Annotation> astNodes);

namespace gg.parse.script.common
{
    public static class CommonParserExtensions
    {
        // -- Common Methods -----------------------------------------------------------------------------------------

        public static ParseOutput Parse(
                this RuleGraph<int> parser, 
                RuleGraph<char> tokenizer, 
                string text, 
                bool failOnWarning = false
            )
        {
            if (!string.IsNullOrEmpty(text))
            {
                var tokenizerTokens = tokenizer.TokenizeText(text);

                if (tokenizerTokens != null && tokenizerTokens.Count > 0)
                {
                    return parser.ParseGrammar(text, tokenizerTokens, failOnWarning);
                }
            }

            return ([], []);
        }

        public static List<Annotation> TokenizeText(this RuleGraph<char> tokenizer, string text)
        {
            Assertions.RequiresNotNullOrEmpty(text, nameof(text));

            var tokenizationResult = tokenizer.Root.Parse(text);

            if (tokenizationResult.FoundMatch && tokenizationResult.Annotations != null)
            {
                if (tokenizationResult
                        .Annotations
                        // xxx fix this, should work the same as parser
                        .ContainsRule(tokenizer.FindRule(CommonTokenNames.UnknownToken)!, out var tokenizerErrors)
                )
                {
                    throw new TokenizeException(
                        "input contains characters which could not be mapped to a token.",
                        tokenizerErrors,
                        text
                    );
                }

                return tokenizationResult.Annotations!;
            }

            throw new TokenizeException("input contains no valid tokens.");
        }

        public static (List<Annotation> tokens, List<Annotation> astNodes) ParseGrammar(
            this RuleGraph<int> parser, 
            string text, 
            in List<Annotation> tokens,
            bool failOnWarning = false)
        {
            var astResult = parser.Root!.Parse(tokens);

            if (astResult.FoundMatch)
            {
                var astNodes = astResult.Annotations;

                if (astNodes == null)
                {
                    throw new ParseException("input contains no valid grammar.");
                }

                if (astNodes.ContainsParseErrors(failOnWarning, out var grammarErrors))
                {
                    throw new ParseException(
                            "Parsing encountered some errors (or warnings which are treated as errors).",
                            grammarErrors,
                            text,
                            tokens
                    );
                }

                return (tokens, astResult.Annotations!);
            }
            else
            {
                return (tokens, []);
            }
        }
    }
}
