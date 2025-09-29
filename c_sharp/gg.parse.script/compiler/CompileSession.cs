
namespace gg.parse.script.compiler
{
    public class CompileSession
    {       
        public RuleCompiler Compiler { get; init; } 

        public string? Text { get; init; }

        public List<Annotation>? Tokens { get; init; } 

        public List<Annotation>? SyntaxTree { get; init; }      
       
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
            SyntaxTree = syntaxTree;
        }

        public (CompileFunction?, string?) FindFunction(IRule rule) =>
            Compiler.FindCompilationFunction(rule.Id);

        public (CompileFunction?, string?) FindFunction(int functionId) =>
            Compiler.FindCompilationFunction(functionId);


        public Range GetTextRange(Range tokenRange) =>
            Tokens!.CombinedRange(tokenRange); 

        public string GetText(Range tokenRange)
        {
            Assertions.RequiresNotNull(Text!);

            var range = GetTextRange(tokenRange);
            
            return Text!.Substring(range.Start, range.Length);
        }
    }
}
