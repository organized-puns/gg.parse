using gg.parse.basefunctions;
using gg.parse.examples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace gg.parse.tests.examples
{
    [TestClass]
    public class JsonParserTests
    {
        [TestMethod]
        public void ParseSimpleKeyValue_ExpectSuccess()
        {
            var tokenizer = new JsonTokenizer();
            var parser = new JsonParser(tokenizer);

            var keyStrValue = "{\"key\": \"value\"}";
            
            var tokens = tokenizer.Tokenize(keyStrValue).Annotations;
            var result = parser.Parse(tokens);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.MatchedLength == tokens.Count);

            var keyIntValue = "{\"key\": 123}";
            
            tokens = tokenizer.Tokenize(keyIntValue).Annotations;
            result = parser.Parse(tokens);

            Assert.IsTrue(result.IsSuccess);

            var keyValueList = "{\"key1\": 123.12, \"key2\": null}";

            tokens = tokenizer.Tokenize(keyValueList).Annotations;
            result = parser.Parse(tokens);

            Assert.IsTrue(result.IsSuccess);

            var emptyKeyValueList = "{}";

            tokens = tokenizer.Tokenize(emptyKeyValueList).Annotations;
            result = parser.Parse(tokens);

            Assert.IsTrue(result.IsSuccess);

            var compoundList = "{\"root\": {\"key1\": 123.12, \"key2\": null}}";

            tokens = tokenizer.Tokenize(compoundList).Annotations;
            result = parser.Parse(tokens);

            Assert.IsTrue(result.IsSuccess);

            var jsonArray = "[1, 2, \"a\"]";

            tokens = tokenizer.Tokenize(jsonArray).Annotations;
            result = parser.Parse(tokens);

            Assert.IsTrue(result.IsSuccess);

            var emptyArray = "[]";

            tokens = tokenizer.Tokenize(emptyArray).Annotations;
            result = parser.Parse(tokens);

            Assert.IsTrue(result.IsSuccess);


            var compoundArray = "[1, [{\"key\": true}, false] ]";

            tokens = tokenizer.Tokenize(compoundArray).Annotations;
            result = parser.Parse(tokens);

            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void AnnotateSimpleJson_ExpectValidHtml()
        {
            var parser = new JsonParser();
            var keyStrValue = "{\"key\": \"value\"}";
            var (tokens, astNodes) = parser.Parse(keyStrValue);

            var html = parser.AnnotateTextUsingHtml(keyStrValue, tokens, astNodes, parser.CreateAstStyleLookup());

            Directory.CreateDirectory("output");

            File.WriteAllText("output/ast_example_annotation.html", html);
        }

        [TestMethod]
        public void AnnotateJsonFile_ExpectValidHtml()
        {
            var parser = new JsonParser();
            var (tokens, astNodes, text) = parser.ParseFile("assets/example.json");

            var html = parser.AnnotateTextUsingHtml(text, tokens, astNodes, parser.CreateAstStyleLookup());

            Directory.CreateDirectory("output");

            File.WriteAllText("output/astfile_example_annotation.html", html);
        }
    }
}
