namespace gg.parse
{

    public abstract class RuleBase<T>(
        string name, 
        AnnotationProduct production = AnnotationProduct.Annotation, 
        int precedence = 0) 
        : IRule
    {
        public string Name { get; init; } = name;

        public int Id { get; set; } = -1;

        public int Precedence { get; init; } = precedence;

        public AnnotationProduct Production { get; init; } = production;

        public abstract ParseResult Parse(T[] input, int start);

        public override string ToString() => $"{Production} {Name}({Id})";

        public ParseResult BuildDataRuleResult(Range dataRange) 
        {
            return Production switch
            {
                AnnotationProduct.Annotation => 
                    new ParseResult(true, dataRange.Length, [new Annotation(this, dataRange)]),

                AnnotationProduct.Transitive => 
                    new ParseResult(true, dataRange.Length, [new Annotation(this, dataRange)]),

                AnnotationProduct.None => 
                    new ParseResult(true, dataRange.Length),
  
                _ => throw new NotImplementedException($"Production rule {Production} is not implemented"),
            };
        }

        public ParseResult BuildFunctionRuleResult(Range dataRange, List<Annotation>? children = null)
        {
            return Production switch
            {
                AnnotationProduct.Annotation => new ParseResult(true, dataRange.Length,
                                        [new Annotation(this, dataRange, children)]),

                AnnotationProduct.Transitive => new ParseResult(true, dataRange.Length, children),

                AnnotationProduct.None => new ParseResult(true, dataRange.Length),

                _ => throw new NotImplementedException($"Production rule {Production} is not implemented"),
            };
        }

        public RuleBase<T> CloneRule() => (RuleBase<T>)Clone();

        public object Clone() => MemberwiseClone();
    }
}
