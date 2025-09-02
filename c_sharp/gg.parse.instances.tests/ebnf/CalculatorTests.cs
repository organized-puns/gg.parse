using gg.parse.ebnf;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.instances.tests.ebnf
{
    [TestClass]
    public class CalculatorTests
    {
        private static readonly string _tokenizerSpec = File.ReadAllText("assets/calculator.tokens");
        private static readonly string _grammarSpec = File.ReadAllText("assets/calculator.grammar");

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
            IsTrue(numberRule.Name == "int");
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
            IsTrue(numberRule.Name == "float");

        }

        /// <summary>
        /// See if a simple addition gets parsed as expected.
        /// </summary>
        [TestMethod]
        public void CreateTokenizerAndGrammar_ParseAddition_ExpectCorrectTypeFound()
        {
            var tokenizerSpec = File.ReadAllText("assets/calculator.tokens");
            var grammarSpec = File.ReadAllText("assets/calculator.grammar");

            var calculatorParser = new EbnfParser(tokenizerSpec, grammarSpec);

            var testText = "-1.0 + 2";
            var tokens = calculatorParser.EbnfTokenizer!.Root!.Parse(testText.ToCharArray(), 0);

            IsTrue(tokens.FoundMatch);
            IsTrue(tokens.Annotations != null && tokens.Annotations.Count == 3);
            IsTrue(tokens[0]!.FunctionId == calculatorParser.EbnfTokenizer.FindRule("float")!.Id);
            IsTrue(tokens[1]!.FunctionId == calculatorParser.EbnfTokenizer.FindRule("plus")!.Id);
            IsTrue(tokens[2]!.FunctionId == calculatorParser.EbnfTokenizer.FindRule("int")!.Id);

            var astTree = calculatorParser.Parse(testText);

            IsTrue(astTree.FoundMatch);
            IsNotNull(astTree.Annotations);
            IsTrue(astTree.Annotations.Count == 1);

            var additionRule = calculatorParser.FindParserRule(astTree[0]!.FunctionId);
            
            IsNotNull(additionRule);
            IsTrue(additionRule.Name == "addition");
            IsTrue(additionRule.Precedence == 50);

            IsTrue(astTree[0]!.Children != null && astTree[0]!.Children!.Count == 3);

            var numberRule = calculatorParser.FindParserRule(astTree[0]![0]!.FunctionId);

            IsNotNull(numberRule);
            IsTrue(numberRule.Name == "float");

            var addOperatorRule = calculatorParser.FindParserRule(astTree[0]![1]!.FunctionId);

            IsNotNull(addOperatorRule);
            IsTrue(addOperatorRule.Name == "plus");

            numberRule = calculatorParser.FindParserRule(astTree[0]![2]!.FunctionId);

            IsNotNull(numberRule);
            IsTrue(numberRule.Name == "int");
        }

        /// <summary>
        /// See if a precedence gets followed with a basic example.
        /// </summary>
        [TestMethod]
        public void CreateTokenizerAndGrammar_ParseOperation_ExpectCorrectLeftToRightPrecedence()
        {
            var calculatorParser = new EbnfParser(_tokenizerSpec, _grammarSpec);

            var testText = "1 * 2 + 3";
            var astTree = calculatorParser.Parse(testText);

            IsTrue(astTree.FoundMatch);
            
            // root
            IsTrue(calculatorParser.FindParserRule(astTree[0].FunctionId).Name == "addition");

            // left
            IsTrue(calculatorParser.FindParserRule(astTree[0][0].FunctionId).Name == "multiplication");

            // left.left
            IsTrue(calculatorParser.FindParserRule(astTree[0][0][0].FunctionId).Name == "int");

            // left.op
            IsTrue(calculatorParser.FindParserRule(astTree[0][0][1].FunctionId).Name == "mult");

            // left.right
            IsTrue(calculatorParser.FindParserRule(astTree[0][0][2].FunctionId).Name == "int");
            
            // op
            IsTrue(calculatorParser.FindParserRule(astTree[0][1].FunctionId).Name == "plus");
            
            // right
            IsTrue(calculatorParser.FindParserRule(astTree[0][2].FunctionId).Name == "int");
        }

        /// <summary>
        /// See if a precedence gets followed with an example containing a group.
        /// </summary>
        [TestMethod]
        public void CreateTokenizerAndGrammar_ParseOperationWithGroup_ExpectCorrectRightToLeftPrecedence()
        {
            var calculatorParser = new EbnfParser(_tokenizerSpec, _grammarSpec);

            var testText = "1 * (2 + 3)";
            var astTree = calculatorParser.Parse(testText);

            IsTrue(astTree.FoundMatch);

            // root
            var f = calculatorParser.FindParserRule(astTree[0].FunctionId);
            IsTrue(f.Name == "multiplication");

            // left
            IsTrue(calculatorParser.FindParserRule(astTree[0][0].FunctionId).Name == "int");

            // op
            IsTrue(calculatorParser.FindParserRule(astTree[0][1].FunctionId).Name == "mult");

            // right
            var group = astTree[0][2];
            IsTrue(calculatorParser.FindParserRule(group.FunctionId).Name == "group");

            // group contents
            IsTrue(calculatorParser.FindParserRule(group[0].FunctionId).Name == "addition");

            IsTrue(calculatorParser.FindParserRule(group[0][0].FunctionId).Name == "int");
            IsTrue(calculatorParser.FindParserRule(group[0][1].FunctionId).Name == "plus");
            IsTrue(calculatorParser.FindParserRule(group[0][2].FunctionId).Name == "int");
        }
    }
}

