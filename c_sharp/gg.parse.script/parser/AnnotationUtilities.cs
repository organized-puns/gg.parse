using gg.parse.rules;

namespace gg.parse.script.parser
{
    public static class AnnotationUtilities
    {
        private static Func<Annotation, bool>? _errorPredicate = null;
        private static Func<Annotation, bool>? _errorAndWarningPredicate = null;

        public static bool ContainsParseErrors(this List<Annotation> annotations, bool failOnWarning, out List<Annotation> errors) =>
            annotations.Contains(ErrorPredicate(failOnWarning), out errors);

        private static Func<Annotation, bool> ErrorPredicate(bool failOnWarning) =>
            failOnWarning
                ? _errorAndWarningPredicate ??= CreateErrorPredicate(failOnWarning)
                : _errorPredicate ??= CreateErrorPredicate(failOnWarning);

        private static Func<Annotation, bool> CreateErrorPredicate(bool failOnWarning)
        {
            var errorLevel = failOnWarning
                ? LogLevel.Warning | LogLevel.Error
                : LogLevel.Error;

            return new Func<Annotation, bool>(
                a => a.Rule is LogRule<int> logRule && (logRule.Level & errorLevel) > 0
            );
        }

        
    }
}
