using gg.parse;

namespace gg.parse.script.compiler
{
    public class CompileSession
    {
        public RuleCompiler Compiler { get; set; } 

        public string? Text { get; set; }

        public List<Annotation>? Tokens { get; set; } 

        public List<Annotation>? AstNodes { get; set; }      
       

        public CompileSession()
        {
            Compiler = new RuleCompiler();
        }

        public CompileSession(
            string text, 
            List<Annotation> tokens, 
            List<Annotation> astNodes,
            RuleCompiler? compiler = null)
           
        {
            Text = text;
            Tokens = tokens;
            AstNodes = astNodes;
            Compiler = compiler ?? new RuleCompiler();
        }

        public Range GetTextRange(Range tokenRange)
        {
            Assertions.RequiresNotNull(Tokens);

            var start = Tokens[tokenRange.Start].Start;
            var length = 0;

            for (var i = 0; i < tokenRange.Length; i++)
            {
                length += Tokens[tokenRange.Start + i].Length;
            }

            return new Range(start, length);
        }

        public string GetText(Range tokenRange)
        {
            var range = GetTextRange(tokenRange);
            
            return Text.Substring(range.Start, range.Length);
        }

        public CompileSession WithText(string text)
        {
            Text = text;
            return this;
        }        

        public CompileSession WithTokens(List<Annotation> tokens)
        {
            Tokens = tokens;
            return this;
        }

    }
}
