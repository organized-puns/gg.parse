// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.script.parser;
using gg.parse.script.pipeline;
using gg.parse.util;

namespace gg.parse.script
{
    public class ParserBuilder
    {
        public MutableRuleGraph<char>? TokenGraph {get;set;}

        public MutableRuleGraph<int>? GrammarGraph { get; set; }

        public ScriptLogger? LogHandler { get; set; }

        public PipelineSession<char>? TokenSession { get; private set; }

        public PipelineSession<int>? GrammarSession { get; private set; }

        public Parser Build()
        {
            Assertions.RequiresNotNull(TokenGraph);

            return GrammarGraph != null
                ? new Parser(TokenGraph.ToImmutable(), GrammarGraph.ToImmutable())
                : new Parser(TokenGraph.ToImmutable());
        }

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
    }
}
