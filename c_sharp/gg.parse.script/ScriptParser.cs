using gg.core.util;
using gg.parse.ebnf;

namespace gg.parse.script
{
    public class ScriptParser
    {
        public RuleGraph<char>? Tokenizer {get;set;}

        public RuleGraph<int>? Parser { get; set; }

        public PipelineLog? LogHandler { get; set; }

        public ScriptParser InitializeFromDefinition(string tokenDefinition, string? grammarDefinition = null, PipelineLog? logger = null)
        {
            LogHandler = logger ?? new PipelineLog();
            
            var tokenSession = ScriptPipelineX.RunTokenPipeline(tokenDefinition, LogHandler);

            Tokenizer = tokenSession.RuleGraph!;

            if (grammarDefinition != null)
            {
                var grammarSession = ScriptPipelineX.RunGrammarPipeline(grammarDefinition, tokenSession);
                Parser = grammarSession.RuleGraph;
            }

            return this;
        }

        public ParseResult Parse(string input)
        {
            Assertions.RequiresNotNull(Tokenizer!);
            Assertions.RequiresNotNull(Tokenizer!.Root!);

            var tokenizeResult = Tokenizer!.Root!.Parse(input);

            if (tokenizeResult.FoundMatch)
            {
                // xxx to do handle potential errors in the token annotations
                if (Parser != null)
                {
                    if (tokenizeResult.Annotations != null && tokenizeResult.Annotations.Count > 0)
                    {
                        // xxx to do handle potential errors in the annotations
                        return Parser!.Root!.Parse(tokenizeResult.Annotations);
                    }

                    // no tokens found, so return empty
                    return new ParseResult(true, 0, []);
                }

                return tokenizeResult;
                
            }

            return ParseResult.Failure;
        }
    }
}
