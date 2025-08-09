using gg.parse.ebnf;
using gg.parse.rulefunctions;

namespace gg.parse.tests.examples
{
    [TestClass]
    public class EbnfParserTest
    {
        /// <summary>
        ///  Spot check to see if all rules are accounted for
        /// </summary>
        [TestMethod]
        public void TokenizerRulesTest()
        {
            var tokenizerSpec = File.ReadAllText("assets/json_tokens.ebnf");
            var grammarSpec = File.ReadAllText("assets/json_grammar.ebnf");

            var jsonParser = new EbnfParser(tokenizerSpec, grammarSpec);

            var tokenizer = jsonParser.EbnfTokenizer;
            var whiteSpaceRule = tokenizer.FindRule("white_space") as MatchDataSet<char>;
            
            Assert.IsTrue(whiteSpaceRule != null);
            Assert.IsTrue(whiteSpaceRule.Production == AnnotationProduct.None);
            Assert.IsTrue(whiteSpaceRule.MatchingValues.Length == 4);
            Assert.IsTrue(whiteSpaceRule.MatchingValues.SequenceEqual(" \t\r\n".ToArray()));
        }

        [TestMethod]
        public void ParseJsonKeyValue_IntegrationTest()
        {
            var tokenizerSpec = File.ReadAllText("assets/json_tokens.ebnf");
            var grammarSpec = File.ReadAllText("assets/json_grammar.ebnf");

            var jsonParser = new EbnfParser(tokenizerSpec, grammarSpec);

            var keyStrValue = "{\"key\": \"value\"}";

            Assert.IsTrue(jsonParser.TryMatch(keyStrValue));

            jsonParser.TryBuildAstTree(keyStrValue, out var tokens, out var astTree);

            // var dump = jsonParser.Dump(keyStrValue, tokens, astTree);    

            Assert.IsTrue(astTree.FoundMatch);
            Assert.IsTrue(astTree.MatchedLength == 5);
            Assert.IsTrue(astTree.Annotations != null);
            Assert.IsTrue(astTree.Annotations.Count == 1);
            Assert.IsTrue(jsonParser.FindParserRule("json") != null);
            Assert.IsTrue(astTree.Annotations[0].FunctionId == jsonParser.FindParserRule("json")!.Id);

            var jsonNode = astTree.Annotations[0];
            var jsonNodeText = EbnfParser.GetText(keyStrValue, jsonNode, tokens);
            Assert.IsTrue(jsonNodeText == keyStrValue);

            Assert.IsTrue(jsonNode.Children != null);
            Assert.IsTrue(jsonNode.Children.Count == 1);
            Assert.IsTrue(jsonParser.FindParserRule("object") != null);
            Assert.IsTrue(jsonNode.Children[0].FunctionId == jsonParser.FindParserRule("object")!.Id);

            var objectNode = jsonNode.Children[0];
            var objectNodeText = EbnfParser.GetText(keyStrValue, objectNode, tokens);

            Assert.IsTrue(objectNodeText == keyStrValue);

            Assert.IsTrue(objectNode.Children != null);
            Assert.IsTrue(objectNode.Children.Count == 3);

            Assert.IsTrue(jsonParser.FindParserRule("scope_start") != null);
            Assert.IsTrue(objectNode.Children[0].FunctionId == jsonParser.FindParserRule("scope_start")!.Id);

            Assert.IsTrue(jsonParser.FindParserRule("scope_end") != null);
            Assert.IsTrue(objectNode.Children[2].FunctionId == jsonParser.FindParserRule("scope_end")!.Id);

            var keyValueListNode = objectNode.Children[1];
            var keyValueListText = EbnfParser.GetText(keyStrValue, keyValueListNode, tokens);

            Assert.IsTrue(keyValueListText == "\"key\": \"value\"");

            Assert.IsTrue(keyValueListNode.Children != null);
            Assert.IsTrue(keyValueListNode.Children.Count == 1);

            // xxx update this
            /*var keyValueListItemNode = keyValueListNode.Children[0];

            Assert.IsTrue(keyValueListItemNode.Children != null);
            Assert.IsTrue(keyValueListItemNode.Children.Count == 3);

            var keyValuePairNode = keyValueListItemNode.Children[0];

            Assert.IsTrue(keyValuePairNode.Children != null);
            Assert.IsTrue(keyValuePairNode.Children.Count == 3);

            var keyNode = keyValuePairNode.Children[0];

            var keyNodeText = EbnfParser.GetText(keyStrValue, keyNode, tokens);

            Assert.IsTrue(keyNodeText == "\"key\"");

            var valueNode = keyValuePairNode.Children[2];

            var valueNodeText = EbnfParser.GetText(keyStrValue, valueNode, tokens);

            Assert.IsTrue(valueNodeText == "\"value\"");

            var keyValuePairListRestNode = keyValueListItemNode.Children[1];

            Assert.IsTrue(keyValuePairListRestNode.Children == null);*/
        }

        
        [TestMethod]
        public void ReadOptimizedEbnfGrammar_IntegrationTest()
        {
            var tokenizerSpec = File.ReadAllText("assets/json_tokens.ebnf");
            var grammarSpec = File.ReadAllText("assets/json_grammar_optimized.ebnf");
            var jsonParser = new EbnfParser(tokenizerSpec, grammarSpec);

            // try parsing an object with two kvp
            var keyStrValue = "{\r\n\"key1\": \"value\", \n \"key2\": 123}";

            Assert.IsTrue(jsonParser.TryMatch(keyStrValue));

            jsonParser.TryBuildAstTree(keyStrValue, out var tokens, out var astTree);

            var dump = jsonParser.Dump(keyStrValue, tokens, astTree);

            // try parsing an array with various values
            var arrayStrValue = "[-123,\"abc\"]";

            Assert.IsTrue(jsonParser.TryMatch(arrayStrValue));

            jsonParser.TryBuildAstTree(arrayStrValue, out tokens, out astTree);

            dump = jsonParser.Dump(arrayStrValue, tokens, astTree);

            // try parsing an empty text
            var emptyText = "";

            Assert.IsTrue(jsonParser.TryMatch(emptyText));
            Assert.IsTrue(jsonParser.TryBuildAstTree(emptyText, out tokens, out astTree));

            dump = jsonParser.Dump(emptyText, tokens, astTree);

            // try an object with all allowed values
            var jsonValuesObject = File.ReadAllText("assets/json_values_object.json");

            Assert.IsTrue(jsonParser.TryMatch(jsonValuesObject));
            Assert.IsTrue(jsonParser.TryBuildAstTree(jsonValuesObject, out tokens, out astTree));

            dump = jsonParser.Dump(jsonValuesObject, tokens, astTree);


            // read a full json file covering all cases
            var jsonFile = File.ReadAllText("assets/example.json");
            
            Assert.IsTrue(jsonParser.TryBuildAstTree(jsonFile, out tokens, out astTree));

            dump = jsonParser.Dump(jsonFile, tokens, astTree);
        }

        [TestMethod]
        public void CreateParserUsingTransitiveRule_ExpectNoExceptions()
        {
            var jsonParser = new EbnfParser("asterix='*';", "#rule=asterix;");

            Assert.IsTrue(jsonParser.TryMatch("*"));

        }
    }
}
