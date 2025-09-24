
using gg.parse.script.common;

namespace gg.parse.json.tests
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
            Assert.IsTrue(annotations[0].Rule == tokenizer.FindRule(CommonTokenNames.ScopeStart));
            Assert.IsTrue(annotations[1].Rule == tokenizer.FindRule(CommonTokenNames.ScopeEnd));
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
    }
}
