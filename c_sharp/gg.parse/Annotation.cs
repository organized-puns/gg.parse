namespace gg.parse
{

    public class Annotation(int functionId, Range range, List<Annotation>? children = null) : IComparable
    {
        public Range Range { get; init; } = range;
        
        public int Start => Range.Start;

        public int End => Range.Start + Range.Length;

        public int Length => Range.Length;

        /// <summary>
        /// Function which produced this annotation.
        /// </summary>
        public int FunctionId { get; init; } = functionId;

        public List<Annotation>? Children { get; } = children;

#if DEBUG
        public string DebugName { get; set; } = nameof(Annotation);
#endif

        public int CompareTo(object? obj)
        {
            if (obj is Annotation other)
            {
                return FunctionId.CompareTo(other.Range.Start);
            }

            return 0;
        }

        public override string ToString() =>
        
#if DEBUG
            $"{DebugName}({FunctionId}, {Range})";
#else
            $"Annotation(Id: {FunctionId}, Range: {Range})";
#endif
    }
}
