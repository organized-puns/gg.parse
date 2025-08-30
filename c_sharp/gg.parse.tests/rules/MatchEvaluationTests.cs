using gg.parse.rulefunctions.datafunctions;
using gg.parse.rulefunctions.rulefunctions;

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

            var addFunction = new MatchFunctionSequence<int>("Add", AnnotationProduct.Annotation, matchNumber, matchOperator, matchNumber)
            {
                Id = 42
            };

            var evalRule = new MatchEvaluation<int>("operator", addFunction)
            {
                Id = 3
            };

            var result = evalRule.Parse([1, 2, 1], 0);

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.MatchedLength == 3);
            Assert.IsTrue(result.Annotations[0].FunctionId == evalRule.Id);
            Assert.IsTrue(result.Annotations[0].Children[0].FunctionId == addFunction.Id);
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

            var addFunction = new MatchFunctionSequence<int>("Add", AnnotationProduct.Annotation, matchNumber, matchOperator, matchNumber)
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
            Assert.IsTrue(result.MatchedLength == 5);
            Assert.IsTrue(result.Annotations[0].FunctionId == evalRule.Id);

            var addRoot = result.Annotations[0].Children[0];
            Assert.IsTrue(addRoot.FunctionId == addFunction.Id);
            Assert.IsTrue(addRoot.Start == 0);
            Assert.IsTrue(addRoot.End == 5);
            
            // check left is another function
            var left = addRoot.Children[0];

            Assert.IsTrue(left.Range.Start == 0);
            Assert.IsTrue(left.Range.End == 3);
            Assert.IsTrue(left.FunctionId == addFunction.Id);
            Assert.IsTrue(left.Children[0].FunctionId == matchNumber.Id);
            Assert.IsTrue(left.Children[1].FunctionId == matchOperator.Id);
            Assert.IsTrue(left.Children[0].FunctionId == matchNumber.Id);

            // check operator
            var op = addRoot.Children[1];
            
            Assert.IsTrue(op.Range.Start == 3);
            Assert.IsTrue(op.Range.End == 4);
            Assert.IsTrue(op.FunctionId == matchOperator.Id);

            // check right
            var right = addRoot.Children[2];

            Assert.IsTrue(right.Range.Start == 4);
            Assert.IsTrue(right.Range.End == 5);
            Assert.IsTrue(right.FunctionId == matchNumber.Id);
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

            var addFunction = new MatchFunctionSequence<int>("Add", AnnotationProduct.Annotation, matchNumber, matchAddOperator, matchNumber)
            {
                Id = 4,
                Precedence = 10
            };

            var multFunction = new MatchFunctionSequence<int>("Multiply", AnnotationProduct.Annotation, matchNumber, matchMultOperator, matchNumber)
            {
                Id = 5,
                Precedence = 100
            };

            // xxx resolve function is not necessary, just go through the options ?
            var evalRule = new MatchEvaluation<int>("operator", addFunction, multFunction)
            {
                Id = 6
            };

            // data could represent something like 3 + 4 * 5
            var result = evalRule.Parse([1, 2, 1, 3, 1], 0);

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.MatchedLength == 5);
            Assert.IsTrue(result.Annotations[0].FunctionId == evalRule.Id);

            var addRoot = result.Annotations[0].Children[0];
            Assert.IsTrue(addRoot.FunctionId == addFunction.Id);
            Assert.IsTrue(addRoot.Start == 0);
            Assert.IsTrue(addRoot.End == 5);

            // check right is another function
            var right = addRoot.Children[2];

            Assert.IsTrue(right.Range.Start == 2);
            Assert.IsTrue(right.Range.End == 5);
            Assert.IsTrue(right.FunctionId == multFunction.Id);
            Assert.IsTrue(right.Children[0].FunctionId == matchNumber.Id);
            Assert.IsTrue(right.Children[1].FunctionId == matchMultOperator.Id);
            Assert.IsTrue(right.Children[0].FunctionId == matchNumber.Id);

            // check operator
            var op = addRoot.Children[1];

            Assert.IsTrue(op.Range.Start == 1);
            Assert.IsTrue(op.Range.End == 2);
            Assert.IsTrue(op.FunctionId == matchAddOperator.Id);

            // check left
            var left = addRoot.Children[0];

            Assert.IsTrue(left.Range.Start == 0);
            Assert.IsTrue(left.Range.End == 1);
            Assert.IsTrue(left.FunctionId == matchNumber.Id);
        }
    }
}
