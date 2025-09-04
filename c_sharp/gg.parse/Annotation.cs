namespace gg.parse
{
    public class Annotation : IComparable
    {
        public Range Range { get; set; }
        
        public int Start => Range.Start;

        public int End => Range.Start + Range.Length;

        public int Length => Range.Length;

        /// <summary>
        /// Function which produced this annotation.
        /// </summary>
        public int FunctionId { get; init; }

        public List<Annotation>? Children { get; init; }

        public Annotation? Parent { get; set; }

        public Annotation? this[int index] => 
            Children == null 
            ? null 
            : Children![index];


#if DEBUG
        // Defaults to the annotation's name. Generally will be overwritten by a rule
        // with the rule name during parsing and constructing of the parse results.
        public string DebugName { get; set; } = nameof(Annotation);
#endif

        public Annotation(int functionId, Range range, List<Annotation>? children = null)
        {
            FunctionId = functionId;
            Range = range;
            Children = children;
        }

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
