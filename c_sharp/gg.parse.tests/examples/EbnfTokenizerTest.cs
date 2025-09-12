using gg.parse.ebnf;
using gg.parse.instances.json;
using gg.parse.rulefunctions;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.tests.examples
{
    [TestClass]
    public class EbnfTokenizerTests
    {
        [TestMethod]
        public void TestEmptyObject_ExpectTwoAnnotations()
        {
            var tokenizer = new EbnfTokenizer();
            var rule = "rule_name = *'literal';";

            var (isSuccess, charactersRead, annotations) = tokenizer.Tokenize(rule);

            IsTrue(isSuccess);
            IsTrue(charactersRead == rule.Length);
            IsTrue(annotations!.Count == 5);
            IsTrue(annotations[0].RuleId == tokenizer.FindRule(CommonTokenNames.Identifier).Id);
            IsTrue(annotations[1].RuleId == tokenizer.FindRule(CommonTokenNames.Assignment).Id);
            IsTrue(annotations[2].RuleId == tokenizer.FindRule(CommonTokenNames.ZeroOrMoreOperator).Id);
            IsTrue(annotations[3].RuleId == tokenizer.FindRule(CommonTokenNames.SingleQuotedString).Id);
            IsTrue(annotations[4].RuleId == tokenizer.FindRule(CommonTokenNames.EndStatement).Id);
        }

        [TestMethod]
        public void CreateEvalRule_Tokenize_ExpectOptionWithPrecedenceTokens()
        {
            var tokenizer = new EbnfTokenizer();
            var rule = "rule_name = 'foo' / 'bar' / 'baz';";

            var (isSuccess, charactersRead, annotations) = tokenizer.Tokenize(rule);

            IsTrue(isSuccess);
            IsTrue(charactersRead == rule.Length);
            IsTrue(annotations!.Count == 8);
            IsTrue(annotations[0].RuleId == tokenizer.FindRule(CommonTokenNames.Identifier).Id);
            IsTrue(annotations[1].RuleId == tokenizer.FindRule(CommonTokenNames.Assignment).Id);
            IsTrue(annotations[2].RuleId == tokenizer.FindRule(CommonTokenNames.SingleQuotedString).Id);
            IsTrue(annotations[3].RuleId == tokenizer.FindRule(CommonTokenNames.OptionWithPrecedence).Id);
            IsTrue(annotations[4].RuleId == tokenizer.FindRule(CommonTokenNames.SingleQuotedString).Id);
            IsTrue(annotations[5].RuleId == tokenizer.FindRule(CommonTokenNames.OptionWithPrecedence).Id);
            IsTrue(annotations[6].RuleId == tokenizer.FindRule(CommonTokenNames.SingleQuotedString).Id);
            IsTrue(annotations[7].RuleId == tokenizer.FindRule(CommonTokenNames.EndStatement).Id);
        }

        [TestMethod]
        public void DefineTryMatchRule_Tokenize_ExpectValidMatchAnyTokens()
        {
            var tokenizer = new EbnfTokenizer();
            // try shorthand first
            var rule = "rule_name = >'literal';";

            var (isSuccess, charactersRead, annotations) = tokenizer.Tokenize(rule);

            Assert.IsTrue(isSuccess);
            Assert.IsTrue(charactersRead == rule.Length);
            Assert.IsTrue(annotations!.Count == 5);
            Assert.IsTrue(annotations[0].RuleId == tokenizer.FindRule(CommonTokenNames.Identifier).Id);
            Assert.IsTrue(annotations[1].RuleId == tokenizer.FindRule(CommonTokenNames.Assignment).Id);
            Assert.IsTrue(annotations[2].RuleId == tokenizer.FindRule(CommonTokenNames.TryMatchOperatorShortHand).Id);
            Assert.IsTrue(annotations[3].RuleId == tokenizer.FindRule(CommonTokenNames.SingleQuotedString).Id);
            Assert.IsTrue(annotations[4].RuleId == tokenizer.FindRule(CommonTokenNames.EndStatement).Id);

            // try full operator
            rule = "rule_name = try 'literal';";

            (isSuccess, charactersRead, annotations) = tokenizer.Tokenize(rule);

            Assert.IsTrue(isSuccess);
            Assert.IsTrue(charactersRead == rule.Length);
            Assert.IsTrue(annotations!.Count == 5);
            Assert.IsTrue(annotations[0].RuleId == tokenizer.FindRule(CommonTokenNames.Identifier).Id);
            Assert.IsTrue(annotations[1].RuleId == tokenizer.FindRule(CommonTokenNames.Assignment).Id);
            Assert.IsTrue(annotations[2].RuleId == tokenizer.FindRule(CommonTokenNames.TryMatchOperator).Id);
            Assert.IsTrue(annotations[2].Start == 12);
            Assert.IsTrue(annotations[2].Length == 4);
            Assert.IsTrue(annotations[3].RuleId == tokenizer.FindRule(CommonTokenNames.SingleQuotedString).Id);
            Assert.IsTrue(annotations[4].RuleId == tokenizer.FindRule(CommonTokenNames.EndStatement).Id);
        }

        [TestMethod]
        public void DefineRuleWithoutPrecedence_Tokenize_ExpectValidRuleDeclarations()
        {
            var tokenizer = new EbnfTokenizer();
            
            // no precedence defined
            var rule = "rule_name = .;";

            var (isSuccess, charactersRead, annotations) = tokenizer.Tokenize(rule);

            Assert.IsTrue(isSuccess);
            Assert.IsTrue(charactersRead == rule.Length);
            Assert.IsTrue(annotations!.Count == 4);
            Assert.IsTrue(annotations[0].RuleId == tokenizer.FindRule(CommonTokenNames.Identifier).Id);
            Assert.IsTrue(annotations[1].RuleId == tokenizer.FindRule(CommonTokenNames.Assignment).Id);
            Assert.IsTrue(annotations[2].RuleId == tokenizer.FindRule(CommonTokenNames.AnyCharacter).Id);
            Assert.IsTrue(annotations[3].RuleId == tokenizer.FindRule(CommonTokenNames.EndStatement).Id);
        }

        [TestMethod]
        public void DefineRuleWithPrecedence_Tokenize_ExpectValidRuleDeclarations()
        {
            var tokenizer = new EbnfTokenizer();

            // simple precedence defined
            var ruleDefinitions = new string[] {
                // try different rulename, spacings and precedence values
                "rule_name   100= .;",
                "rule_name 42  = .;",
                "rule_name1 -1  = .;",
            };

            var expectedPrecedences = new int[] { 100, 42, -1 };

            for (var i = 0; i < ruleDefinitions.Length; i++)
            {
                var rule = ruleDefinitions[i];
                var (isSuccess, charactersRead, annotations) = tokenizer.Tokenize(rule);

                IsTrue(isSuccess);
                IsTrue(charactersRead == rule.Length);
                IsTrue(annotations!.Count == 5);
                IsTrue(annotations[0].RuleId == tokenizer.FindRule(CommonTokenNames.Identifier).Id);
                IsTrue(annotations[1].RuleId == tokenizer.FindRule(CommonTokenNames.Integer).Id);

                IsTrue(annotations[1].RuleId == tokenizer.FindRule(CommonTokenNames.Integer).Id);
                var value = ruleDefinitions[i].AsSpan(annotations[1].Range.Start, annotations[1].Range.Length).ToString();
                IsTrue(int.Parse(value) == expectedPrecedences[i]);

                IsTrue(annotations[2].RuleId == tokenizer.FindRule(CommonTokenNames.Assignment).Id);
                IsTrue(annotations[3].RuleId == tokenizer.FindRule(CommonTokenNames.AnyCharacter).Id);
                IsTrue(annotations[4].RuleId == tokenizer.FindRule(CommonTokenNames.EndStatement).Id);
            }
        }


        [TestMethod]
        public void TestJsonFileWithErrors_ExpectLotsOfAnnotationsAndErrors()
        {
            var tokenizer = new EbnfTokenizer(dropComments: true);
            var text = File.ReadAllText("assets/ebnf_tokenizer_example.ebnf");
            var (isSuccess, charactersRead, annotations) = tokenizer.Tokenize(text);

            Assert.IsTrue(isSuccess);

            Directory.CreateDirectory("output");

            File.WriteAllText("output/ebnf_tokenizer_example_with_errors_annotations.html",
                tokenizer.AnnotateTextUsingHtml(text, annotations, AnnotationMarkup.CreateTokenStyleLookup()));
        }
    }
}
