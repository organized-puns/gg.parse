using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

using gg.parse.ebnf;

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
    }
}
