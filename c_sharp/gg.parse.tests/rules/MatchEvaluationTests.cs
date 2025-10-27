
using gg.parse.rules;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

using Range = gg.parse.util.Range;

namespace gg.parse.tests.rulefunctions
{
    [TestClass]
    public class MatchEvaluationTests
    {
        /// <summary>
        /// Basic test, have a token set with a single operator. Expect a valid AST
        /// with the format
        ///     - eval
        ///         - function
        ///             - left
        ///             - operator
        ///             - right
        /// </summary>
        [TestMethod]
        public void MatchEvaluationFunction_ValidSingleInput_ReturnsSuccess()
        {
            // Create a proxy for addition, where 1 = token for number and 2 =  token for the operator +
            var matchNumber = new MatchSingleData<int>("number", 1);
            var matchOperator = new MatchSingleData<int>("operator", 2);

            var addFunction = new MatchRuleSequence<int>("Add", AnnotationPruning.None, 0, matchNumber, matchOperator, matchNumber)
            {
                Id = 42
            };

            var evalRule = new MatchEvaluation<int>("operator", addFunction)
            {
                Id = 3
            };

            var result = evalRule.Parse([1, 2, 1], 0);

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.MatchLength == 3);
            Assert.IsTrue(result.Annotations[0].Rule == evalRule);
            Assert.IsTrue(result.Annotations[0].Children[0].Rule == addFunction);
        }

        /// <summary>
        /// Have a token set with a one operator and two operands. Expect a valid AST
        /// with the format
        ///     - eval
        ///         - function
        ///             - function
        ///                 - left
        ///                 - operator
        ///                 - right
        ///             - operator
        ///             - right
        /// </summary>
        [TestMethod]
        public void MatchEvaluationFunction_ValidDoubleInput_ReturnValidLeftToRightAST()
        {
            // Create a proxy for addition, where 1 = token for number and 2 =  token for the operator +
            var matchNumber = new MatchSingleData<int>("number", 1)
            {
                Id = 1
            };
            
            var matchOperator = new MatchSingleData<int>("operator", 2)
            {
                Id = 2
            };

            var addFunction = new MatchRuleSequence<int>("Add", AnnotationPruning.None, 0, matchNumber, matchOperator, matchNumber)
            {
                Id = 42
            };

            var evalRule = new MatchEvaluation<int>("operator", addFunction)
            {
                Id = 3
            };

            // data could represent something like 3 + 4 + 5
            var result = evalRule.Parse([1, 2, 1, 2, 1], 0);

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.MatchLength == 5);
            Assert.IsTrue(result.Annotations[0].Rule == evalRule);

            var addRoot = result.Annotations[0].Children[0];
            Assert.IsTrue(addRoot.Rule == addFunction);
            Assert.IsTrue(addRoot.Start == 0);
            Assert.IsTrue(addRoot.End == 5);
            
            // check left is another function
            var left = addRoot.Children[0];

            Assert.IsTrue(left.Range.Start == 0);
            Assert.IsTrue(left.Range.End == 3);
            Assert.IsTrue(left.Rule == addFunction);
            Assert.IsTrue(left.Children[0].Rule == matchNumber);
            Assert.IsTrue(left.Children[1].Rule == matchOperator);
            Assert.IsTrue(left.Children[0].Rule == matchNumber);

            // check operator
            var op = addRoot.Children[1];
            
            Assert.IsTrue(op.Range.Start == 3);
            Assert.IsTrue(op.Range.End == 4);
            Assert.IsTrue(op.Rule == matchOperator);

            // check right
            var right = addRoot.Children[2];

