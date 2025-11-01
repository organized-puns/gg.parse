// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.rules;
using gg.parse.script.compiler;
using gg.parse.script.parser;
using System.Collections.Immutable;
using static gg.parse.util.Assertions;

using Range = gg.parse.util.Range;

namespace gg.parse.script.pipeline
{
    public class ScriptLogger   
    {
        // Note: not thread safe, application will need to deal with this
        public List<(LogLevel level, string message)>? ReceivedLogs { get; set; }

        public int MaxStoredLogs { get; set; } = -1;

        /// <summary>
        /// If set to true an exception will be thrown when a warning is encountered
        /// </summary>
        public bool FailOnWarning { get; set; } = false;

        public Action<LogLevel, string>? Out { get; set; } = null;

        public ScriptLogger(bool storeLogs = true, int maxStoredLogs = -1, Action<LogLevel, string>? output = null)
        {
            if (storeLogs)
            {
                ReceivedLogs = [];
            }

            MaxStoredLogs = maxStoredLogs;

            Out = output;
        }

        public ScriptLogger(Action<LogLevel, string> outputAction, bool storeLogs = true, int maxStoredLogs = -1)
            : this(storeLogs, maxStoredLogs)
        {
            Out = outputAction;
        }

        public void Log(LogLevel level, string message)
        {
            if (ReceivedLogs != null)
            {
                ReceivedLogs.Add((level, message));

                if (MaxStoredLogs > 0 && ReceivedLogs.Count > MaxStoredLogs)
                {
                    ReceivedLogs.RemoveRange(0, ReceivedLogs.Count - MaxStoredLogs);
                }
            }

            Out?.Invoke(level, message);
        }

        public void ProcessTokens(string text, IEnumerable<Annotation> tokens)
        {
            var logs = tokens
                .WhereDfs(node => node.Rule is LogRule<char> rule)
                .Select(node => new Tuple<Annotation, LogRule<char>>(node, (LogRule<char>)node.Rule));
            
            var lineRanges = CollectLineRanges(text);

            foreach (var (annotation, log) in logs)
            {
                var (line, column) = MapRangeToLineColumn(annotation.Range, lineRanges);
                var message = $"({line}, {column}) {log.Text} near: \"{GetTokenText(annotation, text)}\".";

                Log(log.Level, message);
            }
        }

        public void ProcessSyntaxTree(string text, ImmutableList<Annotation> tokens, ImmutableList<Annotation> syntaxTree) 
        {
            var logs = syntaxTree
                        .WhereDfs(node => node.Rule is LogRule<int> rule)
                        .Select(node => new Tuple<Annotation, LogRule<int>>(node, (LogRule<int>)node.Rule));

            var lineRanges = CollectLineRanges(text);

            foreach (var (annotation, log) in logs) 
            {
                var (line, column) = MapAnnotationRangeToLineColumn(annotation, tokens, lineRanges);
                var message = $"({line}, {column}) {log.Text} near \"{GetAnnotationText(annotation, text, tokens)}\".";

                Log(log.Level, message);
            }
        }

        public void ProcessException(Exception exception, bool logException = true)
        {
            if (logException)
            {
                Log(LogLevel.Fatal, $"Exception: {exception}");
            }

            if (exception is ScriptException se)
            {
                ProcessScriptException(se);
            }
        }

        public void ProcessScriptException(ScriptException exception)
        {
            
            if (exception.Errors != null && exception.Text != null)
            {
                if (exception.Tokens == null)
                {
                    ProcessTokens(exception.Text, exception.Errors);
                }
                else
                {
                    ProcessSyntaxTree(exception.Text, exception.Tokens, exception.Errors);
                }
            }
            else
            {
                Log(LogLevel.Fatal, "No further details on the underlying error.");
            }
        }

