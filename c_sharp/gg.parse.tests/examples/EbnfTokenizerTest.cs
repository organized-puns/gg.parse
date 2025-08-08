using gg.parse.ebnf;
using gg.parse.examples;
using gg.parse.rulefunctions;

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

            Assert.IsTrue(isSuccess);
            Assert.IsTrue(charactersRead == rule.Length);
            Assert.IsTrue(annotations!.Count == 5);
            Assert.IsTrue(annotations[0].FunctionId == tokenizer.FindRule(TokenNames.Identifier).Id);
            Assert.IsTrue(annotations[1].FunctionId == tokenizer.FindRule(TokenNames.Assignment).Id);
            Assert.IsTrue(annotations[2].FunctionId == tokenizer.FindRule(TokenNames.ZeroOrMoreOperator).Id);
            Assert.IsTrue(annotations[3].FunctionId == tokenizer.FindRule(TokenNames.SingleQuotedString).Id);
            Assert.IsTrue(annotations[4].FunctionId == tokenizer.FindRule(TokenNames.EndStatement).Id);
        }
        
        [TestMethod]
        public void TestJsonFileWithErrors_ExpectLotsOfAnnotationsAndErrors()
        {
            var tokenizer = new EbnfTokenizer(dropComments: true);
            var ((isSuccess, charactersRead, annotations), text) = tokenizer.ParseFile("assets/ebnf_tokenizer_example.ebnf");

            Assert.IsTrue(isSuccess);

            Directory.CreateDirectory("output");

            File.WriteAllText("output/ebnf_tokenizer_example_with_errors_annotations.html",
                tokenizer.AnnotateTextUsingHtml(text, annotations, AnnotationMarkup.CreateTokenStyleLookup()));
        }
    }
}
