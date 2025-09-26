using gg.parse.instances.calculator;
using gg.parse.rules;
using gg.parse.script.common;
using gg.parse.script.parser;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.calculator.tests
{
    [TestClass]
    public class CalculatorTests
    {
        private static readonly string _tokenizerSpec = File.ReadAllText("assets/calculator.tokens");
        private static readonly string _grammarSpec = File.ReadAllText("assets/calculator.grammar");

        /// <summary>
        /// Spot check for a bug which caused some of the rules' production to fail.
        /// </summary>
        [TestMethod]
        public void CreateInterpreter_FindRules_ExpectRulesToValidate()
        {
            var calculator = new CalculatorInterpreter(_tokenizerSpec, _grammarSpec);
            var parser = calculator.Builder.Parser;
            
            // this is the compiled version of the grammar (script)
            var expressionRule = parser.FindRule("expression") as MatchOneOfFunction<int>;

            IsNotNull(expressionRule);
            IsTrue(expressionRule.Production == AnnotationProduct.Transitive);
            IsTrue(expressionRule.RuleOptions.Length == 2);

            var eofRef = expressionRule.RuleOptions[0] as RuleReference<int>;

            IsNotNull(eofRef);
            IsTrue(eofRef.Production == AnnotationProduct.Annotation);
            IsTrue(eofRef.Reference == "eof");

            var statements = expressionRule.RuleOptions[1] as MatchFunctionCount<int>;

            IsNotNull(statements);
            IsTrue(statements.Min == 1);
            IsTrue(statements.Max == 0);
            IsTrue(statements.Production == AnnotationProduct.Annotation);
        }

        [TestMethod]
        public void CreateAst_ValidateNodes()
        {
            var tokenizer = new ScriptTokenizer();
            var parser = new ScriptParser();

            var tokenizeResult = tokenizer.Tokenize(_tokenizerSpec);

            IsTrue(tokenizeResult.FoundMatch);

            var astResult = parser.ParseGrammar(_grammarSpec, tokenizeResult.Annotations);

            IsTrue(astResult.FoundMatch);
            IsNotNull(astResult.Annotations);

            // expression 
            IsTrue(astResult.Annotations[0].Rule == parser.MatchRule);

            // annotation production
            IsTrue(astResult.Annotations[0][0].Rule == parser.MatchTransitiveSelector);

            // rulename
            IsTrue(astResult.Annotations[0][1].Rule  == parser.MatchRuleName);

            // rule body
            IsTrue(astResult.Annotations[0][2].Rule.GetType() == typeof(MatchOneOfFunction<int>));
        }


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
