using gg.parse.compiler;
using gg.parse.rulefunctions;

using static gg.parse.compiler.CompilerFunctions;

namespace gg.parse.ebnf
{
    public static class CompilerUtils
    {
        private static RuleCompiler<T> RegisterFunction<T>(this RuleCompiler<T> compiler, RuleBase<int> rule, CompileFunction<T> function) where T : IComparable<T> =>
            compiler.RegisterFunction(rule.Id, function, rule.Name);

        public static RuleCompiler<char> RegisterTokenizerCompilerFunctions(this RuleCompiler<char> compiler, EbnfTokenParser parser)
        {
            return compiler
                    .RegisterFunction(parser.MatchAnyToken, CompileAny)
                    .RegisterFunction(parser.MatchCharacterRange, CompileCharacterRange)
                    .RegisterFunction(parser.MatchCharacterSet, CompileCharacterSet)
                    .RegisterFunction(parser.MatchError, CompileError)
                    .RegisterFunction(parser.MatchGroup, CompileGroup)
                    .RegisterFunction(parser.MatchIdentifier, CompileIdentifier)
                    .RegisterFunction(parser.MatchLiteral, CompileLiteral)
                    .RegisterFunction(parser.MatchNotOperator, CompileNot)
                    .RegisterFunction(parser.MatchOneOrMoreOperator, CompileOneOrMore)
                    .RegisterFunction(parser.MatchOption, CompileOption)
                    .RegisterFunction(parser.MatchSequence, CompileSequence)
                    .RegisterFunction(parser.MatchZeroOrMoreOperator, CompileZeroOrMore)
                    .RegisterFunction(parser.MatchZeroOrOneOperator, CompileZeroOrOne);
        }

        public static RuleCompiler<int> RegisterGrammarCompilerFunctions(this RuleCompiler<int> compiler, EbnfTokenParser parser)
        {
            return compiler
                    .RegisterFunction(parser.MatchAnyToken, CompileAny)
                    .RegisterFunction(parser.MatchError, CompileError)
                    .RegisterFunction(parser.MatchGroup, CompileGroup)
                    .RegisterFunction(parser.MatchIdentifier, CompileIdentifier)
                    .RegisterFunction(parser.MatchNotOperator, CompileNot)
                    .RegisterFunction(parser.MatchOneOrMoreOperator, CompileOneOrMore)
                    .RegisterFunction(parser.MatchOption, CompileOption)
                    .RegisterFunction(parser.MatchSequence, CompileSequence)
                    .RegisterFunction(parser.MatchZeroOrMoreOperator, CompileZeroOrMore)
                    .RegisterFunction(parser.MatchZeroOrOneOperator, CompileZeroOrOne);
        }


        public static CompileSession<T> CreateContext<T>(string text, List<Annotation> tokens, List<Annotation> astNodes) where T: IComparable<T>
        {
            return new CompileSession<T>()
                    .WithText(text)
                    .WithTokens(tokens)
                    .WithAstNodes(astNodes);
        }
        public static CompileSession<T> SetAnnotationProductMapping<T>(this CompileSession<T> context, EbnfTokenParser parser)
            where T : IComparable<T>
        {
            context.ProductLookup = [
                (parser.MatchTransitiveSelector.Id, AnnotationProduct.Transitive),
                (parser.MatchNoProductSelector.Id, AnnotationProduct.None),
            ];

            return context;
        }
    }
}
