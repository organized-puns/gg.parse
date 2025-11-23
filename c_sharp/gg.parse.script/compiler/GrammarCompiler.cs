// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.script.parser;

namespace gg.parse.script.compiler
{
    /// <summary>
    /// Class implementing a compiler parsing grammars.
    /// 
    /// Covered by: <see cref="gg.parse.script.tests.compiler.TokenizerCompilerTests"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GrammarCompiler : RuleCompilerBase<int>
    {
        public GrammarCompiler()
        {
            RegisterDefaultFunctions();
        }

        public GrammarCompiler(Dictionary<string, CompileFunc<RuleCompilationContext>> functions)
            : base(functions)
        {
        }

        public override ICompilerTemplate<RuleCompilationContext> RegisterDefaultFunctions()
        {
            // shared rules
            Register(ScriptParser.Names.Any, CompileAny);
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
    }
}
