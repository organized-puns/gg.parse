// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.script.parser;
using gg.parse.util;

using System.Collections.Immutable;

using ParseOutput = (gg.parse.core.ParseResult tokeninzeResult, gg.parse.core.ParseResult parseResult);

namespace gg.parse.script.common
{
    public static class CommonParserExtensions
    {
        // -- Common Methods -----------------------------------------------------------------------------------------

        public static ParseOutput Parse(
                this MutableRuleGraph<int> parser, 
                MutableRuleGraph<char> tokenizer, 
                string text, 
                string? usingRule = null,
                bool failOnWarning = false,
                bool throwExceptionOnTokenizeErrors = true
            )
        {
            if (!string.IsNullOrEmpty(text))
            {
                var tokenizeResult = tokenizer.Tokenize(text, failOnWarning, throwExceptionOnTokenizeErrors);

                if (tokenizeResult.FoundMatch)
                {
                    if (tokenizeResult.Annotations != null && tokenizeResult.Annotations.Count > 0)
                    {
                        return (tokenizeResult,
                                parser.ParseGrammar(text, tokenizeResult.Annotations, usingRule, failOnWarning));
                      
                    }
                }

                return (tokenizeResult, ParseResult.Failure);

            }

            return (ParseResult.Failure, ParseResult.Failure);
        }

        public static ParseResult Tokenize(
            this MutableRuleGraph<char> tokenizer, 
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
            this MutableRuleGraph<int> parser, 
            string text, 
            ImmutableList<Annotation> tokens,
            string? usingRule = null,
            bool failOnWarning = false,
            bool throwExceptionOnErrors = true)
        {
            IRule? rule;

            if (!string.IsNullOrEmpty(usingRule))
            {
                rule = parser.FindRule(usingRule);

                if (rule == null)
                {
                    throw new ArgumentException($"No rule {usingRule} defined");
                }
            }
            else
            {
                rule = parser.Root!;
            }

            var astResult = rule.Parse(tokens);

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
