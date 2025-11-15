// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.rules;
using gg.parse.script.common;
using gg.parse.script.parser;
using gg.parse.util;

using static gg.parse.util.Assertions;

namespace gg.parse.script.compiler
{
    /// <summary>
    /// Class implementing a compiler parsing tokenizers.
    /// 
    /// Covered by: <see cref="gg.parse.script.tests.compiler.TokenizerCompilerTests"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TokenizerCompiler : RuleCompilerBase<char>
    {
        public TokenizerCompiler()
        {
            RegisterDefaultFunctions();
        }

        public TokenizerCompiler(Dictionary<string, CompileFunc<RuleCompilationContext>> functions) 
            : base(functions)
        {
        } 

        public override ICompilerTemplate<RuleCompilationContext> RegisterDefaultFunctions()
        {
            // tokenizer specific rules
            Register(ScriptParser.Names.Any, CompileAny);
            Register(ScriptParser.Names.CharacterRange, CompileCharacterRange);
            Register(ScriptParser.Names.Literal, CompileLiteral);
            Register(ScriptParser.Names.CharacterSet, CompileCharacterSet);

            // shared rules
            Register(ScriptParser.Names.Break, CompileBreak);
            Register(ScriptParser.Names.Count, CompileRangedCount);
            Register(ScriptParser.Names.Evaluation, CompileEvaluation);
            Register(ScriptParser.Names.Find, CompileFind);
            Register(ScriptParser.Names.Group, CompileGroup);
            Register(ScriptParser.Names.If, CompileIf);
            Register(ScriptParser.Names.Log, CompileLog);
            Register(ScriptParser.Names.MatchOneOf, CompileOneOf);
            Register(ScriptParser.Names.Not, CompileNot);
            Register(ScriptParser.Names.OneOrMore, CompileOneOrMore);
            Register(ScriptParser.Names.Reference, CompileIdentifier);
            Register(ScriptParser.Names.Rule, CompileRule);
            Register(ScriptParser.Names.StopAfter, CompileStopAfter);
            Register(ScriptParser.Names.StopAt, CompileStopAt);
            Register(ScriptParser.Names.Sequence, CompileSequence);
            Register(ScriptParser.Names.ZeroOrMore, CompileZeroOrMore);
            Register(ScriptParser.Names.ZeroOrOne, CompileZeroOrOne);

            return this;
        }

        public static RuleBase<char> CompileLiteral(Type? _, Annotation annotation, RuleCompilationContext context)
        {
            var header = context.RuleHeader;

            RequiresNotNull(header);

            var literalText = context.GetText(annotation);

            var unescapedLiteralText = literalText[1..^1].SimpleUnescape();

            if (string.IsNullOrEmpty(unescapedLiteralText))
            {
                throw new CompilationException("Literal text is empty (somehow...).", annotation: annotation);
            }

            return new MatchDataSequence<char>(header.Name, unescapedLiteralText.ToCharArray(), header.Prune, header.Precedence);
        }

        public static RuleBase<char> CompileCharacterSet(Type? _, Annotation annotation, RuleCompilationContext context)
        {
            var header = context.RuleHeader;

            RequiresNotNull(header);

            Requires(annotation != null);
            Requires(annotation!.Children != null);
            Requires(annotation.Children!.Count == 1);

            var setText = context.GetText(annotation.Children[0]);

            if (string.IsNullOrEmpty(setText) || setText.Length <= 2)
            {
                throw new CompilationException("Text defining the MatchDataSet text is null or empty", annotation: annotation);
            }

            setText = setText[1..^1].SimpleUnescape();

            return new MatchDataSet<char>(header.Name, header.Prune, [.. setText], header.Precedence);
        }

        public static RuleBase<char> CompileCharacterRange(Type? _, Annotation annotation, RuleCompilationContext context)
        {
            var header = context.RuleHeader;

            RequiresNotNull(header);

            Requires(annotation != null);
            Requires(annotation!.Children != null);
            Requires(annotation == ScriptParser.Names.CharacterRange);

            var minText = context.GetText(annotation[0]!);

            if (minText.Length != 3)
            {
                throw new CompilationException($"CompileCharacterRange: invalid range definition {minText}.",
                            annotation: annotation);
            }

            var maxText = context.GetText(annotation[1]!);

            if (maxText.Length != 3)
            {
                throw new CompilationException($"CompileCharacterRange: invalid range definition {maxText}.",
                            annotation: annotation);
            }

            return new MatchDataRange<char>(
                header.Name,
                minText[1],
                maxText[1],
                header.Prune,
                header.Precedence
            );
        }
    }
}
