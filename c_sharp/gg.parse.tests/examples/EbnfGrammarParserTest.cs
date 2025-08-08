using gg.parse.compiler;
using gg.parse.examples;
using gg.parse.rulefunctions;

using static gg.parse.examples.TokenizerCompilerFactory;

namespace gg.parse.tests.examples
{
    [TestClass]
    public class EbnfGrammarParserTest
    {
        [TestMethod]
        public void ParseJsonKeyValue_IntegrationTest()
        {
            var tokenizer = new EbnfTokenizer();
            var tokenizerParser = new EbnfTokenizerParser();
            var jsonTokenizer = CreateTokenizerFromEbnfFile("assets/json_tokens.ebnf", tokenizer, tokenizerParser);
            var jsonParser = CreateParserFromEbnfFile("assets/json_grammar.ebnf", tokenizer, tokenizerParser, jsonTokenizer);

            // xxx left off here this doesn't work
            // test something from the parsertests
            var keyStrValue = "{\"key\": \"value\"}";

            Assert.IsTrue(jsonTokenizer != null);
            Assert.IsTrue(jsonTokenizer.Root != null);
            
            var tokens = jsonTokenizer.Root.Parse(keyStrValue.ToArray(), 0).Annotations;

            Assert.IsTrue(tokens != null);
            Assert.IsTrue(tokens.Count == 5);

            Assert.IsTrue(jsonParser != null);
            Assert.IsTrue(jsonParser.Root != null);

            var result = jsonParser.Root.Parse(tokens.Select(t => t.FunctionId).ToArray(), 0);

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.MatchedLength == tokens.Count);
            Assert.IsTrue(result.Annotations != null);
            Assert.IsTrue(result.Annotations.Count == 1);
            Assert.IsTrue(jsonParser.FindRule("json") != null);
            Assert.IsTrue(result.Annotations[0].FunctionId == jsonParser.FindRule("json")!.Id);
        }



        private RuleTable<char> CreateTokenizerFromEbnfFile(
            string path, 
            EbnfTokenizer tokenizer,
            EbnfTokenizerParser tokenizerParser)
        {
            var tokenizerCompiler = new RuleCompiler<char>();

            var tokenizerText = File.ReadAllText(path);

            var tokenizerTokens = tokenizer.Tokenize(tokenizerText).Annotations;
            var tokenizerAstTree = tokenizerParser.Parse(tokenizerTokens).Annotations;

            var tokenContext = CreateContext<char>(tokenizerText, tokenizerTokens, tokenizerAstTree)
                            .RegisterTokenizerCompilerFunctions(tokenizerParser)
                            .SetProductLookup(tokenizerParser);

            return tokenizerCompiler.Compile(tokenContext);
        }

        private RuleTable<int> CreateParserFromEbnfFile(
            string path,
            EbnfTokenizer tokenizer,
            EbnfTokenizerParser tokenizerParser,
            RuleTable<char> dataTokenizer)
        {
            var grammarText = File.ReadAllText(path);
            var grammarParser = new EbnfGrammarParser(tokenizer, dataTokenizer);
            var (grammarTokens, grammarAstNodes) = grammarParser.Parse(grammarText);

            var grammarcontext = CreateContext<int>(grammarText, grammarTokens, grammarAstNodes)
                                     .RegisterGrammarCompilerFunctions(grammarParser)
                                     .SetProductLookup(tokenizerParser);
                                     

            var grammarCompiler = new RuleCompiler<int>();

            var result = grammarcontext.Output;

            // register the tokens with the grammar compiler
            foreach (var tokenFunctionName in dataTokenizer.FunctionNames)
            {
                var tokenFunction = dataTokenizer.FindRule(tokenFunctionName);

                if (tokenFunction.Production == AnnotationProduct.Annotation)
                {
                    result.RegisterRule(new MatchSingleData<int>($"{tokenFunctionName}", tokenFunction.Id));
                }
            }

            grammarCompiler.Compile(grammarcontext);

            return result;
        }

    }
}