            Assert.IsTrue(right.Range.Start == 4);
            Assert.IsTrue(right.Range.End == 5);
            Assert.IsTrue(right.Rule == matchNumber);
        }

        /// <summary>
        /// Have a token set with a two operator and two operands, where the second operator
        /// has higher precendence than the first. Expect a valid AST
        /// with the format
        ///     - eval
        ///         - function
        ///             - left
        ///             - operator
        //              - function
        ///                 - left
        ///                 - operator
        ///                 - right
        /// </summary>
        [TestMethod]
        public void MatchEvaluationFunction_ValidDoubleInput_ReturnValidRightPrecedenceAST()
        {
            // Create a proxy for addition, where 1 = token for number and 2 =  token for the operator +
            var matchNumber = new MatchSingleData<int>("number", 1)
            {
                Id = 1
            };

            var matchAddOperator = new MatchSingleData<int>("+", 2)
            {
                Id = 2,
            };

            var matchMultOperator = new MatchSingleData<int>("*", 3)
            {
                Id = 3,
            };

            var addFunction = new MatchRuleSequence<int>("Add", AnnotationPruning.None, 10, matchNumber, matchAddOperator, matchNumber)
            {
                Id = 4,
            };

            var multFunction = new MatchRuleSequence<int>("Multiply", AnnotationPruning.None, 100, matchNumber, matchMultOperator, matchNumber)
            {
                Id = 5,
            };

            // xxx resolve function is not necessary, just go through the options ?
            var evalRule = new MatchEvaluation<int>("operator", addFunction, multFunction)
            {
                Id = 6
            };

            // data could represent something like 3 + 4 * 5
            var result = evalRule.Parse([1, 2, 1, 3, 1], 0);

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.MatchLength == 5);
            Assert.IsTrue(result.Annotations[0].Rule == evalRule);

            var addRoot = result.Annotations[0].Children[0];
            Assert.IsTrue(addRoot.Rule == addFunction);
            Assert.IsTrue(addRoot.Start == 0);
            Assert.IsTrue(addRoot.End == 5);

            // check right is another function
            var right = addRoot.Children[2];

            Assert.IsTrue(right.Range.Start == 2);
            Assert.IsTrue(right.Range.End == 5);
            Assert.IsTrue(right.Rule == multFunction);
            Assert.IsTrue(right.Children[0].Rule == matchNumber);
            Assert.IsTrue(right.Children[1].Rule == matchMultOperator);
            Assert.IsTrue(right.Children[0].Rule == matchNumber);

            // check operator
            var op = addRoot.Children[1];

            Assert.IsTrue(op.Range.Start == 1);
            Assert.IsTrue(op.Range.End == 2);
            Assert.IsTrue(op.Rule == matchAddOperator);

            // check left
            var left = addRoot.Children[0];

            Assert.IsTrue(left.Range.Start == 0);
            Assert.IsTrue(left.Range.End == 1);
            Assert.IsTrue(left.Rule == matchNumber);
        }

        /// <summary>
        /// Have a token set with a two operator and two operands, where the first operator
        /// has higher precendence than the first. Expect a valid AST
        /// with the format
        ///     - eval
        ///         - function
        ///             - function
        ///                 - left
        ///                 - operator
        ///                 - right
        ///             - operator
        ///             - right
        /// </summary>
        [TestMethod]
        public void MatchEvaluationFunction_ValidDoubleInput_ReturnValidLeftPrecedenceAST()
        {
            // Create a proxy for addition, where 1 = token for number and 2 =  token for the operator +
            var matchNumber = new MatchSingleData<int>("number", 1)
            {
                Id = 1
            };

            var matchAddOperator = new MatchSingleData<int>("+", 2)
            {
                Id = 2,
            };

            var matchMultOperator = new MatchSingleData<int>("*", 3)
            {
                Id = 3,
            };

            var addFunction = new MatchRuleSequence<int>("Add", AnnotationPruning.None, 10, matchNumber, matchAddOperator, matchNumber)
            {
                Id = 4,
            };

            var multFunction = new MatchRuleSequence<int>("Multiply", AnnotationPruning.None, 100, matchNumber, matchMultOperator, matchNumber)
            {
                Id = 5,
            };

            var evalRule = new MatchEvaluation<int>("operator", addFunction, multFunction)
            {
                Id = 6
            };

            // data could represent something like 3 * 4 + 5
            var result = evalRule.Parse([1, 3, 1, 2, 1], 0);

            IsTrue(result.FoundMatch);
            IsTrue(result.MatchLength == 5);
            IsTrue(result.Annotations[0].Rule == evalRule);

            var addRoot = result.Annotations[0].Children[0];
            IsTrue(addRoot.Rule == addFunction);
            IsTrue(addRoot.Start == 0);
            IsTrue(addRoot.End == 5);

            // check left is another function
            var left = addRoot.Children[0];

            IsTrue(left.Range.Start == 0);
            IsTrue(left.Range.End == 3);
            IsTrue(left.Rule == multFunction);
            IsTrue(left.Children[0].Rule == matchNumber);
            IsTrue(left.Children[1].Rule == matchMultOperator);
            IsTrue(left.Children[0].Rule == matchNumber);

            // check operator
            var op = addRoot.Children[1];

            IsTrue(op.Range.Start == 3);
            IsTrue(op.Range.End == 4);
            IsTrue(op.Rule == matchAddOperator);

            // check right
            var right = addRoot.Children[2];

            IsTrue(right.Range.Start == 4);
            IsTrue(right.Range.End == 5);
            IsTrue(right.Rule == matchNumber);
        }

        /// <summary>
        /// Have a token set with a two operator and three operands, where the second operator
        /// has higher precendence than the first and the third has a lower precedence
        /// than the second, eg 3 + 4 * 5 + 6. Expect a valid AST
        /// with the format
        ///     - eval
        ///         - function (add)
        ///             - left: function (add)
        ///                 - left: 3
        ///                 - operator: +
        ///                 - right: function (mult)
        ///                     - left: 4
        ///                     - operator: *
        ///                     - right: 5
        ///             - operator: +
        ///             - right: 6
        /// </summary>
        [TestMethod]
        public void MatchEvaluationFunction_ValidTripleInput_ReturnExpectedAST()
        {
            // Create a proxy for addition, where 1 = token for number and 2 =  token for the operator +
            var matchNumber = new MatchSingleData<int>("number", 1)
            {
                Id = 1
            };

            var matchAddOperator = new MatchSingleData<int>("+", 2)
            {
                Id = 2,
            };

            var matchMultOperator = new MatchSingleData<int>("*", 3)
            {
                Id = 3,
            };

            var addFunction = new MatchRuleSequence<int>("Add", AnnotationPruning.None, 10, matchNumber, matchAddOperator, matchNumber)
            {
                Id = 4,
            };

            var multFunction = new MatchRuleSequence<int>("Multiply", AnnotationPruning.None, 100, matchNumber, matchMultOperator, matchNumber)
            {
                Id = 5,
            };

            var evalRule = new MatchEvaluation<int>("operator", addFunction, multFunction)
            {
                Id = 6
            };

            // data could represent something like 3 + 4 * 5 + 6
            var result = evalRule.Parse([1, 2, 1, 3, 1, 2, 1], 0);

            // for expectation see method summary
            IsTrue(result.FoundMatch);
            IsTrue(result[0].Rule == evalRule);

            var root = result[0][0];
            IsTrue(root.Rule == addFunction);
            
            // check left is another function
            var left = root[0];

            IsTrue(left.Rule == addFunction);

            IsTrue(left[0].Rule == matchNumber);
            IsTrue(left[1].Rule == matchAddOperator);

            var rightLeft = left[2];

            IsTrue(rightLeft.Rule == multFunction);
            IsTrue(rightLeft[0].Rule == matchNumber);
            IsTrue(rightLeft[1].Rule == matchMultOperator);
            IsTrue(rightLeft[2].Rule == matchNumber);

            IsTrue(root[1].Rule == matchAddOperator);
            IsTrue(root[2].Rule == matchNumber);

            // test ranges
            // eval
            IsTrue(result[0].Range.Equals(new Range(0, 7)));
            IsTrue(root.Range.Equals(new Range(0, 7)));
            IsTrue(root[1].Range.Equals(new Range(5, 1)));
            IsTrue(root[2].Range.Equals(new Range(6, 1)));
            IsTrue(left.Range.Equals(new Range(0, 5)));
            IsTrue(rightLeft.Range.Equals(new Range(2, 3)));
        }

        /// <summary>
        /// Hypothetical 3 - 4 * 5 + 6. Expect a valid AST
        /// with the format
        ///     - eval
        ///         - function (minus)
        ///             - left: 3
        ///             - operator: -
        ///             - right: function (add)
        ///                 - left: function (mult)
        ///                     - left: 4
        ///                     - operator: *
        ///                     - right: 5
        ///                 - operator: +
        ///                 - right: 6
        /// </summary>
        [TestMethod]
        public void SetupThreeOperators_Parse_ExpectedAST()
        {
            // Create a proxy for addition, where 1 = token for number and 2 =  token for the operator +
            var matchNumber = new MatchSingleData<int>("number", 1)
            {
                Id = 1
            };

            var matchAddOperator = new MatchSingleData<int>("+", 2)
            {
                Id = 2,
            };

            var matchMultOperator = new MatchSingleData<int>("*", 3)
            {
                Id = 3,
            };

            var matchMinOperator = new MatchSingleData<int>("-", 4)
            {
                Id = 4,
            };

            var addFunction = new MatchRuleSequence<int>("Add", AnnotationPruning.None, 10, matchNumber, matchAddOperator, matchNumber)
            {
                Id = 5,
            };

            var multFunction = new MatchRuleSequence<int>("Multiply", AnnotationPruning.None, 100, matchNumber, matchMultOperator, matchNumber)
            {
                Id = 6,
            };

            var minusFunction = new MatchRuleSequence<int>("Minus", AnnotationPruning.None, 1, matchNumber, matchMinOperator, matchNumber)
            {
                Id = 7,
            };

            var evalRule = new MatchEvaluation<int>("operator", addFunction, multFunction, minusFunction)
            {
                Id = 8
            };

            // data could represent something like 3 - 4 * 5 + 6
            var result = evalRule.Parse([1, 4, 1, 3, 1, 2, 1], 0);

            // for expectation see method summary
            IsTrue(result.FoundMatch);
            IsTrue(result[0].Rule == evalRule);

            var root = result[0][0];
            IsTrue(root.Rule == minusFunction);
            
            IsTrue(root[0].Rule == matchNumber);
            IsTrue(root[1].Rule == matchMinOperator);

            var right = root[2];

            IsTrue(right.Rule == addFunction);

            var leftRight = right[0];

            IsTrue(leftRight.Rule == multFunction);
            IsTrue(leftRight[0].Rule == matchNumber);
            IsTrue(leftRight[1].Rule == matchMultOperator);
            IsTrue(leftRight[2].Rule == matchNumber);

            IsTrue(right[1].Rule == matchAddOperator);
            IsTrue(right[2].Rule == matchNumber);
        }

        /// <summary>
        /// Hypothetical 3 - 4 + 5 * 6. Expect a valid AST
        /// with the format
        ///     - eval
        ///         - function (minus)
        ///             - left: 3
        ///             - operator: -
        ///             - right: function (add)
        ///                 - left: 4
        ///                 - operator: +
        ///                 - right: function (mult)
        ///                 
        ///                     
        /// </summary>
        [TestMethod]
        public void SetupThreeOperators_Parse_ExpectedAST__()
        {
            // Create a proxy for addition, where 1 = token for number and 2 =  token for the operator +
            var matchNumber = new MatchSingleData<int>("number", 1)
            {
                Id = 1
            };

            var matchAddOperator = new MatchSingleData<int>("+", 2)
            {
                Id = 2,
            };

            var matchMultOperator = new MatchSingleData<int>("*", 3)
            {
                Id = 3,
            };

            var matchMinOperator = new MatchSingleData<int>("-", 4)
            {
                Id = 4,
            };

            var addFunction = new MatchRuleSequence<int>("Add", AnnotationPruning.None, 10, matchNumber, matchAddOperator, matchNumber)
            {
                Id = 5,
            };

            var multFunction = new MatchRuleSequence<int>("Multiply", AnnotationPruning.None, 100, matchNumber, matchMultOperator, matchNumber)
            {
                Id = 6,
            };

            var minusFunction = new MatchRuleSequence<int>("Minus", AnnotationPruning.None, 1, matchNumber, matchMinOperator, matchNumber)
            {
                Id = 7,
            };

            var evalRule = new MatchEvaluation<int>("operator", addFunction, multFunction, minusFunction)
            {
                Id = 8
            };

            // data could represent something like 3 - 4 + 5 * 6
            var result = evalRule.Parse([1, 4, 1, 2, 1, 3, 1], 0);

            // for expectation see method summary
            IsTrue(result.FoundMatch);
            IsTrue(result[0].Rule == evalRule);

            var root = result[0][0];
            IsTrue(root.Rule == minusFunction);

            IsTrue(root[0].Rule == matchNumber);
            IsTrue(root[1].Rule == matchMinOperator);

            var right = root[2];

            IsTrue(right.Rule == addFunction);

            IsTrue(right[0].Rule == matchNumber);
            IsTrue(right[1].Rule == matchAddOperator);
            IsTrue(right[2].Rule == multFunction);

            IsTrue(right[2][0].Rule == matchNumber);
            IsTrue(right[2][1].Rule == matchMultOperator);
            IsTrue(right[2][2].Rule == matchNumber);

        }
    }
}
