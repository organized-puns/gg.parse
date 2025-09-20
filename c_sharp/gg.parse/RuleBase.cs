namespace gg.parse
{

    public enum AnnotationProduct
    {
        /// <summary>
        /// Returns an annotation for the matched item.
        /// </summary>
        Annotation,

        /// <summary>
        /// Returns the annotation produced by any child rules.
        /// </summary>
        Transitive,

        /// <summary>
        /// Does not produce an annotation (eg whitespace).
        /// </summary>
        None
    }


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

        public override string ToString() => Name;

        public ParseResult BuildDataRuleResult(Range dataRange) 
        {
#if DEBUG
            return Production switch
            {
                AnnotationProduct.Annotation =>
                    new ParseResult(true, dataRange.Length, [new Annotation(this, dataRange) { DebugName = Name }]),

                AnnotationProduct.Transitive =>
                    new ParseResult(true, dataRange.Length, [new Annotation(this, dataRange) { DebugName = Name }]),

                AnnotationProduct.None =>
                    new ParseResult(true, dataRange.Length),

                _ => throw new NotImplementedException($"Production rule {Production} is not implemented"),
            };
#else
            return Production switch
            {
                AnnotationProduct.Annotation => 
                    new ParseResult(true, dataRange.Length, [new Annotation(Id, dataRange)]),

                AnnotationProduct.Transitive => 
                    new ParseResult(true, dataRange.Length, [new Annotation(Id, dataRange)]),

                AnnotationProduct.None => 
                    new ParseResult(true, dataRange.Length),
  
                _ => throw new NotImplementedException($"Production rule {Production} is not implemented"),
            };
#endif
        }

        public ParseResult BuildFunctionRuleResult(Range dataRange, List<Annotation>? children = null)
        {
#if DEBUG
            return Production switch
            {
                AnnotationProduct.Annotation => new ParseResult(true, dataRange.Length,
                                        [new Annotation(this, dataRange, children) { DebugName = Name }]),

                AnnotationProduct.Transitive => new ParseResult(true, dataRange.Length, children),
                
                AnnotationProduct.None => new ParseResult(true, dataRange.Length),

                _ => throw new NotImplementedException($"Production rule {Production} is not implemented"),
            };
#else
            return Production switch
            {
                AnnotationProduct.Annotation => new ParseResult(true, dataRange.Length,
                                        [new Annotation(Id, dataRange, children)]),

                AnnotationProduct.Transitive => new ParseResult(true, dataRange.Length, children),

                AnnotationProduct.None => new ParseResult(true, dataRange.Length),

                _ => throw new NotImplementedException($"Production rule {Production} is not implemented"),
            };
#endif
        }
    }
}
