using gg.parse.script.common;
using gg.parse.script.pipeline;

namespace gg.parse.script
{
    public class RuleGraphBuilder
    {
        public RuleGraph<char>? Tokenizer {get;set;}

        public RuleGraph<int>? Parser { get; set; }

        public PipelineLog? LogHandler { get; set; }

        public PipelineSession<char>? TokenSession { get; private set; }

        public PipelineSession<int>? GrammarSession { get; private set; }

        public RuleGraphBuilder InitializeFromDefinition(string tokenDefinition, string? grammarDefinition = null, PipelineLog? logger = null)
        {
            LogHandler = logger ?? new PipelineLog();

            TokenSession = ScriptPipeline.RunTokenPipeline(tokenDefinition, LogHandler);

            Tokenizer = TokenSession.RuleGraph!;

            if (grammarDefinition != null)
            {
                GrammarSession = ScriptPipeline.RunGrammarPipeline(grammarDefinition, TokenSession);
                Parser = GrammarSession.RuleGraph;
            }

            return this;
        }

        public (ParseResult tokens, ParseResult astNodes) Parse(string input)
        {
            Assertions.RequiresNotNull(Tokenizer!);
            Assertions.RequiresNotNull(Tokenizer!.Root!);

            return Parser == null
                    ? (Tokenizer.TokenizeText(input), ParseResult.Unknown)
                    : Parser.Parse(Tokenizer, input);
        }
    }
}
