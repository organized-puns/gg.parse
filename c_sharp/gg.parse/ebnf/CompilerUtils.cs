using gg.parse.compiler;
using gg.parse.rulefunctions;

using static gg.parse.compiler.CompilerFunctions;

namespace gg.parse.ebnf
{
    public static class CompilerUtils
    {
        public static CompileContext<T> CreateContext<T>(string text, List<Annotation> tokens, List<Annotation> astNodes) where T: IComparable<T>
        {
            return new CompileContext<T>()
                    .WithText(text)
                    .WithTokens(tokens)
                    .WithAstNodes(astNodes);
        }

        public static CompileContext<char> RegisterTokenizerCompilerFunctions(this CompileContext<char> context, EbnfTokenizerParser parser)
        {
            return context
                    .WithFunction(parser.MatchAnyCharacter.Id, CompileAny)
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
        }

        public static CompileContext<int> RegisterGrammarCompilerFunctions(this CompileContext<int> context, EbnfGrammarParser parser)
        {
            return context
                    .WithFunction(parser.MatchAnyToken.Id, CompileAny)
                    .WithFunction(parser.MatchError.Id, CompileError)
                    .WithFunction(parser.MatchGroup.Id, CompileGroup)
                    //.WithFunction(parser.MatchLiteral.Id, CompileLiteral)
                    .WithFunction(parser.MatchIdentifier.Id, CompileIdentifier)
                    .WithFunction(parser.MatchNotOperator.Id, CompileNot)
                    .WithFunction(parser.MatchOneOrMoreOperator.Id, CompileOneOrMore)
                    .WithFunction(parser.MatchOption.Id, CompileOption)
                    .WithFunction(parser.MatchSequence.Id, CompileSequence)
                    .WithFunction(parser.MatchZeroOrMoreOperator.Id, CompileZeroOrMore)
                    .WithFunction(parser.MatchZeroOrOneOperator.Id, CompileZeroOrOne);
        }

        public static CompileContext<T> SetProductLookup<T>(this CompileContext<T> context, EbnfTokenizerParser parser)
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
