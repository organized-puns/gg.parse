using gg.parse.script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gg.parse.calculator.tests
{
    [TestClass]
    public class CalculatorExceptionTests
    {
        [TestMethod]
        public void CreateCalculator_ParseInvalidInput_ExpectException()
        {
            var tokenizerSpec = File.ReadAllText("assets/calculator.tokens");
            var grammarSpec = File.ReadAllText("assets/calculator.grammar");
            var parser = new RuleGraphBuilder().InitializeFromDefinition(tokenizerSpec, grammarSpec);
            string[] invalidInputs = [
                "1 ++ 2",
                "1 ** 2",
                "1 // 2",
                "1 ^^ 2",
                "1 + (2 * 3",
                "1 + 2 * 3)",
                "abc",
                "1 + abc",
                "1 + 2 * (3 - xyz)",
            ];

            foreach (var input in invalidInputs)
            {
                try
                {
                    var output = parser.Parse(input);
                    Assert.Fail($"Expected exception for input: {input}, but got output: {output}");
                }
                catch (Exception ex)
                {
                    // Expected exception
                    Console.WriteLine($"Caught expected exception for input '{input}': {ex.Message}");
                }
            }
        }

    }
}
