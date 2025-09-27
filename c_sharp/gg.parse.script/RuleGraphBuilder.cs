using gg.parse.script.common;
using gg.parse.script.parser;
using gg.parse.script.pipeline;

namespace gg.parse.script
{
    public class RuleGraphBuilder
    {
        public RuleGraph<char>? Tokenizer {get;set;}

        public RuleGraph<int>? Parser { get; set; }

        public ScriptLogger? LogHandler { get; set; }

        public PipelineSession<char>? TokenSession { get; private set; }

        public PipelineSession<int>? GrammarSession { get; private set; }

        public RuleGraphBuilder InitializeFromDefinition(string tokenDefinition, string? grammarDefinition = null, ScriptLogger? logger = null)
        {
            LogHandler = logger ?? new ScriptLogger();

            TokenSession = ScriptPipeline.RunTokenPipeline(tokenDefinition, LogHandler);

            Tokenizer = TokenSession.RuleGraph!;

            if (grammarDefinition != null)
            {
                GrammarSession = ScriptPipeline.RunGrammarPipeline(grammarDefinition, TokenSession);
                Parser = GrammarSession.RuleGraph;
            }

            return this;
        }

        public (ParseResult tokens, ParseResult astNodes) Parse(
            string input, 
            bool failOnWarning = false,
            bool throwExceptionsOnError = true)
        {
            Assertions.RequiresNotNull(Tokenizer!);
            Assertions.RequiresNotNull(Tokenizer!.Root!);

            return Parser == null
                    ? (Tokenizer.TokenizeText(input, failOnWarning, throwExceptionsOnError), ParseResult.Unknown)
                    : Parser.Parse(Tokenizer, input, failOnWarning, throwExceptionsOnError);
        }
    }
}
