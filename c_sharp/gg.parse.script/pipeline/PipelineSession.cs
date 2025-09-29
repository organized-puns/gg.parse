using gg.parse.script.compiler;
using gg.parse.script.parser;

namespace gg.parse.script.pipeline
{
    public class PipelineSession<T> where T : IComparable<T>
    {
        // -- config -------------------------------------------------
        public string? WorkingFile { get; set; }

        public string? Text { get; set; }

        public string[] IncludePaths { get; set; } = [];

        public Dictionary<string, RuleGraph<T>?> IncludedFiles { get; set; } = [];

        // -- services -----------------------------------------------
        public ScriptTokenizer? Tokenizer { get; set; }

        public ScriptParser? Parser { get; set; }

        public PipelineLogger? LogHandler { get; set; }

        public RuleCompiler Compiler { get; set; }

        // -- output -------------------------------------------------
        
        public RuleGraph<T>? RuleGraph { get; set; }

        public List<Annotation>? Tokens { get; set; }

        public List<Annotation>? SyntaxTree { get; set; }
    }
}
