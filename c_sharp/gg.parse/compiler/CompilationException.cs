namespace gg.parse.compiler
{
    public class CompilationException<T> : Exception where T : IComparable<T>
    {
        public Range Range { get; init; }

        public RuleBase<T>? Rule { get; init; }

        public Annotation? Annotation { get; init; }

        public CompilationException(string message, Range range, RuleBase<T>? rule = null, Annotation? annotation = null)
            : base(message) 
        { 
            Range = range;
            Rule = rule;
            Annotation = annotation;    
        }
    }
}
