// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.script.compiler;
using gg.parse.script.parser;
using System.Collections.Immutable;

namespace gg.parse.script.pipeline
{
    public class PipelineSession<T> where T : IComparable<T>
    {
        // -- config -------------------------------------------------
        public string? WorkingFile { get; set; }

        public string? Text { get; set; }

        public HashSet<string> IncludePaths { get; set; } = [];

        public Dictionary<string, RuleGraph<T>?> IncludedFiles { get; set; } = [];

        // -- services -----------------------------------------------
        public ScriptTokenizer? Tokenizer { get; set; }

        public ScriptParser? Parser { get; set; }

        public ScriptLogger? LogHandler { get; set; }

        public RuleCompiler? Compiler { get; set; }

        // -- output -------------------------------------------------
        
        public RuleGraph<T>? RuleGraph { get; set; }

        public ImmutableList<Annotation>? Tokens { get; set; }

        public ImmutableList<Annotation>? SyntaxTree { get; set; }
    }
}
