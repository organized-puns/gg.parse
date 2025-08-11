using gg.parse.compiler;
using gg.parse.rulefunctions;

using static gg.parse.compiler.CompilerFunctions;

namespace gg.parse.ebnf
{
    public static class CompilerUtils
    {
        public static RuleCompiler<char> RegisterTokenizerCompilerFunctions(this RuleCompiler<char> compiler, EbnfTokenParser parser)
        {
            return compiler
                    .RegisterFunction(parser.MatchAnyToken.Id, CompileAny)
                    .RegisterFunction(parser.MatchCharacterRange.Id, CompileCharacterRange)
                    .RegisterFunction(parser.MatchCharacterSet.Id, CompileCharacterSet)
                    .RegisterFunction(parser.MatchError.Id, CompileError)
                    .RegisterFunction(parser.MatchGroup.Id, CompileGroup)
                    .RegisterFunction(parser.MatchIdentifier.Id, CompileIdentifier)
                    .RegisterFunction(parser.MatchLiteral.Id, CompileLiteral)
                    .RegisterFunction(parser.MatchNotOperator.Id, CompileNot)
                    .RegisterFunction(parser.MatchOneOrMoreOperator.Id, CompileOneOrMore)
                    .RegisterFunction(parser.MatchOption.Id, CompileOption)
                    .RegisterFunction(parser.MatchSequence.Id, CompileSequence)
                    .RegisterFunction(parser.MatchZeroOrMoreOperator.Id, CompileZeroOrMore)
                    .RegisterFunction(parser.MatchZeroOrOneOperator.Id, CompileZeroOrOne);
        }

        public static RuleCompiler<int> RegisterGrammarCompilerFunctions(this RuleCompiler<int> compiler, EbnfTokenParser parser)
        {
            return compiler
                    .RegisterFunction(parser.MatchAnyToken.Id, CompileAny)
                    .RegisterFunction(parser.MatchError.Id, CompileError)
                    .RegisterFunction(parser.MatchGroup.Id, CompileGroup)
                    .RegisterFunction(parser.MatchIdentifier.Id, CompileIdentifier)
                    .RegisterFunction(parser.MatchNotOperator.Id, CompileNot)
                    .RegisterFunction(parser.MatchOneOrMoreOperator.Id, CompileOneOrMore)
                    .RegisterFunction(parser.MatchOption.Id, CompileOption)
                    .RegisterFunction(parser.MatchSequence.Id, CompileSequence)
                    .RegisterFunction(parser.MatchZeroOrMoreOperator.Id, CompileZeroOrMore)
                    .RegisterFunction(parser.MatchZeroOrOneOperator.Id, CompileZeroOrOne);
        }


        public static CompileContext<T> CreateContext<T>(string text, List<Annotation> tokens, List<Annotation> astNodes) where T: IComparable<T>
        {
            return new CompileContext<T>()
                    .WithText(text)
                    .WithTokens(tokens)
                    .WithAstNodes(astNodes);
        }

        /*public static CompileContext<char> RegisterTokenizerCompilerFunctions(this CompileContext<char> context, EbnfTokenParser parser)
        {
            return context
                    .WithFunction(parser.MatchAnyToken.Id, CompileAny)
                    .WithFunction(parser.MatchCharacterRange.Id, CompileCharacterRange)
                    .WithFunction(parser.MatchCharacterSet.Id, CompileCharacterSet)
                    .WithFunction(parser.MatchError.Id, CompileError)
                    .WithFunction(parser.MatchGroup.Id, CompileGroup)
                    .WithFunction(parser.MatchIdentifier.Id, CompileIdentifier)
                    .WithFunction(parser.MatchLiteral.Id, CompileLiteral)
                    .WithFunction(parser.MatchNotOperator.Id, CompileNot)
                    .WithFunction(parser.MatchOneOrMoreOperator.Id, CompileOneOrMore)
                    .WithFunction(parser.MatchOption.Id, CompileOption)
                    .WithFunction(parser.MatchSequence.Id, CompileSequence)
                    .WithFunction(parser.MatchZeroOrMoreOperator.Id, CompileZeroOrMore)
                    .WithFunction(parser.MatchZeroOrOneOperator.Id, CompileZeroOrOne);
        }*/

        /*
        public static CompileContext<int> RegisterGrammarCompilerFunctions(this CompileContext<int> context, EbnfTokenParser parser)
        {
            return context
                    .WithFunction(parser.MatchAnyToken.Id, CompileAny)
                    .WithFunction(parser.MatchError.Id, CompileError)
                    .WithFunction(parser.MatchGroup.Id, CompileGroup)
                    .WithFunction(parser.MatchIdentifier.Id, CompileIdentifier)
                    .WithFunction(parser.MatchNotOperator.Id, CompileNot)
                    .WithFunction(parser.MatchOneOrMoreOperator.Id, CompileOneOrMore)
                    .WithFunction(parser.MatchOption.Id, CompileOption)
                    .WithFunction(parser.MatchSequence.Id, CompileSequence)
                    .WithFunction(parser.MatchZeroOrMoreOperator.Id, CompileZeroOrMore)
                    .WithFunction(parser.MatchZeroOrOneOperator.Id, CompileZeroOrOne);
        }*/

        public static CompileContext<T> SetAnnotationProductMapping<T>(this CompileContext<T> context, EbnfTokenParser parser)
            where T : IComparable<T>
        {
            context.ProductLookup = [
                (parser.MatchTransitiveSelector.Id, AnnotationProduct.Transitive),
                (parser.MatchNoProductSelector.Id, AnnotationProduct.None),
            ];

            return context;
        }


        public static CompileContext<T> SetEngines<T>(this CompileContext<T> context, RuleTable<int> parser)
            where T : IComparable<T>
        {
            context.Parser = parser;
            return context;
        }

    }
}
