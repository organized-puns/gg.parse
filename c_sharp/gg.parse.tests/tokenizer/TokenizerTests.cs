using gg.parse.tokenizer;

namespace gg.parse.tests.tokenizer
{

    [TestClass]
    public class TokenizerTests
    {
        [TestMethod]
        public void EmptyJsonObject_ExpectTwoAnnotations()
        {
            // Arrange simple setup with an empty JSON object
            var tokenizer = TokenizerTools.CreateJsonTokenizer();
            var emptyObjectText = "{ }";

            // Act
            var annotations = tokenizer.Tokenize(emptyObjectText);

            Assert.IsTrue(annotations.Count == 2);
            Assert.IsTrue(annotations[0].Start == 0);
            Assert.IsTrue(annotations[0].Length == 1);
            Assert.IsTrue(annotations[1].Start == 2);
            Assert.IsTrue(annotations[1].Length == 1);
            Assert.IsTrue(tokenizer[annotations[0].ReferenceId].Name == TokenizerTools.TokenNames.ObjectDelimiter);
            Assert.IsTrue(tokenizer[annotations[1].ReferenceId].Name == TokenizerTools.TokenNames.ObjectDelimiter);
        }

        [TestMethod]
        public void KeyValueJsonObject_ExpectFiveAnnotations()
        {
            // Arrange simple setup with an empty JSON object
            var tokenizer = TokenizerTools.CreateJsonTokenizer();
            var keyValuePair = "{ \"key\": 123 }";

            // Act
            var annotations = tokenizer.Tokenize(keyValuePair);

            Assert.IsTrue(annotations.Count == 5);
            
            Assert.IsTrue(annotations[0].Start == 0);
            Assert.IsTrue(annotations[0].Length == 1);
            
            Assert.IsTrue(annotations[1].Start == 2);
            Assert.IsTrue(annotations[1].Length == 5);

            Assert.IsTrue(annotations[2].Start == 7);
            Assert.IsTrue(annotations[2].Length == 1);

            Assert.IsTrue(annotations[3].Start == 9);
            Assert.IsTrue(annotations[3].Length == 3);

            Assert.IsTrue(annotations[4].Start == 13);
            Assert.IsTrue(annotations[4].Length == 1);

            Assert.IsTrue(tokenizer[annotations[0].ReferenceId].Name == TokenizerTools.TokenNames.ObjectDelimiter);
            Assert.IsTrue(tokenizer[annotations[1].ReferenceId].Name == BasicTokenizerFunctions.TokenNames.String);
            Assert.IsTrue(tokenizer[annotations[2].ReferenceId].Name == TokenizerTools.TokenNames.KeyValueSeparator);
            Assert.IsTrue(tokenizer[annotations[3].ReferenceId].Name == BasicTokenizerFunctions.TokenNames.Integer);
            Assert.IsTrue(tokenizer[annotations[4].ReferenceId].Name == TokenizerTools.TokenNames.ObjectDelimiter);
        }

        [TestMethod]
        public void MultipleToken_ExpectManyAnnotations()
        {
            // Arrange simple setup with an empty JSON object
            var tokenizer = TokenizerTools.CreateJsonTokenizer();
            var keyValuePair = "{ \"key1\": 123.123, \"key2\": [ true, false, \"str\" ] }";

            // Act
            var annotations = tokenizer.Tokenize(keyValuePair);

            Assert.IsTrue(annotations.Count == 15);
           
            for (var i = 0; i < annotations.Count; i++)
            {
                var annotation = annotations[i];
                Assert.IsTrue(annotation.Start >= 0, $"Annotation {i} has a negative start index.");
                Assert.IsTrue(annotation.Length > 0, $"Annotation {i} has a no length.");

                if (i > 0)
                {
                    Assert.IsTrue(annotation.Start > annotations[i - 1].Start);
                }
            }
        }

        
        [TestMethod]
        public void AnnotateJsonTest()
        {
            // Arrange
            var styleLookup = TokenizerTools.CreateJsonStyleLookup();
            var tokenizer = TokenizerTools.CreateJsonTokenizer();

            // Act
            var text = File.ReadAllText("assets/example.json");
            var annotations = tokenizer.Tokenize(text);

            Directory.CreateDirectory("output");

            File.WriteAllText("output/json_annotation.html", tokenizer.AnnotateTextUsingHtml(text, annotations, styleLookup));
        }
    }
}
