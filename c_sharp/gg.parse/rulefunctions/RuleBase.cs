namespace gg.parse.rulefunctions
{
    public readonly struct ParseResult(bool isSuccess, int charactersRead, List<gg.parse.basefunctions.Annotation>? annotations = null)
    {
        public static readonly ParseResult Failure = new(false, 0, null);

        public bool IsSuccess { get; init; } = isSuccess;

        public int MatchedLength { get; init; } = charactersRead;

        public List<gg.parse.basefunctions.Annotation>? Annotations { get; init; } = annotations;
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
    }
}
