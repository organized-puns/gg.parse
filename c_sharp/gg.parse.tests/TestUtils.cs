using gg.parse.compiler;
using gg.parse.ebnf;

using static gg.parse.ebnf.CompilerUtils;

namespace gg.parse.tests
{
    public class TokenTestContext
    {
        public string Text { get; set; }

        public List<Annotation> Tokens { get; set; }

        public List<Annotation> AstNodes { get; set; }

        public RuleGraph<char>  TokenRules { get; set; }

        public RuleCompiler<char> TokenCompiler { get; set; }

        public EbnfTokenParser TokenizerParser { get; set; }

        public EbnfTokenizer Tokenizer { get; set; }

    }

    public static class TestUtils
    {
        /// <summary>
        /// Demonstrates how to set up an ebnf tokenizer, parser and compiler
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static (string text, List<Annotation> tokens, List<Annotation> astNodes, RuleGraph<char> table) SetupTokenizeParseCompile(string text)
        {
            var tokenizer = new EbnfTokenizer();
            var parser = new EbnfTokenParser(tokenizer);
            var compiler = new RuleCompiler<char>();

            var result = tokenizer.Tokenize(text);

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.Annotations != null && result.Annotations.Count > 0);

            var tokens = result.Annotations;

            result = parser.Parse(tokens);

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.Annotations != null && result.Annotations.Count > 0);
            Assert.IsTrue(result.Annotations[0].RuleId != parser.UnknownInputError.Id);

            var astNodes = result.Annotations;

            var table = compiler
                        .WithAnnotationProductMapping(parser.CreateAnnotationProductMapping())
                        .RegisterTokenizerCompilerFunctions(parser)
                        .Compile(new CompileSession<char>(text, tokens, astNodes));

            return (text, tokens, astNodes, table);
        }

        public static TokenTestContext SetupTokenizeTestContext(string text)
        {
            var tokenizer = new EbnfTokenizer();
            var parser = new EbnfTokenParser(tokenizer);
            var compiler = new RuleCompiler<char>();

            var result = tokenizer.Tokenize(text);

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.Annotations != null && result.Annotations.Count > 0);

            var tokens = result.Annotations;

            result = parser.Parse(tokens);

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.Annotations != null && result.Annotations.Count > 0);

            var astNodes = result.Annotations;

            var session = new CompileSession<char>(text, tokens, astNodes);

            var table = compiler
                        .WithAnnotationProductMapping(parser.CreateAnnotationProductMapping())
                        .RegisterTokenizerCompilerFunctions(parser)
                        .Compile(session);

            return new TokenTestContext()
            {
                AstNodes = astNodes,
                Text = text,
                TokenCompiler = compiler,
                Tokenizer = tokenizer,
                TokenRules = table,
                TokenizerParser = parser,
                Tokens = tokens
            };
        }
    }
}
