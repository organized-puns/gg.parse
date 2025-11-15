// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.rules;
using gg.parse.script;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.calculator.tests
{
    [TestClass]
    public class CalculatorTests
    {
        private static readonly string _tokenizerSpec = File.ReadAllText("assets/calculator.tokens");
        private static readonly string _grammarSpec = File.ReadAllText("assets/calculator.grammar");

        /// <summary>
        /// Spot check for a bug which caused some of the rules' output to fail.
        /// </summary>
        [TestMethod]
        public void CreateInterpreter_FindRules_ExpectRulesToValidate()
        {
            var calculator = new CalculatorCompiler(_tokenizerSpec, _grammarSpec);
            var grammar = calculator.Grammar;
            
            // this is the compiled version of the grammar (script)
            var expressionRule = grammar["expression"] as MatchOneOf<int>;

            IsNotNull(expressionRule);
            IsTrue(expressionRule.Prune == AnnotationPruning.Root);
            IsTrue(expressionRule.Count == 3);

            var operationRef = expressionRule[0] as RuleReference<int>;

            IsNotNull(operationRef);
            IsTrue(operationRef.Prune == AnnotationPruning.None);            
            IsTrue(operationRef.ReferenceName == "operation");

            var termRef = expressionRule[1] as RuleReference<int>;

            IsNotNull(termRef);
            IsTrue(termRef.Prune == AnnotationPruning.None);            
            IsTrue(termRef.ReferenceName == "term");

            var unknown = expressionRule[2] as RuleReference<int>;

            IsNotNull(unknown);
            IsTrue(unknown.Prune == AnnotationPruning.None);
            IsTrue(unknown.ReferenceName == "unknown_expression");
        }      

        [TestMethod]
        public void CreateTokenizerAndGrammar_ParseAndCalculate_ExpectMatchingOutpout()
        {

            var calculator = new CalculatorCompiler(_tokenizerSpec, _grammarSpec);

            (string input, double expectedOutput)[] testValues = [
                ("42", 42.0),
                ("1 * -2", -2),
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

        // export the names so we address results by reference
        /*[TestMethod]
        public void ExportNames()
        {
            var builder = new ParserBuilder().From(_tokenizerSpec, _grammarSpec);

            var output = ScriptUtils.ExportNames(builder.TokenGraph, builder.GrammarGraph, "gg.parse.calculator", "CalculatorNames");

            File.WriteAllText("CalculatorNames.cs", output);
        }*/
    }
}
