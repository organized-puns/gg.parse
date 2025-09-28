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
                bool failOnWarning = false,
                bool throwExceptionOnTokenizeErrors = true
            )
        {
            if (!string.IsNullOrEmpty(text))
            {
                var tokenizeResult = tokenizer.TokenizeText(text, failOnWarning, throwExceptionOnTokenizeErrors);

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

        public static ParseResult TokenizeText(
            this RuleGraph<char> tokenizer, 
            string text, 
            bool failOnWarning = false,
            bool throwExceptionOnErrors = true)
        {
            Assertions.RequiresNotNullOrEmpty(text, nameof(text));
            Assertions.RequiresNotNull(tokenizer);
            Assertions.RequiresNotNull(tokenizer.Root!);

            var tokenizationResult = tokenizer.Root!.Parse(text);

            if (tokenizationResult.FoundMatch && tokenizationResult.Annotations != null)
            {
                if (throwExceptionOnErrors
                    && tokenizationResult.Annotations.ContainsErrors<char>(failOnWarning, out var tokenizerErrors))
                {
                    /*throw new TokenizeException(
                        "input contains characters which could not be mapped to a token.",
                        tokenizerErrors,
                        text
                    );*/
                    throw new ScriptException(
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
            bool failOnWarning = false,
            bool throwExceptionOnErrors = true)
        {
            var astResult = parser.Root!.Parse(tokens);

            if (astResult.FoundMatch)
            {
                var astNodes = astResult.Annotations;

                if (astNodes != null 
                    && throwExceptionOnErrors 
                    && astNodes.ContainsErrors<int>(failOnWarning, out var grammarErrors))
                {
                    throw new ScriptException(
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
