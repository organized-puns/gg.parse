// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.script.parser;
using gg.parse.util;

namespace gg.parse.script
{
    public sealed class ParseConfiguration
    {
        public string Text { get; init; }

        public bool FailOnWarning { get; init; }

        public bool ThrowExceptionsOnError { get; init; }

        public bool ProcessLogsOnResult { get; init;}

        public ScriptLogger? LogHandler { get; set; }

        public ParseConfiguration(string text)
        {
            Assertions.RequiresNotNullOrEmpty(text);
            Text = text;
        }
    }

    public sealed class Parser
    {
        private readonly IRuleGraph<char> _tokens;
        private readonly IRuleGraph<int>? _grammar;
        public IRuleGraph<int>? Grammar =>
            _grammar;

        public IRuleGraph<char> Tokens =>
            _tokens;


        public Parser(RuleGraph<char> tokens)
        {
            _tokens = tokens;
        }

        public Parser(IRuleGraph<char> tokens, IRuleGraph<int> grammar)
        {
            _tokens = tokens;
            _grammar = grammar;
        }

        public ParseResult Tokenize(
            string input, 
            bool throwExceptions = true, 
            bool processLogs = false,
            string? usingRule = null,
            ScriptLogger? logger = null
        ) =>
            Tokenize(
                new ParseConfiguration(input)
                {
                    ProcessLogsOnResult = processLogs,
                    ThrowExceptionsOnError = throwExceptions,
                    LogHandler = logger
                },
                usingRule: usingRule
            );

        public ParseResult Tokenize(ParseConfiguration config, string? usingRule = null)
        {
            if (string.IsNullOrEmpty(config.Text))
            {
                return ParseResult.Success;
            }

            IRule rule = string.IsNullOrEmpty(usingRule)
                    ? _tokens.Root!
                    : _tokens[usingRule];

            var result = rule.Parse(config.Text);

            if (result.FoundMatch && result.Annotations != null)
            {
                if (config.ThrowExceptionsOnError
                    && result
                        .Annotations.ContainsErrors<char>(config.FailOnWarning, out var tokenizerErrors))
                {
                    var exception = new ScriptException(
                        "input contains characters which could not be mapped to a token.",
                        tokenizerErrors,
                        config.Text
                    );

                    config.LogHandler?.ProcessScriptException(exception);
                    
                    throw exception;
                }
            }
                
            if (config.ProcessLogsOnResult && config.LogHandler != null && result.Annotations != null)
            {
                config.LogHandler.ProcessTokens(config.Text, result.Annotations);
            }

            return result;
        }

        public (ParseResult tokens, ParseResult syntaxTree) Parse(
            string input, 
            bool throwExceptions = true, 
            bool processLogs = false,
            string? usingRule = null,
            ScriptLogger? logger = null
        ) =>
            Parse(
                new ParseConfiguration(input)
                {
                    ThrowExceptionsOnError = throwExceptions,
                    ProcessLogsOnResult = processLogs,
                    LogHandler = logger
                },
                usingRule
            );   

        public (ParseResult tokens, ParseResult syntaxTree) Parse(ParseConfiguration config, string? usingRule = null)
        {
            Assertions.RequiresNotNull(_grammar, "No grammar defined, did you want to use 'Tokenize' instead?");

            if (string.IsNullOrEmpty(config.Text))
            {
                return (ParseResult.Success, ParseResult.Success);
            }

            var tokenizeResult = Tokenize(config);

            if (tokenizeResult.FoundMatch)
            {
                if (tokenizeResult.Annotations != null && tokenizeResult.Annotations.Count > 0)
                {
                    IRule? rule = string.IsNullOrEmpty(usingRule)
                        ? _grammar.Root
                        : _grammar[usingRule];

                    var astResult = rule!.Parse(tokenizeResult.Annotations, 0);

                    if (astResult.FoundMatch)
                    {
                        var astNodes = astResult.Annotations;

                        if (astNodes != null
                            && config.ThrowExceptionsOnError
                            && astNodes.ContainsErrors<int>(config.FailOnWarning, out var grammarErrors))
                        {
                            var exception = new ScriptException(
                                    "Parsing encountered some errors (or warnings which are treated as errors).",
                                    grammarErrors,
                                    config.Text,
                                    tokenizeResult.Annotations
                            );

                            config.LogHandler?.ProcessException(exception);
                            throw exception;
                        }
                    }

                    return (tokenizeResult, astResult);
                }

            }

            return (tokenizeResult, ParseResult.Failure);
        }
    }
}
