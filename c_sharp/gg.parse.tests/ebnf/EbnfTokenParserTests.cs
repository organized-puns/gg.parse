
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
        /// Random # or ~ without following keyword should lead to an error that these are being ignored.
        /// </summary>
        [TestMethod]
        public void CreateGroupedRuleWithProductionModifier_ParseRule_ExpectAnnotationIndicatingProductionsAreIgnored()
        {
            var tokenizer = new EbnfTokenizer();
            var tokenizerParser = new EbnfTokenParser(tokenizer);
            var tokenizeResult = tokenizer.Tokenize($"rule = #('foo', ~bar, ~'baz', 'qad', #) ~;");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);
            IsTrue(tokenizeResult.Annotations.Count == 18);

            var parseResult = tokenizerParser.Parse(tokenizeResult.Annotations);

            IsTrue(parseResult.FoundMatch);
            IsTrue(parseResult.Annotations != null && parseResult.Annotations.Count == 1);

            // xxx fix this
            // test whether or not the parser caught the stray modifiers
            IsTrue(parseResult.Annotations[0].Children != null && parseResult.Annotations[0].Children!.Count == 4);

            // children = rulename, UnexpectedProductError, sequence, UnexpectedProductError
            IsTrue(parseResult.Annotations[0].Children!.Count( c => c.FunctionId == tokenizerParser.UnexpectedProductError.Id ) == 2);

            IsTrue(parseResult.Annotations[0][2]!.Children != null && parseResult.Annotations[0][2]!.Children!.Count == 6);

            // sequence = literal, ref, UnexpectedProductError, literal, literal, UnexpectedProductError
            IsTrue(parseResult.Annotations[0][2]!.Children!.Count(c => c.FunctionId == tokenizerParser.UnexpectedProductError.Id) == 2);
        }
    }
}
