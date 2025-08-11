namespace gg.parse
{
    public readonly struct ParseResult(bool isSuccess, int charactersRead, List<Annotation>? annotations = null)
    {
        public static readonly ParseResult Failure = new(false, 0, null);

        public bool FoundMatch { get; init; } = isSuccess;

        public int MatchedLength { get; init; } = charactersRead;

        public List<Annotation>? Annotations { get; init; } = annotations;

        public void Deconstruct(out bool isSuccess, out int matchedLength, out List<Annotation>? annotations)
        {
            isSuccess = FoundMatch;
            matchedLength = MatchedLength;
            annotations = Annotations;
        }
    }

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


    public abstract class RuleBase<T>(string name, AnnotationProduct production = AnnotationProduct.Annotation)
        where T : IComparable<T>
    {
        public string Name { get; init; } = name;

        public int Id { get; set; } = -1;

        public AnnotationProduct Production { get; init; } = production  ;

        public abstract ParseResult Parse(T[] input, int start);

        public override string ToString() => Name;

        public ParseResult BuildDataRuleResult(Range dataRange) 
        {
            return Production switch
            {
                AnnotationProduct.Annotation => new ParseResult(true, dataRange.Length,
                                        [new Annotation(Id, dataRange)]),
                AnnotationProduct.Transitive => new ParseResult(true, dataRange.Length,
                                        [new Annotation(Id, dataRange)]),
                // throw new NotImplementedException("Cannot apply transitive production to a rule which has no children"),
                AnnotationProduct.None => new ParseResult(true, dataRange.Length),
                _ => throw new NotImplementedException($"Production rule {Production} is not implemented"),
            };
        }

        public ParseResult BuildFunctionRuleResult(Range dataRange, List<Annotation>? children = null)
        {
            return Production switch
            {
                AnnotationProduct.Annotation => new ParseResult(true, dataRange.Length,
                                        [new Annotation(Id, dataRange, children)]),
                AnnotationProduct.Transitive => new ParseResult(true, dataRange.Length, children),
                AnnotationProduct.None => new ParseResult(true, dataRange.Length),
                _ => throw new NotImplementedException($"Production rule {Production} is not implemented"),
            };
        }
    }
}
