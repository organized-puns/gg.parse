// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.script;
using gg.parse.script.parser;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

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
            var parser = new ParserBuilder().From(tokenizerSpec, grammarSpec).Build();
            string[] invalidInputs = [
                // note the calculator only reads a single expression,
                // so anything after the first expression is ignored.
                // So this expression is valid (')' after '3' is ignored):
                //"1 + 2 * 3)",

                "3 * 3 * ",
                "1.0 + -2.0 * ",
                "1.0 + -2.5 - ",


                "1 # 2",
                "1 ** 2",
                
                "1 * ",
                "2 / ",
                "3.5 - ",
                "4 + ",

                "1 * a",
                "2 / b",
                "3.5 - c",
                "4 + #",

                "1+",
                "1+ +",
                "1 - (",
                "1 * r",
                "a * 2",
                ".1 * 2",
                "1 // 2",
                "1 ^^ 2",
                "1 + (2 * 3",
                "1 + ((2 * 3 )",

                "abc",
                "1 + abc",
                "1 + 2 * (3 - xyz)",

                "1 2",
                ")1+ 2)",
            ];

            foreach (var input in invalidInputs)
            {
                try
                {
                    var output = parser.Parse(input);
                    Fail($"Expected exception for input: {input}, but got output: {output}");
                }
                catch (ScriptException)
                {
                }
            }
        }

    }
}
