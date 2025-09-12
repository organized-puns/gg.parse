using gg.parse.instances.json;
using gg.parse.rulefunctions;

namespace gg.parse.tests.examples
{
    [TestClass]
    public class JsonTokenizerTests
    {
        [TestMethod]
        public void TestEmptyObject_ExpectTwoAnnotations()
        {
            var tokenizer = new JsonTokenizer();
            var emptyObjectText = "{ }";

            var (isSuccess, charactersRead, annotations) = tokenizer.Tokenize(emptyObjectText);

            Assert.IsTrue(isSuccess);
            Assert.IsTrue(charactersRead == 3);
            Assert.IsTrue(annotations!.Count == 2);
            Assert.IsTrue(annotations[0].RuleId == tokenizer.FindRule(CommonTokenNames.ScopeStart).Id);
            Assert.IsTrue(annotations[1].RuleId == tokenizer.FindRule(CommonTokenNames.ScopeEnd).Id);
        }

        [TestMethod]
        public void TestJsonFile_ExpectLotsOfAnnotations()
        {
            var tokenizer = new JsonTokenizer();
            var ((isSuccess, charactersRead, annotations), text) = tokenizer.ParseFile("assets/example.json");

            Assert.IsTrue(isSuccess);

            Directory.CreateDirectory("output");

            File.WriteAllText("output/jsontokenizer_example_annotation.html",
                tokenizer.AnnotateTextUsingHtml(text, annotations, AnnotationMarkup.CreateTokenStyleLookup()));
        }

        [TestMethod]
        public void TestJsonFileWithErrors_ExpectLotsOfAnnotationsAndErrors()
        {
            var tokenizer = new JsonTokenizer();
            var ((isSuccess, charactersRead, annotations), text) = tokenizer.ParseFile("assets/example_with_errors.json");

            Assert.IsTrue(isSuccess);

            Directory.CreateDirectory("output");

            File.WriteAllText("output/jsontokenizer_example_with_errors_annotations.html",
                tokenizer.AnnotateTextUsingHtml(text, annotations, AnnotationMarkup.CreateTokenStyleLookup()));
        }
    }
}
