
namespace gg.parse
{
    public enum AnnotationDataCategory
    {
        Data,
        Error,
        Warning
    }

    public class AnnotationBase(AnnotationDataCategory category, int functionId, Range range)
    {
        public Range Range { get; init; } = range;
        
        public int Start => Range.Start;

        public int End => Range.Start + Range.Length;

        public int Length => Range.Length;

        public AnnotationDataCategory Category { get; init; } = category;

        /// <summary>
        /// Function which produced this annotation. .
        /// </summary>
        public int FunctionId { get; init; } = functionId;
        
        public override string ToString()
        {
            return $"Annotation(Type: {Category}, Id: {FunctionId}, Range: {Range})";
        }
    }
}
