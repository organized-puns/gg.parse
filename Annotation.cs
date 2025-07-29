
namespace gg.parse
{
    public enum AnnotationCategory
    {
        /// <summary>
        /// Annotates a token at the specified range.
        /// </summary>
        Token,

        Error,

        Warning
    }

    public class Annotation(AnnotationCategory type, int id, Range range)
    {
        public Range Range { get; init; } = range;
        
        public int Start => Range.Start;

        public int End => Range.Start + Range.Length;

        public int Length => Range.Length;

        public AnnotationCategory Category { get; init; } = type;
        
        public int FunctionId { get; init; } = id;
        
        public override string ToString()
        {
            return $"Annotation(Type: {Category}, Id: {FunctionId}, Range: {Range})";
        }
    }
}
