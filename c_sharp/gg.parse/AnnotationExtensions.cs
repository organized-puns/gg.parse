// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;

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

        public static string GetText(this Annotation annotation, string text) =>
            text.Substring(annotation.Start, annotation.Length);
       
        public static string GetText(this Annotation grammarAnnotation, string text, ImmutableList<Annotation> tokens)
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
        public static Range CombinedRange(this ImmutableList<Annotation> tokens, Range tokensRange)
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
        /// Finds all nodes matching the predicate and return them in a flat list
        /// </summary>
        /// <param name="annotationList"></param>
        /// <param name="predicate"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        //  note: can't name this "Where" because of name clashes with linq
        public static ImmutableList<Annotation> WhereDfs(this IEnumerable<Annotation> annotationList, Func<Annotation, bool> predicate, List<Annotation>? result = null)
        {
            result ??= [];

            foreach (var annotation in annotationList)
            {
                WhereDfs(annotation, predicate, result);
            }

            return [.. result];
        }

        /// <summary>
        /// Finds all nodes in (and including) the annotation and returns a flat list of the results
        /// </summary>
        /// <param name="annotation"></param>
        /// <param name="predicate"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        // note: can't name this "Where" because of name clashes with Linq
        public static ImmutableList<Annotation> WhereDfs(this Annotation annotation, Func<Annotation, bool> predicate, List<Annotation>? result = null)
        {
            result ??= [];

            if (predicate(annotation))
            {
                result.Add(annotation);
            }

            return annotation.Children != null
                ? annotation.Children.WhereDfs(predicate, result)
                : [..result];
        }

        /// <summary>
        /// Recursively goes through all annotations and their children that match the filter, pruning
        /// nodes that do not match the filter. 
        /// </summary>
        /// <param name="annotations"></param>
        /// <param name="predicate">Remove an annotation if it matches the given predicate</param>
        /// <returns></returns>
        public static ImmutableList<Annotation> Prune(this ImmutableList<Annotation> annotations, Func<Annotation, bool> predicate)
        {
            List<Annotation> result = [];

            annotations.ForEach(a =>
            {
                // invert the stated goals - keep everything that does not
                // match the predicate in order to keep the original the same 
                if (!predicate(a))
                {
                    result.Add(a.Prune(predicate));
                }
            });

            return [..result];
        }

        /// <summary>
        /// Prune any children that do not match the predicate
        /// </summary>
        /// <param name="annotation"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Annotation Prune(this Annotation annotation, Func<Annotation, bool> predicate) =>
        
            annotation.Children == null || annotation.Children.Count == 0
                ? annotation
                : new Annotation(
                    annotation.Rule,
                    annotation.Range,
                    [.. annotation.Children.Prune(predicate)],
                    annotation.Parent
                );
        

        /// <summary>
        /// Finds the first annotation in a DFS manner which matches the predicate.
        /// </summary>
        /// <param name="annotations"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Annotation? FirstOrDefaultDfs(this IEnumerable<Annotation> annotations, Func<Annotation, bool> predicate) =>
            annotations.FirstOrDefault(a => a.FirstOrDefaultDfs(predicate) != null);
        

        /// <summary>
        /// Find the first child, using DFS, which matches the predicate
        /// </summary>
        /// <param name="annotation"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Annotation? FirstOrDefaultDfs(this Annotation annotation, Func<Annotation, bool> predicate)
        {
            if (predicate(annotation))
            {
                return annotation;
            }

            return annotation.Children != null
                 ? annotation.Children.FirstOrDefaultDfs(predicate)
                 : default;
        }
    }
}
