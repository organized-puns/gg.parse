using gg.parse.script.common;
using gg.parse.script.parser;
using gg.parse.script.pipeline;

namespace gg.parse.script
{
    public class ParserBuilder
    {
        public RuleGraph<char>? TokenGraph {get;set;}

        public RuleGraph<int>? GrammarGraph { get; set; }

        public ScriptLogger? LogHandler { get; set; }

        public PipelineSession<char>? TokenSession { get; private set; }

        public PipelineSession<int>? GrammarSession { get; private set; }

        public ParserBuilder From(string tokenDefinition, string? grammarDefinition = null, ScriptLogger? logger = null)
        {
            LogHandler = logger ?? new ScriptLogger();

            TokenSession = ScriptPipeline.RunTokenPipeline(tokenDefinition, LogHandler);

            TokenGraph = TokenSession.RuleGraph!;

            if (grammarDefinition != null)
            {
                GrammarSession = ScriptPipeline.RunGrammarPipeline(grammarDefinition, TokenSession);
                GrammarGraph = GrammarSession.RuleGraph;
            }

            return this;
        }

        public (ParseResult tokens, ParseResult astNodes) Parse(
            string input, 
            bool failOnWarning = false,
            bool throwExceptionsOnError = true)
        {
            Assertions.RequiresNotNull(TokenGraph!);
            Assertions.RequiresNotNull(TokenGraph!.Root!);

            return GrammarGraph == null
                    ? (TokenGraph.TokenizeText(input, failOnWarning, throwExceptionsOnError), ParseResult.Unknown)
                    : GrammarGraph.Parse(TokenGraph, input, failOnWarning, throwExceptionsOnError);
        }
    }
}
