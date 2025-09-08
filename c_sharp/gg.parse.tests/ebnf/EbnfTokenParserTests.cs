
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
                "~'foo'",
                2,
                [
                    tokenizerParser => tokenizerParser.MatchNoProductSelector.Id,
                    tokenizerParser => tokenizerParser.MatchLiteral.Id,
                    tokenizerParser => tokenizerParser.UnexpectedProductError.Id
                ]
            );
        }

        /// <summary>
        /// Try parse a range with a production qualifier. This should yield an unexpected product error. 
        /// </summary>
        [TestMethod]
        public void CreateTokensForRangeWithProductModifier_ParseUnexpectedProductError_ExpectMatchFound()
        {
            TestParseUnexpectedProductError(
                "~ {'0'..'9'}",
                6,
                [
                    tokenizerParser => tokenizerParser.MatchNoProductSelector.Id,
                    tokenizerParser => tokenizerParser.MatchCharacterRange.Id,
                    tokenizerParser => tokenizerParser.UnexpectedProductError.Id
                ]
            );
        }

        /// <summary>
        /// Try parse a set with a production qualifier. This should yield an unexpected product error. 
        /// </summary>
        [TestMethod]
        public void CreateTokensForSetWithProductModifier_ParseUnexpectedProductError_ExpectMatchFound()
        {
            TestParseUnexpectedProductError(
                "#{'abc'}",
                4,
                [
                    tokenizerParser => tokenizerParser.MatchTransitiveSelector.Id,
                    tokenizerParser => tokenizerParser.MatchCharacterSet.Id,
                    tokenizerParser => tokenizerParser.UnexpectedProductError.Id
                ]
            );
        }

        /// <summary>
        /// Try parse a set with a production qualifier. This should yield an unexpected product error. 
        /// </summary>
        [TestMethod]
        public void CreateTokensForAnyWithProductModifier_ParseUnexpectedProductError_ExpectMatchFound()
        {
            TestParseUnexpectedProductError(
                "#.",
                2,
                [
                    tokenizerParser => tokenizerParser.MatchTransitiveSelector.Id,
                    tokenizerParser => tokenizerParser.MatchAnyToken.Id,
                    tokenizerParser => tokenizerParser.UnexpectedProductError.Id
                ]
            );
        }

        /// <summary>
        /// Try parse a group with a production qualifier. This should yield an unexpected product error. 
        /// </summary>
        [TestMethod]
        public void CreateTokensForGroupWithProductModifier_ParseUnexpectedProductError_ExpectMatchFound()
        {
            TestParseUnexpectedProductError(
                "#('foo')",
                4,
                [
                    tokenizerParser => tokenizerParser.MatchTransitiveSelector.Id,
                    // group is transitive
                    tokenizerParser => tokenizerParser.MatchLiteral.Id,
                    tokenizerParser => tokenizerParser.UnexpectedProductError.Id
                ]
            );
        }

        /// <summary>
        /// Try parse a not with a production qualifier. This should yield an unexpected product error. 
        /// </summary>
        [TestMethod]
        public void CreateTokensForNotWithProductModifier_ParseUnexpectedProductError_ExpectMatchFound()
        {
            TestParseUnexpectedProductError(
                "~!'foo'",
                3,
                [
                    tokenizerParser => tokenizerParser.MatchNoProductSelector.Id,
                    tokenizerParser => tokenizerParser.MatchNotOperator.Id,
                    tokenizerParser => tokenizerParser.UnexpectedProductError.Id
                ]
            );
        }

        /// <summary>
        /// Try parse a count with a production qualifier. This should yield an unexpected product error. 
        /// </summary>
        [TestMethod]
        public void CreateTokensForCountWithProductModifier_ParseUnexpectedProductError_ExpectMatchFound()
        {
            TestParseUnexpectedProductError(
                "#*'foo'",
                3,
                [
                    tokenizerParser => tokenizerParser.MatchTransitiveSelector.Id,
                    tokenizerParser => tokenizerParser.MatchZeroOrMoreOperator.Id,
                    tokenizerParser => tokenizerParser.UnexpectedProductError.Id
                ]
            );

            TestParseUnexpectedProductError(
                "~?'foo'",
                3,
                [
                    tokenizerParser => tokenizerParser.MatchNoProductSelector.Id,
                    tokenizerParser => tokenizerParser.MatchZeroOrOneOperator.Id,
                    tokenizerParser => tokenizerParser.UnexpectedProductError.Id
                ]
            );

            TestParseUnexpectedProductError(
                "~+'foo'",
                3,
                [
                    tokenizerParser => tokenizerParser.MatchNoProductSelector.Id,
                    tokenizerParser => tokenizerParser.MatchOneOrMoreOperator.Id,
                    tokenizerParser => tokenizerParser.UnexpectedProductError.Id
                ]
            );
        }

        private static void TestParseUnexpectedProductError(
            string testData, 
            int expectedTokenCount, 
            Func<EbnfTokenParser, int>[] expectedFunctionIds
        )
        {
            var tokenizer = new EbnfTokenizer();
            var tokenizeResult = tokenizer.Tokenize(testData);

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
                    && errorParseResult.Annotations[0].FunctionId == tokenizerParser.MatchUnexpectedProductError.Id);

            for (var i = 0; i < expectedFunctionIds.Length; i++)
            {
                IsTrue(errorParseResult.Annotations[0][i]!.FunctionId == expectedFunctionIds[i](tokenizerParser));
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
