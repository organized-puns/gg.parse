using gg.parse.script.compiler;
using gg.parse.script.parser;

using static gg.parse.script.compiler.CompilerFunctions;

namespace gg.parse.script
{
    public static class CompilerUtils
    {
        private static RuleCompiler<T> RegisterFunction<T>(this RuleCompiler<T> compiler, RuleBase<int> rule, CompileFunction<T> function) where T : IComparable<T> =>
            compiler.RegisterFunction(rule.Id, function, rule.Name);

        public static RuleCompiler<char> RegisterTokenizerCompilerFunctions(this RuleCompiler<char> compiler, ScriptParser parser)
        {
            return compiler
                    .RegisterFunction(parser.MatchAnyToken, CompileAny)
                    .RegisterFunction(parser.MatchCharacterRange, CompileCharacterRange)
                    .RegisterFunction(parser.MatchCharacterSet, CompileCharacterSet)
                    .RegisterFunction(parser.MatchLog, CompileLog)
                    .RegisterFunction(parser.MatchGroup, CompileGroup)
                    .RegisterFunction(parser.MatchIdentifier, CompileIdentifier)
                    .RegisterFunction(parser.MatchLiteral, CompileLiteral)
                    .RegisterFunction(parser.MatchNotOperator, CompileNot)
                    .RegisterFunction(parser.TryMatchOperator, CompileTryMatch)
                    .RegisterFunction(parser.MatchOneOrMoreOperator, CompileOneOrMore)
                    .RegisterFunction(parser.MatchOption, CompileOption)
                    .RegisterFunction(parser.MatchSequence, CompileSequence)
                    .RegisterFunction(parser.MatchZeroOrMoreOperator, CompileZeroOrMore)
                    .RegisterFunction(parser.MatchZeroOrOneOperator, CompileZeroOrOne);
        }

        public static RuleCompiler<int> RegisterGrammarCompilerFunctions(this RuleCompiler<int> compiler, ScriptParser parser)
        {
            return compiler
                    .RegisterFunction(parser.MatchAnyToken, CompileAny)
                    .RegisterFunction(parser.MatchGroup, CompileGroup)
                    .RegisterFunction(parser.MatchIdentifier, CompileIdentifier)
                    .RegisterFunction(parser.MatchNotOperator, CompileNot)
                    .RegisterFunction(parser.TryMatchOperator, CompileTryMatch)
                    .RegisterFunction(parser.MatchOneOrMoreOperator, CompileOneOrMore)
                    .RegisterFunction(parser.MatchOption, CompileOption)
                    .RegisterFunction(parser.MatchSequence, CompileSequence)
                    .RegisterFunction(parser.MatchZeroOrMoreOperator, CompileZeroOrMore)
                    .RegisterFunction(parser.MatchZeroOrOneOperator, CompileZeroOrOne)
                    .RegisterFunction(parser.MatchEval, CompileEvaluation)
                    .RegisterFunction(parser.MatchLog, CompileLog);
        }

        // xxx needs to move out of compiler utils
        public static (int functionId, AnnotationProduct product)[] CreateAnnotationProductMapping(this ScriptParser parser) =>
            [
                (parser.MatchTransitiveSelector.Id, AnnotationProduct.Transitive),
                (parser.MatchNoProductSelector.Id, AnnotationProduct.None),
            ];
    }
}
