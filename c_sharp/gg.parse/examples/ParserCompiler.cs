using gg.parse.rulefunctions;

namespace gg.parse.examples
{
    /// <summary>
    /// Generates a parser (RuleTable<int>) based on an EBNF spec
    /// </summary>
    public class ParserCompiler : RuleTable<int>
    {
        public EbnfTokenizer Tokenizer { get; init; }

        public ParserCompiler()
            : this(new EbnfTokenizer())
        {
        }
            
        public ParserCompiler(EbnfTokenizer tokenizer)
        {
            Tokenizer = tokenizer;
        }
    }
}
