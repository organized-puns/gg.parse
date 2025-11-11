// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.script.parser;
using gg.parse.util;
using System.Collections.Immutable;

namespace gg.parse.script.compiler
{
    public class CompileContext
    {
        private TextPositionMap? _positionMap;

        public string Text { get; init; }

        // xxx keep this out of the context
        // public ICompilerTemplate Compiler { get; init; }

        public ImmutableList<Annotation> Tokens { get; init; }

        public ImmutableList<Annotation>? SyntaxTree { get; init; }

        public List<Exception> Exceptions { get; init; }

        public CompileContext(
            string text,
            ImmutableList<Annotation> tokens
        )
        {
            Text = text;
            Tokens = tokens;
            SyntaxTree = null;
            Exceptions = [];
        }

        public CompileContext(
            string text,
            ImmutableList<Annotation> tokens, 
            ImmutableList<Annotation> syntaxTree
        )
        {
            Text = text;
            Tokens = tokens;
            SyntaxTree = syntaxTree;
            Exceptions = [];
        }

        public string GetText(Annotation annotation) =>
            Text.Substring(Tokens.CombinedRange(annotation.Range));

        public void ReportException<TException>(string message, Annotation annotation) where TException : Exception
        {
            _positionMap = TextPositionMap.CreateOrUpdate(_positionMap, Text);

            var (line, column) = _positionMap.GetGrammarPosition(annotation, Tokens);
            var exception = Activator.CreateInstance(typeof(TException), $"({line}, {column}) {message}");
                        
            Exceptions.Add((TException) exception!);
        }
    }
}
