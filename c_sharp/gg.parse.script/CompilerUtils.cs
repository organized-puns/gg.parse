using gg.parse.script.compiler;
using gg.parse.script.parser;

using static gg.parse.script.compiler.CompilerFunctions;

namespace gg.parse.script
{
    // xxx move to pipeline
    public static class CompilerUtils
    {
        private static RuleCompiler RegisterFunction(this RuleCompiler compiler, RuleBase<int> rule, CompileFunction function) =>
            compiler.RegisterFunction(rule.Id, function, rule.Name);

        public static RuleCompiler RegisterTokenizerCompilerFunctions(this RuleCompiler compiler, ScriptParser parser)
        {
            return compiler
                    .RegisterFunction(parser.MatchAnyToken, CompileAny<char>)
                    .RegisterFunction(parser.MatchCharacterRange, CompileCharacterRange)
                    .RegisterFunction(parser.MatchCharacterSet, CompileCharacterSet)
                    .RegisterFunction(parser.MatchLog, CompileLog<char>)
                    .RegisterFunction(parser.MatchGroup, CompileGroup<char>)
                    .RegisterFunction(parser.MatchIdentifier, CompileIdentifier<char>)
                    .RegisterFunction(parser.MatchLiteral, CompileLiteral)
                    .RegisterFunction(parser.MatchNotOperator, CompileNot<char>)
                    .RegisterFunction(parser.IfMatchOperator, CompileTryMatch<char>)
                    .RegisterFunction(parser.MatchOneOrMoreOperator, CompileOneOrMore<char>)
                    .RegisterFunction(parser.MatchOption, CompileOption<char>)
                    .RegisterFunction(parser.MatchSequence, CompileSequence<char>)
                    .RegisterFunction(parser.MatchZeroOrMoreOperator, CompileZeroOrMore<char>)
                    .RegisterFunction(parser.MatchZeroOrOneOperator, CompileZeroOrOne<char>)
                    .RegisterFunction(parser.MatchFindOperator, CompileFind<char>)
                    .RegisterFunction(parser.MatchSkipOperator, CompileSkip<char>);
        }

        public static RuleCompiler RegisterGrammarCompilerFunctions(this RuleCompiler compiler, ScriptParser parser)
        {
            return compiler
                    .RegisterFunction(parser.MatchAnyToken, CompileAny<int>)
                    .RegisterFunction(parser.MatchGroup, CompileGroup<int>)
                    .RegisterFunction(parser.MatchIdentifier, CompileIdentifier<int>)
                    .RegisterFunction(parser.MatchNotOperator, CompileNot<int>)
                    .RegisterFunction(parser.IfMatchOperator, CompileTryMatch<int>)
                    .RegisterFunction(parser.MatchOneOrMoreOperator, CompileOneOrMore<int>)
                    .RegisterFunction(parser.MatchOption, CompileOption<int>)
                    .RegisterFunction(parser.MatchSequence, CompileSequence<int>)
                    .RegisterFunction(parser.MatchZeroOrMoreOperator, CompileZeroOrMore<int>)
                    .RegisterFunction(parser.MatchZeroOrOneOperator, CompileZeroOrOne<int>)
                    .RegisterFunction(parser.MatchEval, CompileEvaluation<int>)
                    .RegisterFunction(parser.MatchLog, CompileLog<int>)
                    .RegisterFunction(parser.MatchFindOperator, CompileFind<int>)
                    .RegisterFunction(parser.MatchSkipOperator, CompileSkip<int>);
        }

        // xxx needs to move out of compiler utils
        public static (int functionId, IRule.Output product)[] CreateAnnotationProductMapping(this ScriptParser parser) =>
            [
                (parser.MatchTransitiveSelector.Id, IRule.Output.Children),
                (parser.MatchNoProductSelector.Id, IRule.Output.Void),
            ];
    }
}
