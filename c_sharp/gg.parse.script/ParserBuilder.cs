// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.rules;
using gg.parse.script.common;
using gg.parse.script.pipeline;
using gg.parse.util;

namespace gg.parse.script
{
    public class ParserBuilder
    {
        public RuleGraph<char>? TokenGraph {get;set;}

        public RuleGraph<int>? GrammarGraph { get; set; }

        public ScriptLogger? LogHandler { get; set; }

        public PipelineSession<char>? TokenSession { get; private set; }

        public PipelineSession<int>? GrammarSession { get; private set; }


        public ParserBuilder FromFile(
            string tokensFilename, 
            string? grammarFilename = null, 
            ScriptLogger? logger = null,
            HashSet<string>? includePaths = null)
        {
            var sessionIncludePaths = includePaths ?? [AppContext.BaseDirectory];
            var fullTokenPath = tokensFilename.ResolveFile(sessionIncludePaths);
            var tokensDefinition = File.ReadAllText(fullTokenPath);

            sessionIncludePaths.Add(Path.GetDirectoryName(fullTokenPath)!);

            if (!string.IsNullOrEmpty(grammarFilename))
            {
                var fullGrammarPath = grammarFilename.ResolveFile(sessionIncludePaths);
                var grammarDefinition = File.ReadAllText(fullGrammarPath);

                sessionIncludePaths.Add(Path.GetDirectoryName(fullGrammarPath)!);

                return From(tokensDefinition, grammarDefinition, logger: logger, includePaths: sessionIncludePaths);
            }
            else
            {
                return From(tokensDefinition, logger: logger, includePaths: sessionIncludePaths);
            }
            
        }

        public ParserBuilder From(
            string tokenDefinition, 
            string? grammarDefinition = null, 
            ScriptLogger? logger = null,
            HashSet<string>? includePaths = null)
        {
            LogHandler = logger ?? new ScriptLogger();

            TokenSession = ScriptPipeline.RunTokenPipeline(tokenDefinition, LogHandler, includePaths);

            TokenGraph = TokenSession.RuleGraph!;

            if (grammarDefinition != null)
            {
                GrammarSession = ScriptPipeline.RunGrammarPipeline(grammarDefinition, TokenSession, includePaths);
                GrammarGraph = GrammarSession.RuleGraph;
            }

            return this;
        }

        public ParseResult Tokenize(
            string input,
            string? usingRule = null,
            bool failOnWarning = false,
            bool throwExceptionsOnError = true,
            bool processLogsOnResult = false)
        {
            Assertions.RequiresNotNull(TokenGraph!);
            Assertions.RequiresNotNull(TokenGraph!.Root!);

            try
            {
                ParseResult result;

                if (string.IsNullOrEmpty(usingRule))
                {
                    result = TokenGraph.Tokenize(input, failOnWarning, throwExceptionsOnError);
                }
                else
                {
                    var rule = TokenGraph.FindRule(usingRule);

                    result = rule != null
                        ? rule.Parse(input)
                        : throw new ArgumentException($"No rule {usingRule} defined in the token graph");
                }

                if (processLogsOnResult && LogHandler != null && result.Annotations != null)
                {
                    LogHandler.ProcessTokens(input, result.Annotations);
                }

                return result;
            }
            catch (Exception e)
            {
                LogHandler?.ProcessException(e);
                throw;
            }
        }

        public (ParseResult tokens, ParseResult syntaxTree) Parse(
            string input,
            string? usingRule = null,
            bool failOnWarning = false,
            bool throwExceptionsOnError = true,
            bool processLogsOnResult = false)
        {
            Assertions.RequiresNotNull(TokenGraph!);
            Assertions.RequiresNotNull(TokenGraph!.Root!);

            try
            {
                var (tokens, syntaxTree) = GrammarGraph == null
                        ? (TokenGraph.Tokenize(input, failOnWarning, throwExceptionsOnError), ParseResult.Unknown)
                        : GrammarGraph.Parse(TokenGraph, input, usingRule, failOnWarning, throwExceptionsOnError);

                if (processLogsOnResult && LogHandler != null)
                {
                    if (tokens.Annotations != null)
                    {
                        LogHandler.ProcessTokens(input, tokens.Annotations);

                        if (syntaxTree.Annotations != null)
                        {
                            LogHandler.ProcessSyntaxTree(input, tokens.Annotations, syntaxTree.Annotations);
                        }
                    }
                }

                return (tokens, syntaxTree);
            }
            catch (Exception e)
            {
                LogHandler?.ProcessException(e);
                throw;
            }
        }

        /// <summary>
        /// Convenience method to write the received logs during parsing / compilation
        /// to the given output action
        /// </summary>
        /// <param name="output"></param>
        /// <param name="includedLevels"></param>
        public void WriteLogs(Action<LogLevel,string> output, LogLevel includedLevels = LogLevel.Any)
        {
            if (LogHandler != null && LogHandler.ReceivedLogs != null)
            {
                LogHandler
                    .ReceivedLogs
                    .Where(log => (log.level & includedLevels) > 0)
                    .ForEach(log => output(log.level, log.message));
            }
        }
    }
}
