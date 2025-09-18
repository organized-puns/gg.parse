using gg.parse.ebnf;

namespace gg.parse.script
{
    public class ScriptParser
    {
        public RuleGraph<char> Tokenizer {get;set;}

        public RuleGraph<int>? Parser { get; set; }

        public ScriptParser CreateFromDefinition(string tokenDefinition, string grammarDefinition, PipelineLog? logger = null)
        {
            var tokenSession = ScriptPipeline.RunTokenPipeline(tokenDefinition, logger);
            var grammarSession = ScriptPipeline.RunGrammarPipeline(grammarDefinition, tokenSession);

            Tokenizer = tokenSession.RuleGraph!;
            Parser = grammarSession.RuleGraph;

            return this;
        }

        public ParseResult Parse(string input)
        {
            var tokenizeResult = Tokenizer.Root!.Parse(input);

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
