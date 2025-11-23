// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;

using gg.parse.core;
using gg.parse.script.common;
using gg.parse.script.parser;
using gg.parse.util;

namespace gg.parse.script.compiler
{
    public class CompileContext : IParseSession
    {
        private readonly LogCollection _logCollection;
        private TextPositionMap? _positionMap;
        
        public string Text { get; init; }

        public ImmutableList<Annotation> Tokens { get; init; }

        public ImmutableList<Annotation>? SyntaxTree { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public LogCollection Logs => _logCollection;

        public CompileContext(
            string text,
            ImmutableList<Annotation> tokens,
            LogCollection? collection = null
        )
        {
            Text = text;
            Tokens = tokens;
            SyntaxTree = null;
            _logCollection = collection ?? [];
        }

        public CompileContext(
            string text,
            ImmutableList<Annotation> tokens, 
            ImmutableList<Annotation> syntaxTree,
            LogCollection? collection = null
        )
        {
            Text = text;
            Tokens = tokens;
            SyntaxTree = syntaxTree;
            _logCollection = collection ?? [];
        }
        
        public string GetText(Annotation annotation) =>
            SyntaxTree == null
                ? Text.Substring(annotation.Start, annotation.Length)
                : Text.Substring(Tokens.CombinedRange(annotation.Range));


        public void Log(LogLevel level, string message, Annotation annotation, Exception? ex = null)
        {
            _positionMap = TextPositionMap.CreateOrUpdate(_positionMap, Text);
            var position = _positionMap.GetGrammarPosition(annotation, Tokens);

            _logCollection.Log(level, $"{position} {level}: {message}", position, ex);
        }

        public void ReportException(Exception ex, Annotation annotation)
        {
            Log(LogLevel.Error, ex.Message, annotation, ex);
        }

        public void ReportError(string message, Annotation annotation, Exception? ex = null)
        {
            Log(LogLevel.Error, message, annotation, ex);
        }

        public void ReportError(string message, Exception? ex = null)
        {
            _logCollection.Log(LogLevel.Error, $"{LogLevel.Error}: {message}", exception: ex);
        }
    }
}
