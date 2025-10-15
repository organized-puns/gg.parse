using gg.parse.script.common;
using gg.parse.script.pipeline;
using gg.parse.util;

namespace gg.parse.script
{
    public class ParserBuilder
    {
        public RuleGraph<char>? TokenGraph {get;set;}

        public RuleGraph<int>? GrammarGraph { get; set; }

        public PipelineLogger? LogHandler { get; set; }

        public PipelineSession<char>? TokenSession { get; private set; }

        public PipelineSession<int>? GrammarSession { get; private set; }


        public ParserBuilder FromFile(
            string tokensFilename, 
            string? grammarFilename = null, 
            PipelineLogger? logger = null,
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
            PipelineLogger? logger = null,
            HashSet<string>? includePaths = null)
        {
            LogHandler = logger ?? new PipelineLogger();

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
            bool failOnWarning = false,
            bool throwExceptionsOnError = true)
        {
            Assertions.RequiresNotNull(TokenGraph!);
            Assertions.RequiresNotNull(TokenGraph!.Root!);

            try
            {
                return TokenGraph.Tokenize(input, failOnWarning, throwExceptionsOnError);
            }
            catch (Exception e)
            {
                LogHandler?.ProcessException(e);
                throw;
            }
        }

        public (ParseResult tokens, ParseResult syntaxTree) Parse(
            string input, 
            bool failOnWarning = false,
            bool throwExceptionsOnError = true)
        {
            Assertions.RequiresNotNull(TokenGraph!);
            Assertions.RequiresNotNull(TokenGraph!.Root!);

            try
            {
                return GrammarGraph == null
                        ? (TokenGraph.Tokenize(input, failOnWarning, throwExceptionsOnError), ParseResult.Unknown)
                        : GrammarGraph.Parse(TokenGraph, input, failOnWarning, throwExceptionsOnError);
            }
            catch (Exception e)
            {
                LogHandler?.ProcessException(e);
                throw;
            }
        }
    }
}
