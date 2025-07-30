
namespace gg.parse
{
    public enum AnnotationCategory
    {
        Token,
        AstNode,
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

        /// <summary>
        /// Entity which produced this annotation. Eg TokenFunction or ParseRule.
        /// </summary>
        public int ReferenceId { get; init; } = id;
        
        public override string ToString()
        {
            return $"Annotation(Type: {Category}, Id: {ReferenceId}, Range: {Range})";
        }
    }
}
