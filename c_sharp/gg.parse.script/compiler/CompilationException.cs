namespace gg.parse.script.compiler
{
    public class CompilationException : Exception 
    {
        public Annotation? Annotation { get; init; }

        public IRule Rule { get; init; }

        public CompilationException(
            string message,
            Annotation? annotation = null,
            IRule rule = null)
            : base(message) 
        { 
            Annotation = annotation;    
            Rule = rule;
        }
    }
}
