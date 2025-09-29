using System.Data;

namespace gg.parse.script.compiler
{
    public class CompileSession
    {
        
        public RuleCompiler Compiler { get; init; } 

        public string? Text { get; init; }

        public List<Annotation>? Tokens { get; init; } 

        public List<Annotation>? AstNodes { get; init; }      
       

        public CompileSession(
            RuleCompiler compiler,
            string text, 
            List<Annotation> tokens, 
            List<Annotation>? syntaxTree = null
        )
        {
            Compiler = compiler;
            Text = text;
            Tokens = tokens;
            AstNodes = syntaxTree;
            
        }

        public (CompileFunction?, string?) FindFunction(IRule rule) =>
            Compiler.FindCompilationFunction(rule.Id);

        public (CompileFunction?, string?) FindFunction(int functionId) =>
            Compiler.FindCompilationFunction(functionId);


        public Range GetTextRange(Range tokenRange)
        {
            Assertions.RequiresNotNull(Tokens!);

            var start = Tokens![tokenRange.Start].Start;
            var length = 0;

            for (var i = 0; i < tokenRange.Length; i++)
            {
                length += Tokens[tokenRange.Start + i].Length;
            }

            return new Range(start, length);
        }

        public string GetText(Range tokenRange)
        {
            Assertions.RequiresNotNull(Text!);

            var range = GetTextRange(tokenRange);
            
            return Text!.Substring(range.Start, range.Length);
        }
    }
}
