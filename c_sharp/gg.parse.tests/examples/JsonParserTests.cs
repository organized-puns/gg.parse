using gg.parse.examples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        }
    }
}
