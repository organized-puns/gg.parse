namespace gg.parse
{
    public readonly struct ParseResult(bool isSuccess, int charactersRead, List<Annotation>? annotations = null)
    {
        public static readonly ParseResult Failure = new(false, 0, null);

        public bool FoundMatch { get; init; } = isSuccess;

        public int MatchedLength { get; init; } = charactersRead;

        public List<Annotation>? Annotations { get; init; } = annotations;

        public Annotation? this [int index] => Annotations != null ? Annotations[index] : null;

        public void Deconstruct(out bool isSuccess, out int matchedLength, out List<Annotation>? annotations)
        {
            isSuccess = FoundMatch;
            matchedLength = MatchedLength;
            annotations = Annotations;
        }
    }
}
