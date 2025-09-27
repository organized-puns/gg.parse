namespace gg.parse
{
    public readonly struct ParseResult(bool isSuccess, int dataRead, List<Annotation>? annotations = null)
    {
        public static readonly ParseResult Success = new(true, 0, null);
        public static readonly ParseResult Unknown = new(true, -1, null);
        public static readonly ParseResult Failure = new(false, 0, null);

        public bool FoundMatch { get; init; } = isSuccess;

        public int MatchedLength { get; init; } = dataRead;

        public List<Annotation>? Annotations { get; init; } = annotations;

        public Annotation? this [int index] => Annotations != null ? Annotations[index] : null;

        public void Deconstruct(out bool isSuccess, out int matchedLength, out List<Annotation>? annotations)
        {
            isSuccess = FoundMatch;
            matchedLength = MatchedLength;
            annotations = Annotations;
        }

        public int[] CollectRuleIds() => Annotations == null ? [] : [.. Annotations.Select(a => a.Rule.Id)];

        public override string ToString()
        {
            if (FoundMatch)
            {
                return $"match, length={MatchedLength}, count={(Annotations == null ? 0 : Annotations.Count)}";
            }
            else
            {
                return $"no match, length={MatchedLength}";
            }
        }
    }
}
