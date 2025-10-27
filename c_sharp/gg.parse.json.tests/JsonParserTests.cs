using gg.parse.rules;
using gg.parse.script;

namespace gg.parse.json.tests
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

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.MatchLength == tokens.Count);

            var keyIntValue = "{\"key\": 123}";
            
            tokens = tokenizer.Tokenize(keyIntValue).Annotations;
            result = parser.Parse(tokens);

            Assert.IsTrue(result.FoundMatch);

            var keyValueList = "{\"key1\": 123.12, \"key2\": null}";

            tokens = tokenizer.Tokenize(keyValueList).Annotations;
            result = parser.Parse(tokens);

            Assert.IsTrue(result.FoundMatch);

            var emptyKeyValueList = "{}";

            tokens = tokenizer.Tokenize(emptyKeyValueList).Annotations;
            result = parser.Parse(tokens);

            Assert.IsTrue(result.FoundMatch);

            var compoundList = "{\"root\": {\"key1\": 123.12, \"key2\": null}}";

            tokens = tokenizer.Tokenize(compoundList).Annotations;
            result = parser.Parse(tokens);

            Assert.IsTrue(result.FoundMatch);

            var jsonArray = "[1, 2, \"a\"]";

            tokens = tokenizer.Tokenize(jsonArray).Annotations;
            result = parser.Parse(tokens);

            Assert.IsTrue(result.FoundMatch);

            var emptyArray = "[]";

            tokens = tokenizer.Tokenize(emptyArray).Annotations;
            result = parser.Parse(tokens);

            Assert.IsTrue(result.FoundMatch);


            var compoundArray = "[1, [{\"key\": true}, false] ]";

            tokens = tokenizer.Tokenize(compoundArray).Annotations;
            result = parser.Parse(tokens);

            Assert.IsTrue(result.FoundMatch);
        }

        [TestMethod]
        public void Missing_KeyValue_ExpectRecovery()
        {
            var tokenizer = new JsonTokenizer();
            var parser = new JsonParser(tokenizer);

            var missingValue = "{\"key\": }";
            var tokens = tokenizer.Tokenize(missingValue).Annotations;

#pragma warning disable IDE0059 // Unnecessary assignment of a value
            var result = parser.Parse(tokens);
#pragma warning restore IDE0059 // Unnecessary assignment of a value

            // xxx missing assert

        }

        [TestMethod]
        public void AnnotateSimpleJson_ExpectValidHtml()
        {
            var parser = new JsonParser();
            var keyStrValue = "{\"key\": \"value\"}";
            var (tokens, astNodes) = parser.Parse(keyStrValue);

            var html = JsonParser.AnnotateTextUsingHtml(keyStrValue, tokens, astNodes, JsonParser.CreateAstStyleLookup());

            Directory.CreateDirectory("output");

            File.WriteAllText("output/ast_example_annotation.html", html);
        }

        [TestMethod]
        public void AnnotateJsonFile_ExpectValidHtml()
        {
            var parser = new JsonParser();
            var (tokens, astNodes, text) = parser.ParseFile("assets/example.json");

            var html = JsonParser.AnnotateTextUsingHtml(text, tokens, astNodes, JsonParser.CreateAstStyleLookup());

            Directory.CreateDirectory("output");

            File.WriteAllText("output/astfile_example_annotation.html", html);
        }


        [TestMethod]
        public void CreateParser_TestTokenization_ExpectAllInputToHaveTokens()
        {
            var tokenizerSpec = File.ReadAllText("assets/json.tokens");

            var jsonParser = new ParserBuilder().From(tokenizerSpec);

            var generatedTokenizer = jsonParser.TokenGraph;

            // test the full set of tokens
            var validTokens = "{ } [ ] , : \"key\" 123 123.0 true false null @";

            var tokens = generatedTokenizer.Root.Parse([.. validTokens], 0);

            Assert.IsTrue(tokens.FoundMatch);
            Assert.IsTrue(tokens.Annotations != null);
            Assert.IsTrue(tokens.Annotations.Count == 13);
            Assert.IsTrue(generatedTokenizer.FindRule("scope_start") != null);
            Assert.IsTrue(generatedTokenizer.FindRule("scope_end") != null);
            Assert.IsTrue(generatedTokenizer.FindRule("unknown_token") != null);
            Assert.IsTrue(tokens.Annotations[0].Rule == generatedTokenizer.FindRule("scope_start"));
            Assert.IsTrue(tokens.Annotations[1].Rule == generatedTokenizer.FindRule("scope_end"));
            Assert.IsTrue(tokens.Annotations[12].Rule == generatedTokenizer.FindRule("unknown_token"));
        }


        [TestMethod]
        public void CreateParser_TestWhiteSpaceRule_ExpectToMatchWhiteSpaceChars()
        {
            var tokenizerSpec = File.ReadAllText("assets/json.tokens");
            var grammarSpec = File.ReadAllText("assets/json.grammar");

            var jsonParser = new ParserBuilder().From(tokenizerSpec, grammarSpec);

            var tokenizer = jsonParser.TokenGraph;
            var whiteSpaceRule = tokenizer.FindRule("white_space") as MatchDataSet<char>;

            Assert.IsTrue(whiteSpaceRule != null);
            Assert.IsTrue(whiteSpaceRule.Prune == AnnotationPruning.All);
            Assert.IsTrue(whiteSpaceRule.MatchingValues.Length == 4);
            Assert.IsTrue(whiteSpaceRule.MatchingValues.SequenceEqual(" \t\r\n".ToArray()));
        }
    }
}
