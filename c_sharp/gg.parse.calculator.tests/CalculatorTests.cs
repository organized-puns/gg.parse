using gg.parse.instances.calculator;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.calculator.tests
{
    [TestClass]
    public class CalculatorTests
    {
        private static readonly string _tokenizerSpec = File.ReadAllText("assets/calculator.tokens");
        private static readonly string _grammarSpec = File.ReadAllText("assets/calculator.grammar");

        [TestMethod]
        public void CreateTokenizerAndGrammar_ParseAndCalculate_ExpectMatchingOutpout()
        {
            var calculator = new CalculatorInterpreter(_tokenizerSpec, _grammarSpec);

            
            (string input, double expectedOutput)[] testValues = [
                ("42", 42.0),
                ("1 + 2-3", 0.0),
                ("1.5 * 2", 3.0),
                ("2 * 2 + 1", 5.0),
                ("2 + 2*1", 4.0),
                ("3- - 3", 6.0),
                ("3-3", 0.0),
                ("3 - +3", 0.0),
                ("3--3", 6.0),
                ("3 - -3", 6.0),
                ("(1)-3", -2.0),
                ("((1))-3", -2.0),
                ("((1))-(3)", -2.0),
                ("2 * 2 + 1-3", 2.0),
                ("2 * 2 + 1- -3", 8.0),
                ("(2 + (2)) + 1", 5.0),
                ("1 - 2 * 2 + 1", -4.0),
                ("(1 - 2) * (2 + 1)", -3.0),
                ("2 * 2 * (2)", 8.0),
                ("2 * 2 * (2 + (3 - -3))", 32.0),
                ("2 * 2 * (2 + (3 - -3)) + --(1)", 33.0),
            ];

            foreach (var (input, expectedOutput) in testValues)
            {
                var output = calculator.Interpret(input);
                IsTrue(Math.Abs(output - expectedOutput) < 0.000001);
            }
        }
    }
}
