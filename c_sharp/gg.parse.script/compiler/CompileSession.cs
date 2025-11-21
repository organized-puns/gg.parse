// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;

using gg.parse.core;
using gg.parse.script.common;
using gg.parse.util;

using Range = gg.parse.util.Range;

namespace gg.parse.script.compiler
{
    // xxx replace with compile context
    public class CompileSession : ISession
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

        public string GetText(Annotation annotation) =>
            Text!.Substring(Tokens!.CombinedRange(annotation.Range));
        
    }
}
