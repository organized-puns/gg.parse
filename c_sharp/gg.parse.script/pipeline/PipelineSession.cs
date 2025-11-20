// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;

using gg.parse.core;
using gg.parse.script.compiler;
using gg.parse.script.parser;

namespace gg.parse.script.pipeline
{
    // xxx merge with session
    public class PipelineSession<T> where T : IComparable<T>
    {
        // -- config -------------------------------------------------
        public string? WorkingFile { get; set; }

        public string? Text { get; set; }

        public HashSet<string> IncludePaths { get; set; } = [];

        public Dictionary<string, MutableRuleGraph<T>?> IncludedFiles { get; set; } = [];

        // -- services -----------------------------------------------
        public ScriptTokenizer? Tokenizer { get; set; }

        public ScriptParser? Parser { get; set; }

        public ScriptLogger? LogHandler { get; set; }

        // xxx left off here
        public ICompilerTemplate<RuleCompilationContext>? Compiler { get; set; }

        // -- output -------------------------------------------------
        
        public MutableRuleGraph<T>? RuleGraph { get; set; }

        public ImmutableList<Annotation>? Tokens { get; set; }

        public ImmutableList<Annotation>? SyntaxTree { get; set; }
    }
}