        public void ProcessExceptions(ScriptException exception)
        {
            Log(LogLevel.Fatal, $"Exception: {exception}");

            if (exception.Errors != null && exception.Text != null && exception.Tokens != null)
            {
                ProcessSyntaxTree(exception.Text, exception.Tokens, exception.Errors);
            }
            else
            {
                Log(LogLevel.Fatal, "No further details on the underlying error.");
            }
        }

        public void ProcessExceptions(
            IEnumerable<Exception> exceptions, 
            string text,
            ImmutableList<Annotation> tokens)
        {
            var lineRanges = CollectLineRanges(text);

            foreach (var ex in exceptions)
            {
                if (ex is ScriptException scriptEx)
                {
                    ProcessException(scriptEx, logException: false);
                }
                else if (ex is CompilationException ce)
                {
                    ProcessException(ce, tokens, lineRanges);
                }
                else
                {
                    Log(LogLevel.Error, $"Exception: {ex}");
                }
            }
        }

        public void ProcessException(CompilationException exception, ImmutableList<Annotation> tokens, List<Range> lineRanges)
        {
            if (exception.Annotation != null)
            {
                var (line, column) = MapAnnotationRangeToLineColumn(exception.Annotation, tokens, lineRanges);
                Log(LogLevel.Error, $"({line}, {column}) Compilation error: {exception.Message}");
            }          
            else
            {
                Log(LogLevel.Error, $"Compilation error: {exception.Message}");
            }
        }

        private static string GetAnnotationText(
            Annotation annotation, 
            string text,
            ImmutableList<Annotation> tokens, 
            int minStringLength = 8, 
            int maxStringLength = 60
        )
        {
            Requires(minStringLength < maxStringLength);
            Requires(maxStringLength > 4);

            var annotationText = annotation.GetText(text, tokens);

            if (annotationText.Length < minStringLength)
            {
                // check if there is a parent
                if (annotation.Parent != null)
                {
                    return GetAnnotationText(annotation.Parent, text, tokens, minStringLength, maxStringLength);
                }

                return annotationText;
            }

            if (annotation.Length >= maxStringLength)
            {
                return string.Concat(annotationText.AsSpan(maxStringLength - 4), " ...");
            }

            return annotationText;
        }

        private static string GetTokenText(
            Annotation token,
            string text,
            int minStringLength = 8,
            int maxStringLength = 60
        )
        {
            Requires(minStringLength < maxStringLength);
            Requires(maxStringLength > 4);

            var textLength = token.Length == 0
                ? Math.Max(0, Math.Min(minStringLength, (text.Length - token.Start)))
                : token.Length;

            var annotationText = text.Substring(token.Start, textLength);

            if (annotationText.Length < minStringLength)
            {
                // check if there is a parent
                if (token.Parent != null)
                {
                    return GetTokenText(token.Parent, text, minStringLength, maxStringLength);
                }

                return annotationText;
            }

            if (annotationText.Length >= maxStringLength)
            {
                return string.Concat(annotationText.AsSpan(maxStringLength - 4), " ...");
            }

            return annotationText;
        }

        private static List<Range> CollectLineRanges(string text)
        {
            var result = new List<Range>();
            var start = 0;

            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    result.Add(new Range(start, i - start));
                    start = i + 1;
                }
            }

            result.Add(new(start, text.Length - start));

            return result;
        }

        public static (int line, int column) MapRangeToLineColumn(Range textRange, List<Range> lineRanges)
        {
            int line;

            for (line = 0; line < lineRanges.Count; line++)
            {
                if (textRange.Start >= lineRanges[line].Start && textRange.Start <= lineRanges[line].End)
                {
                    break;
                }
            }

            return (line + 1, textRange.Start - lineRanges[line].Start + 1);
        }

        private static (int line, int column) MapAnnotationRangeToLineColumn(
            Annotation annotation,
            ImmutableList<Annotation> tokens, 
            List<Range> lineRanges) =>

           MapRangeToLineColumn(tokens.CombinedRange(annotation.Range), lineRanges);         
    }
}
