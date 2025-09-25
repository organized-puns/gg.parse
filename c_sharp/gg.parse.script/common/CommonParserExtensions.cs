using gg.parse.script.parser;

using ParseOutput = (gg.parse.ParseResult tokeninzeResult, gg.parse.ParseResult parseResult);

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
                var tokenizeResult = tokenizer.TokenizeText(text);

                if (tokenizeResult.FoundMatch)
                {
                    if (tokenizeResult.Annotations != null && tokenizeResult.Annotations.Count > 0)
                    {
                        return (tokenizeResult,
                                parser.ParseGrammar(text, tokenizeResult.Annotations, failOnWarning));
                    }
                }

                return (tokenizeResult, ParseResult.Failure);

            }

            return (ParseResult.Failure, ParseResult.Failure);
        }

        public static ParseResult TokenizeText(this RuleGraph<char> tokenizer, string text)
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
            }

            return tokenizationResult;
        }

        public static ParseResult ParseGrammar(
            this RuleGraph<int> parser, 
            string text, 
            in List<Annotation> tokens,
            bool failOnWarning = false)
        {
            var astResult = parser.Root!.Parse(tokens);

            if (astResult.FoundMatch)
            {
                var astNodes = astResult.Annotations;

                if (astNodes != null && astNodes.ContainsParseErrors(failOnWarning, out var grammarErrors))
                {
                    throw new ParseException(
                            "Parsing encountered some errors (or warnings which are treated as errors).",
                            grammarErrors,
                            text,
                            tokens
                    );
                }
            }

            return astResult;
        }
    }
}
