// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.rules;
using System.Collections.Immutable;

namespace gg.parse.script.parser
{
    public static class AnnotationUtilities
    {
        public static bool ContainsErrors<T>(this ImmutableList<Annotation> annotations, bool failOnWarning, out ImmutableList<Annotation> errors)
            where T : IComparable<T>
        {
            var errorLevel = failOnWarning
                ? LogLevel.Warning | LogLevel.Error
                : LogLevel.Error;

            var predicate = new Func<Annotation, bool>(
                a => a.Rule is LogRule<T> logRule && (logRule.Level & errorLevel) > 0
            );

            errors = annotations.WhereDfs(predicate);
            return errors.Count > 0;
        }
    }
}
