using gg.parse.compiler;

namespace gg.parse.ebnf
{
    public class PipelineSessionX<T> where T : IComparable<T>
    {
        // -- config -------------------------------------------------
        public string? WorkingFile { get; set; }

        public string? Text { get; set; }

        public string[] IncludePaths { get; set; } = [];

        public Dictionary<string, RuleGraph<T>?> IncludedFiles { get; set; } = [];

        // -- services -----------------------------------------------
        public EbnfTokenizer? Tokenizer { get; set; }

        public EbnfTokenParser? Parser { get; set; }

        public PipelineLog? LogHandler { get; set; }

        public RuleCompiler<T>? Compiler { get; set; }

        // -- output -------------------------------------------------
        
        public RuleGraph<T>? RuleGraph { get; set; }

        public List<Annotation>? Tokens { get; set; }

        public List<Annotation>? AstNodes { get; set; }
    }
}
