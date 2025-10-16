// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.util;
using Range = gg.parse.util.Range;

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
            [.. annotations.Select(t => t.Rule.Id)];

        public static string GetText(this Annotation annotation, string text)
        {
            return text.Substring(annotation.Start, annotation.Length);
        }

        public static string GetText(this Annotation grammarAnnotation, string text, List<Annotation> tokens)
        {
            var range = CombinedRange(tokens, grammarAnnotation.Range);
            return text.Substring(range.Start, range.Length);
        }

        public static string GetText(this Annotation grammarAnnotation, string text, ParseResult tokens)
        {
            Assertions.RequiresNotNull(tokens.Annotations);

            var range = CombinedRange(tokens.Annotations, grammarAnnotation.Range);
            return text.Substring(range.Start, range.Length);
        }

        /// <summary>
        /// Returns union of the ranges of the tokens from/to the given tokensRange
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="tokensRange"></param>
        /// <returns></returns>
        public static Range CombinedRange(this List<Annotation> tokens, Range tokensRange)
        {
            // tokenRange is allowed to start above the max count as it signals a token
            // at the eof.
            if (tokensRange.Start < tokens.Count)
            {
                var startIndex = tokens[tokensRange.Start].Start;
                var start = startIndex;
                var length = 0;

                for (var i = 0; i < tokensRange.Length && i < tokens.Count; i++)
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
                    annotation.Rule,
                    annotation.Range,
                    [..annotation
                        .Children
                        .Where(c => filter(c))
                        .Select( c => c.FilterChildren(filter))],
                    annotation.Parent
                );

        public static void InvokeOnMatchingRuleNames(this List<Annotation> annotations, Dictionary<string, Action<Annotation>> actions) =>
            annotations.ForEach( a => a.InvokeOnMatchingRuleNames(actions));

        public static void InvokeOnMatchingRuleNames(this Annotation annotation, Dictionary<string, Action<Annotation>> actions)
        {
            if (actions.TryGetValue(annotation.Rule.Name, out var annotationAction))
            {
                annotationAction(annotation);
            }

            if (annotation.Children != null)
            {
                InvokeOnMatchingRuleNames(annotation.Children, actions);
            }
        }

        /// <summary>
        /// Finds the first annotation in a DFS manner which matches the predicate.
        /// </summary>
        /// <param name="annotations"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Annotation? FirstOrDefault(this IEnumerable<Annotation> annotations, Func<Annotation, bool> predicate)
        {
            foreach (var annotation in annotations)
            {
                var result = annotation.FirstOrDefault(predicate);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public static Annotation? FirstOrDefault(this Annotation annotation, Func<Annotation, bool> predicate)
        {
            if (predicate(annotation))
            {
                return annotation;
            }

            if (annotation.Children != null)
            {
                var result = FirstOrDefault(annotation.Children, predicate);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public static Annotation? FindByRuleName(this Annotation annotation, string ruleName) =>
            annotation.FirstOrDefault(a => a.Rule != null && a.Rule.Name == ruleName);
        

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

        public static bool Contains(this List<Annotation> annotations, Func<Annotation, bool> predicate, out List<Annotation> results)
        {
            results = [];

            foreach (var annotation in annotations)
            {
                annotation.Collect(predicate, results);
            }

            return results.Count > 0;
        }

        public static string Substring(this string str, Annotation annotation) => str.Substring(annotation.Range);

        public static string Substring(this string str, Range range) => str.Substring(range.Start, range.Length);

    }
}
