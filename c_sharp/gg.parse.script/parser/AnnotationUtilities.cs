// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.rules;

namespace gg.parse.script.parser
{
    public static class AnnotationUtilities
    {
        public static bool ContainsErrors<T>(this List<Annotation> annotations, bool failOnWarning, out List<Annotation> errors)
            where T : IComparable<T> =>
            annotations.Contains(ErrorPredicate<T>(failOnWarning), out errors);

        private static Func<Annotation, bool> ErrorPredicate<T>(bool failOnWarning) where T : IComparable<T> =>
            failOnWarning
                ? CreateErrorPredicate<T>(failOnWarning)
                : CreateErrorPredicate<T>(failOnWarning);

        private static Func<Annotation, bool> CreateErrorPredicate<T>(bool failOnWarning)
            where T : IComparable<T>
        {
            var errorLevel = failOnWarning
                ? LogLevel.Warning | LogLevel.Error
                : LogLevel.Error;

            return new Func<Annotation, bool>(
                a => a.Rule is LogRule<T> logRule && (logRule.Level & errorLevel) > 0
            );
        }        
    }
}
