// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;

using gg.parse.core;
using gg.parse.util;

namespace gg.parse.script.compiler
{
    public class CompileContext
    {
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
    }
}
