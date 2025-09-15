using System;

namespace gg.parse
{
    public static class AnnotationExtensions
    {
        /// <summary>
        /// Utility function
        /// </summary>
        /// <param name="annotations"></param>
        /// <returns></returns>
        public static int[] SelectRuleIds(this IEnumerable<Annotation> annotations) =>
            annotations.Select(t => t.RuleId).ToArray();


        public static string GetText(this Annotation annotation, string text)
        {
            return text.Substring(annotation.Start, annotation.Length);
        }

        public static string GetText(this Annotation grammarAnnotation, string text, List<Annotation> tokens)
        {
            var range = UnionOfRanges(tokens, grammarAnnotation.Range);
            return text.Substring(range.Start, range.Length);
        }

        /// <summary>
        /// Returns union of the ranges of the tokens from/to the given tokensRange
        /// </summary>
        /// <param name="annotations"></param>
        /// <param name="tokensRange"></param>
        /// <returns></returns>
        public static Range UnionOfRanges(this List<Annotation> annotations, Range tokensRange)
        {
            var startIndex = annotations[tokensRange.Start].Start;
            var start = startIndex;
            var length = 0;

            for (var i = 0; i < tokensRange.Length; i++)
            {
                // need to take in account possible white space
                var token = annotations[tokensRange.Start + i];
                length += (token.Start - (startIndex + length)) + token.Length;
            }

            return new Range(start, length);
        }

        
    }
}
