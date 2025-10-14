using System.Diagnostics.CodeAnalysis;

namespace gg.parse.util
{
    public readonly struct Range(int start, int length) 
    {
        public static readonly Range Undefined = new Range(0, -1);

        public int Start { get; init; } = start;

        public int End => Start + Length;

        public int Length { get; init; } = length;

        public override string ToString()
        {
            return $"Range(Start: {Start}, Length: {Length})";
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj != null
                && obj is Range r
                && r.Start == Start
                && r.Length == Length;
        }

        public static Range Union(IEnumerable<Range> ranges)
        {
            if (ranges == null || !ranges.Any())
            {
                throw new ArgumentException("Cannot create union of null or empty ranges.");
            }
            
            var minStart = ranges.Min(r => r.Start);
            var maxEnd = ranges.Max(r => r.End);
            return new Range(minStart, maxEnd - minStart);
        }
    }
}
