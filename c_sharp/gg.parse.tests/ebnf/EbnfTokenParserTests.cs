
using gg.parse.ebnf;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.tests.ebnf
{
    [TestClass]
    public class EbnfTokenParserTests
    {
        [TestMethod]
        public void CreateRuleWithPrecedence_ParseRule_ExpectRuleToHaveCorrectPrecedence()
        {
            var tokenizer = new EbnfTokenizer();
            var tokenizerParser = new EbnfTokenParser(tokenizer);
            var expectedPrecedence = 100;
            var tokenizeResult = tokenizer.Tokenize($"rule {expectedPrecedence} = .;");

            IsNotNull(tokenizeResult);
            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);
            IsTrue(tokenizeResult.Annotations.Count == 5);

            var parseResult = tokenizerParser.Parse(tokenizeResult.Annotations);

            IsNotNull(parseResult);
            IsTrue(parseResult.FoundMatch);
            IsNotNull(parseResult.Annotations);
            IsTrue(parseResult.Annotations.Count == 1);

            // rule should have precedence set
            var root = parseResult.Annotations[0];

            IsTrue(root.Children != null && root.Children.Count == 3);

            // declaration should have name and precedence 
            IsTrue(root.Children[0].FunctionId == tokenizerParser.MatchRuleName.Id);
            IsTrue(root.Children[1].FunctionId == tokenizerParser.MatchPrecedence.Id);
        }

        [TestMethod]
        public void CreateEvalRule_ParseRule_ExpectEvalRuleAnnotations()
        {
            var tokenizer = new EbnfTokenizer();
            var tokenizerParser = new EbnfTokenParser(tokenizer);
            var tokenizeResult = tokenizer.Tokenize($"rule = 'foo' / 'bar' / 'baz';");

            IsNotNull(tokenizeResult);
            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);
            IsTrue(tokenizeResult.Annotations.Count == 8);

            var parseResult = tokenizerParser.Parse(tokenizeResult.Annotations);

            IsNotNull(parseResult);
            IsTrue(parseResult.FoundMatch);
            IsNotNull(parseResult.Annotations);
            IsTrue(parseResult.Annotations.Count == 1);

            var root = parseResult.Annotations[0];

            IsTrue(root.FunctionId == tokenizerParser.MatchRule.Id);
            IsTrue(root.Children != null && root.Children.Count == 2);

            // declaration should have name and an eval
            IsTrue(root.Children[0].FunctionId == tokenizerParser.MatchRuleName.Id);
            IsTrue(root.Children[1].FunctionId == tokenizerParser.MatchEval.Id);

            var eval = root.Children[1];

            IsTrue(eval.Children != null && eval.Children.Count == 3);

            IsTrue(eval.Children.All(child => child.FunctionId == tokenizerParser.MatchLiteral.Id));
        }

        /// <summary>
        /// Try parse a literal with a production qualifier. This should yield an unexpected product error. 
        /// </summary>
        [TestMethod]
        public void CreateTokensForLiteralWithProductModifier_ParseUnexpectedProductError_ExpectMatchFound()
        {
            TestParseUnexpectedProductError(
                ["#'foo'", "~'bar'"],
                2,
                tokenizerParser => tokenizerParser.MatchLiteral.Id
            );
        }

        /// <summary>
        /// Try parse a range with a production qualifier. This should yield an unexpected product error. 
        /// </summary>
        [TestMethod]
        public void CreateTokensForRangeWithProductModifier_ParseUnexpectedProductError_ExpectMatchFound()
        {
            TestParseUnexpectedProductError(
                ["#{'a'..'z'}", "~{'0'..'9'}"], 
                6, 
                tokenizerParser => tokenizerParser.MatchCharacterRange.Id
            );
        }

        /// <summary>
        /// Try parse a set with a production qualifier. This should yield an unexpected product error. 
        /// </summary>
        [TestMethod]
        public void CreateTokensForSetWithProductModifier_ParseUnexpectedProductError_ExpectMatchFound()
        {
            TestParseUnexpectedProductError(
                ["#{'abc'}", "~{'123'}"],
                4,
                tokenizerParser => tokenizerParser.MatchCharacterSet.Id
            );
        }

        /// <summary>
        /// Try parse a set with a production qualifier. This should yield an unexpected product error. 
        /// </summary>
        [TestMethod]
        public void CreateTokensForAnyWithProductModifier_ParseUnexpectedProductError_ExpectMatchFound()
        {
            TestParseUnexpectedProductError(
                ["#.", "~."],
                2,
                tokenizerParser => tokenizerParser.MatchAnyToken.Id
            );
        }

        public void TestParseUnexpectedProductError(string[] testData, int expectedTokenCount, Func<EbnfTokenParser, int> expectedFunctionId)
        {
            foreach (var inputText in testData)
            {
                var tokenizer = new EbnfTokenizer();
                var tokenizeResult = tokenizer.Tokenize(inputText);

                IsTrue(tokenizeResult.FoundMatch);
                IsTrue(tokenizeResult.Annotations != null && tokenizeResult.Annotations.Count == expectedTokenCount);

                var tokenizerParser = new EbnfTokenParser(tokenizer);
                var errorParseResult = tokenizerParser.MatchUnexpectedProductError.Parse(tokenizeResult.Annotations.Select(a => a.FunctionId).ToArray(), 0);

                IsTrue(errorParseResult.FoundMatch);
                IsTrue(errorParseResult.MatchedLength == expectedTokenCount);
                IsTrue(errorParseResult.Annotations != null
                        && errorParseResult.Annotations.Count == 1
                        && errorParseResult.Annotations[0].Children != null
                        && errorParseResult.Annotations[0].Children!.Count == 3
                        && errorParseResult.Annotations[0].FunctionId == tokenizerParser.MatchUnexpectedProductError.Id
                        && errorParseResult.Annotations[0][1]!.FunctionId == expectedFunctionId(tokenizerParser)
                        && errorParseResult.Annotations[0][2]!.FunctionId == tokenizerParser.UnexpectedProductError.Id);
            }
        }

        /// <summary>
        /// Random # or ~ without following keyword should lead to an error in a rule.
        /// </summary>
        [TestMethod]
        public void CreateRuleWithProductionModifiersInElements_ParseRule_ExpectErrorsInAnnotations()
        {
            var tokenizer = new EbnfTokenizer();
            var tokenizerParser = new EbnfTokenParser(tokenizer);
            var tokenizeResult = tokenizer.Tokenize($"rule = ~foo, #'bar', ~{{'a'..'z'}};");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);
            
            var parseResult = tokenizerParser.Parse(tokenizeResult.Annotations);

            IsTrue(parseResult.FoundMatch);
            IsTrue(parseResult.Annotations != null && parseResult.Annotations.Count == 1);

            // rulename & sequence
            IsTrue(parseResult.Annotations[0].Children != null && parseResult.Annotations[0].Children!.Count == 2);

            var sequence = parseResult[0]![1];
            IsTrue(sequence!.Children != null && sequence!.Children!.Count == 3);
            IsTrue(sequence[0]!.FunctionId == tokenizerParser.MatchIdentifier.Id);
            IsTrue(sequence[1]!.FunctionId == tokenizerParser.MatchUnexpectedProductError.Id);
            IsTrue(sequence[2]!.FunctionId == tokenizerParser.MatchUnexpectedProductError.Id);
        }
    }
}
