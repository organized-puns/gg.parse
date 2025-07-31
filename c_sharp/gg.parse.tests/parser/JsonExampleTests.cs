using gg.parse.basefunctions;
using gg.parse.parser;

namespace gg.parse.tests.parser
{
    [TestClass]
    public class JsonExampleTests
    {
        [TestMethod]
        public void TestEmptyObject_ExpectTwoAnnotations()
        {
            var tokenizer = JsonExample.CreateTokenizer();
            var emptyObjectText = "{ }";

            var annotations = tokenizer.Parse(emptyObjectText.ToCharArray());

            Assert.IsTrue(annotations.Count == 2);
            Assert.IsTrue(annotations[0].Category == AnnotationDataCategory.Data);
            Assert.IsTrue(annotations[0].FunctionId == tokenizer.FindFunctionBase(TokenNames.ScopeStart).Id);
            Assert.IsTrue(annotations[1].FunctionId == tokenizer.FindFunctionBase(TokenNames.ScopeEnd).Id);
            Assert.IsTrue(annotations[1].Category == AnnotationDataCategory.Data);

        }


        [TestMethod]
        public void JsonTokenizer_TestEmptyObject_ExpectTwoAnnotations()
        {
            var tokenizer = JsonExample.CreateJsonTokenizer();
            var emptyObjectText = "{ }";

            var annotations = tokenizer.Parse(emptyObjectText.ToCharArray(), 0);

            /*Assert.IsTrue(annotations.Count == 2);
            Assert.IsTrue(annotations[0].Category == AnnotationDataCategory.Data);
            Assert.IsTrue(annotations[0].FunctionId == tokenizer.FindFunctionBase(TokenNames.ScopeStart).Id);
            Assert.IsTrue(annotations[1].FunctionId == tokenizer.FindFunctionBase(TokenNames.ScopeEnd).Id);
            Assert.IsTrue(annotations[1].Category == AnnotationDataCategory.Data);*/

        }

        [TestMethod]
        public void AnnotateJsonTokensTest()
        {
            // Arrange
            var tokenizer = JsonExample.CreateTokenizer();
            var styleLookup = JsonExample.CreateTokenStyleLookup();
            
            // Act
            var text = File.ReadAllText("assets/example.json");
            var annotations = tokenizer.Parse(text.ToCharArray());

            Directory.CreateDirectory("output");

            File.WriteAllText("output/json_example_annotation.html", 
                tokenizer.AnnotateTextUsingHtml(text, annotations, styleLookup));
        }

        [TestMethod]
        public void TestAnnotationSequence_ExpectSuccess()
        {
            var annotations = new List<Annotation>
            {
                new (AnnotationDataCategory.Data, 1, new Range(0, 5)),
                new (AnnotationDataCategory.Data, 2, new Range(5, 3)),
                new (AnnotationDataCategory.Data, 3, new Range(8, 4)),
                new (AnnotationDataCategory.Data, 4, new Range(12, 1))
            };

            var annotationSequence = new MatchDataSequence<int>("test", 42, ProductionEnum.ProduceItem, [1, 2, 3]);
            var result = annotationSequence.Parse(annotations.Select(a => a.FunctionId).ToArray(), 0);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FunctionId == annotationSequence.Id);
            Assert.IsNotNull(result.Start == 0);
            Assert.IsNotNull(result.End == 3);
        }

        [TestMethod]
        public void TestKeyValueParser()
        {
            var tokenizer = JsonExample.CreateTokenizer();
            var text = "\"key\": \"value\"";
            var tokens = tokenizer.Parse(text.ToCharArray());

            var parser = JsonExample.CreateJsonParser(tokenizer);

            var astTree = parser.Parse(tokens.Select(t => t.FunctionId).ToArray());

        }

    }
}
