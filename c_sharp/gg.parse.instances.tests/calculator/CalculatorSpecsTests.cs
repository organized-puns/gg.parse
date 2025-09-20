using gg.parse.ebnf;
using System.Diagnostics;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.instances.tests.calculator
{
    [TestClass]
    public class CalculatorSpecs_1Tests
    {
        private static readonly string _tokenizerSpec = File.ReadAllText("assets/calculator.tokens");
        private static readonly string _grammarSpec = File.ReadAllText("assets/calculator.grammar");

        /// <summary>
        /// Basic test to ensure that all tokens in the calculator example are found.
        /// </summary>
        [TestMethod]
        public void CreateTokenizer_FindTokens_ExpectAllTokensFound()
        {
            var calculatorParser = new ScriptPipeline(_tokenizerSpec, null);

            IsNotNull(calculatorParser);

            var expectedTokens = new[]
            {
                "calculator_tokens",
                "valid_token",
                "plus",
                "minus",
                "mult",
                "div",
                "digit",
                "number",
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
            var calculatorParser = CreateParser();

            IsNotNull(calculatorParser);
            IsNotNull(calculatorParser.EbnfGrammarParser!.Root);

            var expectedRules = new[]
            {
                "expression",
                "term",
                "unary_operation",
                "multiplication",
                "division",
                "addition",
                "subtraction",
                "group",
                "number",
                "plus",
                "minus",
            };

            foreach (var ruleName in expectedRules)
            {
                IsNotNull(calculatorParser.EbnfGrammarParser.FindRule(ruleName), $"Rule '{ruleName}' not found.");
            }
        }

        /// <summary>
        /// See if a simple number gets parsed as expected.
        /// </summary>
        [TestMethod]
        public void CreateTokenizerAndGrammar_ParsePositiveNumber_ExpectCorrectTypeFound()
        {
            var calculatorParser = new ScriptPipeline(_tokenizerSpec, _grammarSpec);

            var tokens = calculatorParser.EbnfTokenizer!.Root!.Parse("1".ToCharArray(), 0);

            IsTrue(tokens.FoundMatch);
            IsTrue(tokens.Annotations != null && tokens.Annotations.Count == 1);
            IsTrue(tokens[0]!.Rule == calculatorParser.EbnfTokenizer.FindRule("number"));

            var astTree = calculatorParser.Parse("1");

            IsTrue(astTree.FoundMatch);
            IsNotNull(astTree.Annotations);
            IsTrue(astTree.Annotations.Count == 1);

            var numberRule = astTree[0]!.Rule;

            IsNotNull(numberRule);
            IsTrue(numberRule.Name == "number");
        }

        /// <summary>
        /// See if a simple negative number gets parsed as expected.
        /// </summary>
        [TestMethod]
        public void CreateTokenizerAndGrammar_ParseNegativeNumber_ExpectCorrectTypeFound()
        {
            var calculatorParser = CreateParser();

            var tokens = calculatorParser.EbnfTokenizer!.Root!.Parse("-1".ToCharArray(), 0);

            IsTrue(tokens.FoundMatch);
            IsTrue(tokens.Annotations != null && tokens.Annotations.Count == 2);
            IsTrue(tokens[0]!.Rule == calculatorParser.EbnfTokenizer.FindRule("minus"));
            IsTrue(tokens[1]!.Rule == calculatorParser.EbnfTokenizer.FindRule("number"));

            var astTree = calculatorParser.Parse("-1");

            IsTrue(astTree.FoundMatch);
            IsNotNull(astTree.Annotations);
            IsTrue(astTree.Annotations.Count == 1);

            IsTrue(astTree[0]!.Rule!.Name == "unary_operation");
            IsTrue(astTree[0]![0]!.Rule!.Name == "minus");
            IsTrue(astTree[0]![1]!.Rule!.Name == "number");
        }

        /// <summary>
        /// See if a simple subtraction gets parsed correctly as an operation.
        /// </summary>
        [TestMethod]
        public void CreateTokenizerAndGrammar_ParseSubtraction_ExpectCorrectTypeFound()
        {
            var calculatorParser = CreateParser();
            var astTree = calculatorParser.Parse("2-1");

            IsTrue(astTree.FoundMatch);
            IsNotNull(astTree.Annotations);
            IsTrue(astTree.Annotations.Count == 1);

            IsTrue(astTree[0]!.Rule!.Name == "subtraction");
            IsTrue(astTree[0]![0]!.Rule!.Name == "number");
            IsTrue(astTree[0]![1]!.Rule!.Name == "minus");
            IsTrue(astTree[0]![2]!.Rule!.Name == "number");
        }


        /// <summary>
        /// See if a simple addition gets parsed as expected.
        /// </summary>
        [TestMethod]
        public void CreateTokenizerAndGrammar_ParseAddition_ExpectCorrectTypeFound()
        {
            var calculatorParser = CreateParser();
            var testText = "-1.01 + -2";
            var astTree = calculatorParser.Parse(testText);

            IsTrue(astTree.FoundMatch);
            IsNotNull(astTree.Annotations);
            IsTrue(astTree.Annotations.Count == 1);

            var addition = astTree[0];

            IsNotNull(addition);

            var additionRule = addition.Rule;

            IsNotNull(additionRule);
            IsTrue(additionRule.Name == "addition");
            IsTrue(additionRule.Precedence == 50);

            IsTrue(addition[0]!.Rule!.Name == "unary_operation");
            IsTrue(addition[0]![0]!.Rule!.Name == "minus");
            IsTrue(addition[0]![1]!.Rule!.Name == "number");

            IsTrue(addition[1]!.Rule!.Name == "plus");
            IsTrue(addition[2]!.Rule!.Name == "unary_operation");
        }

        
        /// <summary>
        /// See if a precedence gets followed with a basic example.
        /// </summary>
        [TestMethod]
        public void CreateTokenizerAndGrammar_ParseOperation_ExpectCorrectLeftToRightPrecedence()
        {
            var calculatorParser = CreateParser();
            var testText = "1 * 2 + 3";
            var astTree = calculatorParser.Parse(testText);

            IsTrue(astTree.FoundMatch);

            // root
            IsTrue(astTree[0].Rule.Name == "addition");

            // left
            IsTrue(astTree[0][0].Rule.Name == "multiplication");

            // left.left
            IsTrue(astTree[0][0][0].Rule.Name == "number");

            // left.op
            IsTrue(astTree[0][0][1].Rule.Name == "mult");

            // left.right
            IsTrue(astTree[0][0][2].Rule.Name == "number");

            // op
            IsTrue(astTree[0][1].Rule.Name == "plus");

            // right
            IsTrue(astTree[0][2].Rule.Name == "number");
        }

        
        /// <summary>
        /// See if a precedence gets followed with an example containing a group.
        /// </summary>
        [TestMethod]
        public void CreateTokenizerAndGrammar_ParseOperationWithGroup_ExpectCorrectRightToLeftPrecedence()
        {
            var calculatorParser = CreateParser();

            var testText = "1 * (2 + 3)";
            var astTree = calculatorParser.Parse(testText);

            IsTrue(astTree.FoundMatch);

            // root
            var f = astTree[0].Rule;
            IsTrue(f.Name == "multiplication");

            // left
            IsTrue(astTree[0][0].Rule.Name == "number");

            // op
            IsTrue(astTree[0][1].Rule.Name == "mult");

            // right
            var group = astTree[0][2];
            IsTrue(group.Rule.Name == "group");

            // group contents
            IsTrue(group[0].Rule.Name == "addition");

            IsTrue(group[0][0].Rule.Name == "number");
            IsTrue(group[0][1].Rule.Name == "plus");
            IsTrue(group[0][2].Rule.Name == "number");
        }

        
        [TestMethod]
        public void CreateTokenizerAndGrammar_ParseSomeWhatComplexOperationWithGroup_ExpectCorrectRightToLeftPrecedence()
        {
            var calculatorParser = CreateParser();
            var testText = "1 - (2 + 3) * 4";
            var astTree = calculatorParser.Parse(testText);

            IsTrue(astTree.FoundMatch);

            // root
            IsTrue(astTree[0].Rule.Name == "subtraction");

            // left
            IsTrue(astTree[0][0].Rule.Name == "number");

            // op
            IsTrue(astTree[0][1].Rule.Name == "minus");

            // right
            IsTrue(astTree[0][2].Rule.Name == "multiplication");

            IsTrue(astTree[0][2][0].Rule.Name == "group");
            IsTrue(astTree[0][2][1].Rule.Name == "mult");
            IsTrue(astTree[0][2][2].Rule.Name == "number");

            // group
            IsTrue(astTree[0][2][0][0].Rule.Name == "addition");
            IsTrue(astTree[0][2][0][0][0].Rule.Name == "number");
            IsTrue(astTree[0][2][0][0][1].Rule.Name == "plus");
            IsTrue(astTree[0][2][0][0][2].Rule.Name == "number");
        }


        [TestMethod]
        public void CreateTokenizerAndGrammar_ParseGroupWithInnerGroup_ExpectToFindAllElements()
        {
            var calculatorParser = CreateParser();
            var testText = "(2 + (3)) + 4";

            IsTrue(calculatorParser.TryBuildAstTree(testText, out var tokens, out var astTree));

            var expectedTokenTypes = new[]
            {
                "group_start",
                "number",
                "plus",
                "group_start",
                "number",
                "group_end",
                "group_end",
                "plus",
                "number"
            };

            IsTrue(tokens!.Annotations!.Count == expectedTokenTypes.Length, $"Expected {expectedTokenTypes.Length} tokens but found {tokens.Annotations.Count} tokens.");

            for (int i = 0; i < expectedTokenTypes.Length; i++)
            {
                var expectedType = expectedTokenTypes[i];
                var actualType = tokens![i]!.Rule!.Name;

                IsTrue(expectedType == actualType, $"Token {i} expected to be '{expectedType}' but found '{actualType}'");
            }


            IsTrue(astTree.FoundMatch);
            IsTrue(astTree.MatchedLength == expectedTokenTypes.Length);

            // root
            IsTrue(astTree![0]!.Rule!.Name == "addition");

            // left
            IsTrue(astTree[0]![0]!.Rule!.Name == "group");

            IsTrue(astTree[0][0][0].Rule.Name == "addition");

            IsTrue(astTree[0][0][0][0].Rule.Name == "number");
            IsTrue(astTree[0][0][0][1].Rule.Name == "plus");
            IsTrue(astTree[0][0][0][2].Rule.Name == "group");

            IsTrue(astTree[0][0][0][2][0].Rule.Name == "number");

            // op
            IsTrue(astTree[0][1].Rule.Name == "plus");

            // right
            IsTrue(astTree[0][2].Rule.Name == "number");
        }

        private ScriptPipeline CreateParser()
        {
            try
            {
                return new ScriptPipeline(_tokenizerSpec, _grammarSpec);
            }
            catch (EbnfException e)
            {
                if (e.InnerException is ParseException ex)
                {
                    if (ex.Errors != null && ex.Text != null && ex.Tokens != null)
                    {
                        var errorMessages = ex.Errors.Select(annotation => $"Error: {annotation.GetText(ex.Text, ex.Tokens)}.");
                        foreach (var errorMessage in errorMessages)
                        {
                            Console.WriteLine(errorMessage);
                            Debug.WriteLine(errorMessage);
                        }
                    }
                }

                throw;
            }
        }
    }
}

