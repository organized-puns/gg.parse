// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.util;
using System.Collections.Immutable;
using Range = gg.parse.util.Range;

namespace gg.parse.script.compiler
{
    public class CompileSession
    {       
        public RuleCompiler Compiler { get; init; } 

        public string? Text { get; init; }

        public ImmutableList<Annotation>? Tokens { get; init; } 

        public ImmutableList<Annotation>? SyntaxTree { get; init; }      
       
        public List<Exception> Exceptions { get; init; } = [];

        public CompileSession(
            RuleCompiler compiler,
            string text,
            ImmutableList<Annotation> tokens,
            ImmutableList<Annotation>? syntaxTree = null
        )
        {
            Compiler = compiler;
            Text = text;
            Tokens = tokens;
            SyntaxTree = syntaxTree;
        }

        public (CompileFunction?, string?) FindFunction(IRule rule) =>
            Compiler.FindCompilationFunction(rule);

        public Range GetTextRange(Range tokenRange) =>
            Tokens!.CombinedRange(tokenRange);

        public string GetText(Range tokenRange) =>
            Text!.Substring(GetTextRange(tokenRange));
    }
}
