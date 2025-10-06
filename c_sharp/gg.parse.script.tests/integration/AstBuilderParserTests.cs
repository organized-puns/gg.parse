
using gg.parse.rules;

using gg.parse.script.common;
using gg.parse.script.parser;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

using Range = gg.parse.util.Range;

namespace gg.parse.script.tests.integration
{
    [TestClass]
    public class AstBuilderParserTests
    {
        [TestMethod]
        public void CreateRuleWithPrecedence_ParseRule_ExpectRuleToHaveCorrectPrecedence()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizerParser = new ScriptParser(tokenizer);
            var expectedPrecedence = 100;
            var tokenizeResult = tokenizer.Tokenize($"rule {expectedPrecedence} = .;");

            IsNotNull(tokenizeResult);
            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);
            IsTrue(tokenizeResult.Annotations.Count == 5);

            var parseResult = tokenizerParser.Root!.Parse(tokenizeResult.Annotations);

            IsNotNull(parseResult);
            IsTrue(parseResult.FoundMatch);
            IsNotNull(parseResult.Annotations);
            IsTrue(parseResult.Annotations.Count == 1);

            // rule should have precedence set
            var root = parseResult.Annotations[0];

            IsTrue(root.Children != null && root.Children.Count == 3);

            // declaration should have name and precedence 
            IsTrue(root.Children[0].Rule == tokenizerParser.MatchRuleName);
            IsTrue(root.Children[1].Rule == tokenizerParser.MatchPrecedence);
        }

        [TestMethod]
        public void CreateEvalRule_ParseRule_ExpectEvalRuleAnnotations()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizerParser = new ScriptParser(tokenizer);
            var tokenizeResult = tokenizer.Tokenize($"rule = 'foo' / 'bar' / 'baz';");

            IsNotNull(tokenizeResult);
            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);
            IsTrue(tokenizeResult.Annotations.Count == 8);

            var parseResult = tokenizerParser.Root!.Parse(tokenizeResult.Annotations);

            IsNotNull(parseResult);
            IsTrue(parseResult.FoundMatch);
            IsNotNull(parseResult.Annotations);
            IsTrue(parseResult.Annotations.Count == 1);

            var root = parseResult.Annotations[0];

            IsTrue(root.Rule == tokenizerParser.MatchRule);
            IsTrue(root.Children != null && root.Children.Count == 2);

            // declaration should have name and an eval
            IsTrue(root.Children[0].Rule == tokenizerParser.MatchRuleName);
            IsTrue(root.Children[1].Rule == tokenizerParser.MatchEval);

            var eval = root.Children[1];

            IsTrue(eval.Children != null && eval.Children.Count == 3);

            IsTrue(eval.Children.All(child => child.Rule == tokenizerParser.MatchLiteral));
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
                    tokenizerParser => tokenizerParser.UnexpectedProductInBodyError.Id
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
                    tokenizerParser => tokenizerParser.UnexpectedProductInBodyError.Id
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
                    tokenizerParser => tokenizerParser.UnexpectedProductInBodyError.Id
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
                    tokenizerParser => tokenizerParser.UnexpectedProductInBodyError.Id
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
                    tokenizerParser => tokenizerParser.UnexpectedProductInBodyError.Id
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
                    tokenizerParser => tokenizerParser.UnexpectedProductInBodyError.Id
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
                    tokenizerParser => tokenizerParser.UnexpectedProductInBodyError.Id
                ]
            );

            TestParseUnexpectedProductError(
                "~?'foo'",
                3,
                [
                    tokenizerParser => tokenizerParser.MatchNoProductSelector.Id,
                    tokenizerParser => tokenizerParser.MatchZeroOrOneOperator.Id,
                    tokenizerParser => tokenizerParser.UnexpectedProductInBodyError.Id
                ]
            );

            TestParseUnexpectedProductError(
                "~+'foo'",
                3,
                [
                    tokenizerParser => tokenizerParser.MatchNoProductSelector.Id,
                    tokenizerParser => tokenizerParser.MatchOneOrMoreOperator.Id,
                    tokenizerParser => tokenizerParser.UnexpectedProductInBodyError.Id
                ]
            );
        }

        private static void TestParseUnexpectedProductError(
            string testData,
            int expectedTokenCount,
            Func<ScriptParser, int>[] expectedFunctionIds
        )
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizeResult = tokenizer.Tokenize(testData);

            IsTrue(tokenizeResult.FoundMatch);
            IsTrue(tokenizeResult.Annotations != null && tokenizeResult.Annotations.Count == expectedTokenCount);

            var tokenizerParser = new ScriptParser(tokenizer);
            var errorParseResult = tokenizerParser.MatchUnexpectedProductInBodyError.Parse(tokenizeResult.Annotations.Select(a => a.Rule.Id).ToArray(), 0);

            IsTrue(errorParseResult.FoundMatch);
            IsTrue(errorParseResult.MatchLength == expectedTokenCount);
            IsTrue(errorParseResult.Annotations != null
                    && errorParseResult.Annotations.Count == 1
                    && errorParseResult.Annotations[0].Children != null
                    && errorParseResult.Annotations[0].Children!.Count == 3
                    && errorParseResult.Annotations[0].Rule == tokenizerParser.MatchUnexpectedProductInBodyError);

            for (var i = 0; i < expectedFunctionIds.Length; i++)
            {
                IsTrue(errorParseResult.Annotations[0][i]!.Rule.Id == expectedFunctionIds[i](tokenizerParser));
            }
        }

        /// <summary>
        /// Random # or ~ without following keyword should lead to an error in a rule.
        /// </summary>
        [TestMethod]
        public void CreateRuleWithProductionModifiersInElements_ParseRule_ExpectErrorsInAnnotations()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizerParser = new ScriptParser(tokenizer);
            var tokenizeResult = tokenizer.Tokenize($"rule = ~foo, #'bar', ~{{'a'..'z'}};");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var parseResult = tokenizerParser.Root.Parse(tokenizeResult.Annotations);

            IsTrue(parseResult.FoundMatch);
            IsTrue(parseResult.Annotations != null && parseResult.Annotations.Count == 1);

            // rulename & sequence
            IsTrue(parseResult.Annotations[0].Children != null && parseResult.Annotations[0].Children!.Count == 2);

            var sequence = parseResult[0]![1];
            IsTrue(sequence!.Children != null && sequence!.Children!.Count == 3);
            IsTrue(sequence[0]!.Rule == tokenizerParser.MatchIdentifier);
            IsTrue(sequence[1]!.Rule == tokenizerParser.MatchUnexpectedProductInBodyError);
            IsTrue(sequence[2]!.Rule == tokenizerParser.MatchUnexpectedProductInBodyError);
        }

        [TestMethod]
        public void CreateRuleWithInvalidRuleDefinition_Parse_ExpectDefintionMarkedWithError()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizerParser = new ScriptParser(tokenizer);
            var tokenizeResult = tokenizer.Tokenize($"rule = *; rule2 = 'foo';");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var parseResult = tokenizerParser.Root!.Parse(tokenizeResult.Annotations);

            IsTrue(parseResult.FoundMatch);
            IsTrue(parseResult.Annotations != null && parseResult.Annotations.Count == 2);
            IsTrue(parseResult.Annotations[0] != null 
                    && parseResult.Annotations[0]!.Children != null
                    && parseResult.Annotations[0]!.Children!.Count == 2);
            IsTrue(parseResult.Annotations[0]!.Children![1].Rule == tokenizerParser.MatchZeroOrMoreOperator);
            IsTrue(parseResult.Annotations[0]!.Children![1].Range.Equals(new Range(2, 1)));

            IsTrue(parseResult[0]![1]![0]!.Rule == tokenizerParser.MissingUnaryOperatorTerm);
        }

        [TestMethod]
        public void CreateRuleWithMissingEndRule_Parse_ExpectErrorRaised()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizerParser = new ScriptParser(tokenizer);
            var tokenizeResult = tokenizer.Tokenize($"rule = 'bar' rule2 = 'foo'");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var parseResult = tokenizerParser.Root!.Parse(tokenizeResult.Annotations);

            IsTrue(parseResult.FoundMatch);
            IsTrue(parseResult.Annotations != null && parseResult.Annotations.Count == 2);
            IsTrue(parseResult.Annotations[0] != null
                    && parseResult.Annotations[0]!.Children != null
                    && parseResult.Annotations[0]!.Children!.Count == 3);
            IsTrue(parseResult.Annotations[0]!.Children![2].Rule == tokenizerParser.MissingRuleEndError);

            IsTrue(parseResult.Annotations[1] != null
                    && parseResult.Annotations[1]!.Children != null
                    && parseResult.Annotations[1]!.Children!.Count == 3);
            IsTrue(parseResult.Annotations[1]!.Children![2].Rule == tokenizerParser.MissingRuleEndError);
        }

        [TestMethod]
        public void CreateRuleWithMissingRemainderOperator_Parse_ExpectErrorRaised()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizerParser = new ScriptParser(tokenizer);
            var tokenizeResult = tokenizer.Tokenize($"rule = a, b c;");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var parseResult = tokenizerParser.Root!.Parse(tokenizeResult.Annotations);

            IsTrue(parseResult.FoundMatch);
            
            // expecting: rule[0] / sequence[1] / error[2]
            var errorRule = parseResult[0]![1]![2]!.Rule;
            var expectedRule = tokenizerParser.MissingOperatorError[CommonTokenNames.CollectionSeparator];

            IsTrue(errorRule == expectedRule);
        }

        [TestMethod]
        public void CreateRuleWithDifferentRemainderOperator_Parse_ExpectErrorRaised()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizerParser = new ScriptParser(tokenizer);
            var tokenizeResult = tokenizer.Tokenize($"r1 = a, b |c; r2 = d;");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var parseResult = tokenizerParser.Root!.Parse(tokenizeResult.Annotations);

            IsTrue(parseResult.FoundMatch);

            // should find two rules
            IsTrue(parseResult.Annotations!.Count == 2);
            
            // expecting: rule[0] / sequence[1] / error[2]

            var errorRule = parseResult[0]![1]![2]!.Rule;

            // name should be error containing an indication what operator we're missing
            var expectedRule = tokenizerParser.WrongOperatorTokenError[CommonTokenNames.CollectionSeparator];

            // name should be error containing an indication what operator we're missing
            IsTrue(errorRule == expectedRule);
        }

        [TestMethod]
        public void CreateRuleWithMissingTermsAfterOperatorInRemainder_Parse_ExpectErrorRaised()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizerParser = new ScriptParser(tokenizer);
            var tokenizeResult = tokenizer.Tokenize($"r1 = a, b,; r2 = d;");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var parseResult = tokenizerParser.Root!.Parse(tokenizeResult.Annotations);

            IsTrue(parseResult.FoundMatch);

            // should find two rules
            IsTrue(parseResult.Annotations!.Count == 2);

            // expecting: rule[0] / sequence[1] / error[2]

            var errorRule = parseResult[0]![1]![2]!.Rule;
            var expectedRule = tokenizerParser.MissingTermAfterOperatorInRemainderError[CommonTokenNames.CollectionSeparator];

            // name should be error containing an indication what operator we're missing
            IsTrue(errorRule == expectedRule);
        }

        [TestMethod]
        public void CreateRuleWithMissingTermsAfterOperator_Parse_ExpectErrorRaised()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizerParser = new ScriptParser(tokenizer);
            var tokenizeResult = tokenizer.Tokenize($"r1 = a,; r2 = d;");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var parseResult = tokenizerParser.Root!.Parse(tokenizeResult.Annotations);

            IsTrue(parseResult.FoundMatch);

            // should find two rules
            IsTrue(parseResult.Annotations!.Count == 2);

            // expecting: rule[0] / sequence[1] / error[2]

            var errorRule = parseResult[0]![1]!.Rule;
            var expectedRule = tokenizerParser.MissingTermAfterOperatorError[CommonTokenNames.CollectionSeparator];

            // name should be error containing an indication what operator we're missing
            IsTrue(errorRule == expectedRule);
        }

        [TestMethod]
        public void CreateLogErrorRuleWithText_ParseWithMatchLog_ExpectMatchFound()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizeResult = tokenizer.Tokenize($"error 'text'");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var tokens = tokenizeResult.CollectRuleIds();
            
            var tokenizerParser = new ScriptParser(tokenizer);
            var parseResult = tokenizerParser.MatchLog.Parse(tokens, 0);

            IsTrue(parseResult.FoundMatch);
            IsTrue(parseResult[0]!.Children != null && parseResult[0]!.Children!.Count == 2);
        }

        [TestMethod]
        public void CreateLogErrorRuleWithTextAndCondition_ParseWithMatchLog_ExpectMatchFound()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizeResult = tokenizer.Tokenize($"warning 'text' if !'foo'");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var tokens = tokenizeResult.CollectRuleIds();

            var tokenizerParser = new ScriptParser(tokenizer);
            var parseResult = tokenizerParser.MatchLog.Parse(tokens, 0);

            IsTrue(parseResult.FoundMatch);
            IsTrue(parseResult[0]!.Children != null && parseResult[0]!.Children!.Count == 3);
        }

        [TestMethod]
        public void CreateRuleWithNoBody_ParseWithMatchRule_ExpectWarning()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizeResult = tokenizer.Tokenize($"rule = ;");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var tokens = tokenizeResult.CollectRuleIds();

            var parser = new ScriptParser(tokenizer);
            var ruleMatcher = parser.MatchRule;
            var parseResult = ruleMatcher.Parse(tokens, 0);

            IsTrue(parseResult.FoundMatch);
            var warning = parseResult[0]![1]!.Rule as LogRule<int>;

            IsNotNull(warning);
            IsNotNull(warning.Level == LogLevel.Warning);
        }

        [TestMethod]
        public void CreateRuleWithReferenceAndProduction_ParseWithMatchRule_ExpectCorrectProductionModifiers()
        {
            var tokenizer = new ScriptTokenizer();
            var parser = new ScriptParser(tokenizer);
            var (_, astNodes) = parser.Parse("foo='foo';sequence = ~foo, #foo, foo;");

            // expect two rules
            IsTrue(astNodes.Count == 2);

            // expect production modifiers for each of the sequence's terms
            var sequenceNode = astNodes[1][1];

            var modifierNode = sequenceNode[0][0];
            
            IsTrue(modifierNode.Rule == parser.MatchNoProductSelector);

            modifierNode = sequenceNode[1][0];

            IsTrue(modifierNode.Rule == parser.MatchTransitiveSelector);

            // no modifier selector
            IsTrue(sequenceNode[2][0].Rule == parser.IdentifierToken);
        }
    }
}
