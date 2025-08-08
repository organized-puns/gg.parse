using gg.parse.compiler;
using gg.parse.ebnf;
using gg.parse.rulefunctions;

using static gg.parse.ebnf.CompilerUtils;

namespace gg.parse.tests
{
    public class TokenTestContext
    {
        public string Text { get; set; }

        public List<Annotation> Tokens { get; set; }

        public List<Annotation> AstNodes { get; set; }

        public RuleTable<char>  TokenRules { get; set; }

        public RuleCompiler<char> TokenCompiler { get; set; }

        public EbnfTokenizerParser TokenizerParser { get; set; }

        public EbnfTokenizer Tokenizer { get; set; }

    }

    public static class TestUtils
    {
        /// <summary>
        /// Demonstrates how to set up an ebnf tokenizer, parser and compiler
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static (string text, List<Annotation> tokens, List<Annotation> astNodes, RuleTable<char> table) SetupTokenizeParseCompile(string text)
        {
            var tokenizer = new EbnfTokenizer();
            var parser = new EbnfTokenizerParser(tokenizer);
            var compiler = new RuleCompiler<char>();

            var result = tokenizer.Tokenize(text);

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.Annotations != null && result.Annotations.Count > 0);

            var tokens = result.Annotations;

            result = parser.Parse(tokens);

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.Annotations != null && result.Annotations.Count > 0);

            var astNodes = result.Annotations;

            var context = CreateContext<char>(text, tokens, astNodes)
                            .RegisterTokenizerCompilerFunctions(parser)
                            .SetProductLookup(parser);

            var table = compiler.Compile(context);

            return (text, tokens, astNodes, table);
        }

        public static TokenTestContext SetupTokenizeTestContext(string text)
        {
            var tokenizer = new EbnfTokenizer();
            var parser = new EbnfTokenizerParser(tokenizer);
            var compiler = new RuleCompiler<char>();

            var result = tokenizer.Tokenize(text);

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.Annotations != null && result.Annotations.Count > 0);

            var tokens = result.Annotations;

            result = parser.Parse(tokens);

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.Annotations != null && result.Annotations.Count > 0);

            var astNodes = result.Annotations;

            var context = CreateContext<char>(text, tokens, astNodes)
                            .RegisterTokenizerCompilerFunctions(parser)
                            .SetProductLookup(parser);

            var table = compiler.Compile(context);

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
