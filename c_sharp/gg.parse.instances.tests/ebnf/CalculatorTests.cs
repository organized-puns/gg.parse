using gg.parse.ebnf;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.instances.tests.ebnf
{
    [TestClass]
    public class CalculatorTests
    {
        /// <summary>
        /// Basic test to ensure that all tokens in the calculator example are found.
        /// </summary>
        [TestMethod]
        public void CreateTokenizer_FindTokens_ExpectAllTokensFound()
        {
            var tokenizerSpec = File.ReadAllText("assets/calculator.tokens");
            
            var calculatorParser = new EbnfParser(tokenizerSpec, null);

            IsNotNull(calculatorParser);

            var expectedTokens = new[]
            {
                "calculator_tokens",
                "valid_token",
                "plus",
                "minus",
                "mult",
                "div",
                "sign",
                "digit",
                "float",
                "int",
                "group_start",
                "group_end",
                "white_space",
                "unknown_token"
            };

            foreach (var token in expectedTokens)
            {
                IsNotNull(calculatorParser.EbnfTokenizer.FindRule(token), $"Token '{token}' not found.");
            }
        }

        /// <summary>
        /// Basic test to ensure that all grammar rules in the calculator example are found.
        /// </summary>
        [TestMethod]
        public void CreateTokenizerAndGrammar_FindTokens_ExpectAllTokensFound()
        {
            var tokenizerSpec = File.ReadAllText("assets/calculator.tokens");
            var grammarSpec = File.ReadAllText("assets/calculator.grammar");

            var calculatorParser = new EbnfParser(tokenizerSpec, grammarSpec);

            IsNotNull(calculatorParser);
            IsNotNull(calculatorParser.EbnfGrammarParser!.Root);

            var expectedRules = new[]
            {
                "expression",
                "multiplication",
                "division",
                "addition",
                "subtraction",
                "group",
                "int",
                "number",
                "float"
            };

            foreach (var ruleName in expectedRules)
            {
                IsNotNull(calculatorParser.EbnfGrammarParser.FindRule(ruleName), $"Rule '{ruleName}' not found.");
            }
        }

        /// <summary>
        /// See if a simple int gets parsed as expected.
        /// </summary>
        [TestMethod]
        public void CreateTokenizerAndGrammar_ParseInt_ExpectCorrectTypeFound()
        {
            var tokenizerSpec = File.ReadAllText("assets/calculator.tokens");
            var grammarSpec = File.ReadAllText("assets/calculator.grammar");

            var calculatorParser = new EbnfParser(tokenizerSpec, grammarSpec);

            var tokens = calculatorParser.EbnfTokenizer!.Root!.Parse("1".ToCharArray(), 0);

            IsTrue(tokens.FoundMatch);
            IsTrue(tokens.Annotations != null && tokens.Annotations.Count == 1);
            IsTrue(tokens[0]!.FunctionId == calculatorParser.EbnfTokenizer.FindRule("int")!.Id);

            var astTree = calculatorParser.Parse("1");

            IsTrue(astTree.FoundMatch);
            IsNotNull(astTree.Annotations);
            IsTrue(astTree.Annotations.Count == 1);

            var numberRule = calculatorParser.FindParserRule(astTree[0]!.FunctionId);

            IsNotNull(numberRule);
            IsTrue(numberRule.Name == "number");

            IsTrue(astTree[0]!.Children != null);
            IsTrue(astTree[0]!.Children.Count == 1);

            var intRule = calculatorParser.FindParserRule(astTree[0][0]!.FunctionId);

            IsNotNull(intRule);
            IsTrue(intRule.Name == "int");
        }

        /// <summary>
        /// See if a simple float gets parsed as expected.
        /// </summary>
        [TestMethod]
        public void CreateTokenizerAndGrammar_ParseFloat_ExpectCorrectTypeFound()
        {
            var tokenizerSpec = File.ReadAllText("assets/calculator.tokens");
            var grammarSpec = File.ReadAllText("assets/calculator.grammar");

            var calculatorParser = new EbnfParser(tokenizerSpec, grammarSpec);

            var tokens = calculatorParser.EbnfTokenizer!.Root!.Parse("-1.0".ToCharArray(), 0);

            IsTrue(tokens.FoundMatch);
            IsTrue(tokens.Annotations != null && tokens.Annotations.Count == 1);
            IsTrue(tokens[0]!.FunctionId == calculatorParser.EbnfTokenizer.FindRule("float")!.Id);

            var astTree = calculatorParser.Parse("-1.0");

            IsTrue(astTree.FoundMatch);
            IsNotNull(astTree.Annotations);
            IsTrue(astTree.Annotations.Count == 1);

            var numberRule = calculatorParser.FindParserRule(astTree[0]!.FunctionId);

            IsNotNull(numberRule);
            IsTrue(numberRule.Name == "number");

            IsTrue(astTree[0]!.Children != null);
            IsTrue(astTree[0]!.Children.Count == 1);

            var floatRule = calculatorParser.FindParserRule(astTree[0][0]!.FunctionId);

            IsNotNull(floatRule);
            IsTrue(floatRule.Name == "float");
        }
    }
}
