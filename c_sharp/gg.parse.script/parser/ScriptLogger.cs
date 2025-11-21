// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.rules;
using gg.parse.script.compiler;
using gg.parse.util;
using System.Collections.Immutable;
using static gg.parse.util.Assertions;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace gg.parse.script.parser
{
    public class ScriptLogger   
    {
        private TextPositionMap? _positionMap;

        public LogCollection? ReceivedLogs { get; set; }

        /// <summary>
        /// If set to true an exception will be thrown when a warning is encountered
        /// </summary>
        // xxx replace with loglevel
        public bool FailOnWarning { get; set; } = false;

        public Action<LogLevel, string>? Out { get; set; } = null;

        public ScriptLogger(
            bool storeLogs = true, 
            int maxStoredLogs = -1, 
            Action<LogLevel, string>? output = null,
            bool failOnWarning = false)
        {
            if (storeLogs)
            {
                ReceivedLogs = new(maxStoredLogs);
            }

            Out = output;

            FailOnWarning = failOnWarning;
        }

        public ScriptLogger(Action<LogLevel, string> outputAction, bool storeLogs = true, int maxStoredLogs = -1)
            : this(storeLogs, maxStoredLogs)
        {
            Out = outputAction;
        }

        public void Log(LogLevel level, string message)
        {
            ReceivedLogs?.Log(level, message);
            Out?.Invoke(level, message);
        }

        public void ProcessTokens(string text, IEnumerable<Annotation> tokens)
        {
            var logs = tokens
                .WhereDfs(node => node.Rule is LogRule<char> rule)
                .Select(node => new Tuple<Annotation, LogRule<char>>(node, (LogRule<char>)node.Rule));

            _positionMap = TextPositionMap.CreateOrUpdate(_positionMap, text);

            foreach (var (annotation, log) in logs)
            {
                var (line, column) = _positionMap.GetTokenPosition(annotation); 
                var message = $"({line}, {column}) {log.Text} near: \"{GetTokenText(annotation, text)}\".";

                Log(log.Level, message);
            }
        }

        public void ProcessSyntaxTree(string text, ImmutableList<Annotation> tokens, ImmutableList<Annotation> syntaxTree) 
        {
            var logs = syntaxTree
                        .WhereDfs(node => node.Rule is LogRule<int> rule)
                        .Select(node => new Tuple<Annotation, LogRule<int>>(node, (LogRule<int>)node.Rule));

            _positionMap = TextPositionMap.CreateOrUpdate(_positionMap, text);

            foreach (var (annotation, log) in logs) 
            {
                var (line, column) = _positionMap.GetGrammarPosition(annotation, tokens);
                var message = $"({line}, {column}) {log.Text} near \"{GetAnnotationText(annotation, text, tokens)}\".";

                Log(log.Level, message);
            }
        }


        public void ProcessException(Exception exception, string? text, ImmutableList<Annotation>? tokens)
        {
            if (exception is AggregateErrorException ae)
            {
                ProcessAggregateErrorException(ae);
            }
            else if (exception is CompilationException ce)
            {
                ProcessCompilationException(ce, text, tokens);
            }
            else if (exception is ScriptException se)
            {
                ProcessScriptException(se);
            }
            else
            {
                Log(LogLevel.Fatal, $"Exception: {exception}");
            }
        }

        public void ProcessAggregateErrorException(AggregateErrorException exception)
        {
            if (ReceivedLogs != null)
            {
                exception.Errors.ForEach( e => ReceivedLogs.Add(e));
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
            string? text,
            ImmutableList<Annotation> ?tokens)
        {
            exceptions.ForEach(e => ProcessException(e, text, tokens));
        }

        public void ProcessCompilationException(CompilationException exception, string? text, ImmutableList<Annotation>? tokens)
        {
            if (text != null && tokens != null && exception.Annotation != null)
            { 
                _positionMap = TextPositionMap.CreateOrUpdate(_positionMap, text);

                RequiresNotNull(_positionMap);

                var (line, column) = _positionMap.GetGrammarPosition(exception.Annotation, tokens);
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
                ? Math.Max(0, Math.Min(minStringLength, text.Length - token.Start))
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
    }
}
