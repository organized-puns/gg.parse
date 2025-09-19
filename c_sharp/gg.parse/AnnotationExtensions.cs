
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
        /// <param name="tokens"></param>
        /// <param name="tokensRange"></param>
        /// <returns></returns>
        public static Range UnionOfRanges(this List<Annotation> tokens, Range tokensRange)
        {
            // tokenRange is allowed to start above the max count as it signals a token
            // at the eof.
            if (tokensRange.Start < tokens.Count)
            {
                var startIndex = tokens[tokensRange.Start].Start;
                var start = startIndex;
                var length = 0;

                for (var i = 0; i < tokensRange.Length; i++)
                {
                    // need to take in account possible white space
                    var token = tokens[tokensRange.Start + i];
                    length += (token.Start - (startIndex + length)) + token.Length;
                }

                return new Range(start, length);
            }

            return new(tokens[^1].End, 0);
        }

        /// <summary>
        /// Recursively goes through all annotations and their children that match the filter 
        /// </summary>
        /// <param name="annotations"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static List<Annotation> Filter(this List<Annotation> annotations, Func<Annotation, bool> filter) =>
            [.. annotations
                .Where(a => filter(a))
                .Select( a => a.FilterChildren(filter))];

        public static Annotation FilterChildren(this Annotation annotation, Func<Annotation, bool> filter) =>
            
            annotation.Children == null
                ? annotation
                : new Annotation(
                    annotation.RuleId,
                    annotation.Range,
                    [..annotation
                        .Children
                        .Where(c => filter(c))
                        .Select( c => c.FilterChildren(filter))],
                    annotation.Parent
                );

        /// <summary>
        /// Map an annotation to a value and add it to the result if the value is not null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="annotation"></param>
        /// <param name="predicate"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static List<T> SelectNotNull<T>(this Annotation annotation, Func<Annotation, T> predicate, List<T>? result = null) 
            where T : class
        {
            result ??= [];

            var v = predicate(annotation);

            if (v != null)
            {
                result.Add(v);
            }

            if (annotation.Children != null)
            {
                foreach (var child in annotation.Children)
                {
                    child.SelectNotNull(predicate, result);
                }
            }

            return result;
        }
    }
}
