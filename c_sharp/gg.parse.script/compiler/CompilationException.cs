namespace gg.parse.script.compiler
{
    public class CompilationException : Exception 
    {
        public Annotation? Annotation { get; init; }

        public int RuleId { get; init; }

        public CompilationException(
            string message,
            Annotation? annotation = null,
            int ruleId = -1)
            : base(message) 
        { 
            Annotation = annotation;    
            RuleId = ruleId;
        }
    }
}
