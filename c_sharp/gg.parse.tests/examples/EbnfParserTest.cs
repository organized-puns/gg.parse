using gg.parse.ebnf;

namespace gg.parse.tests.examples
{
    [TestClass]
    public class EbnfParserTest
    {
        [TestMethod]
        public void ParseJsonKeyValue_IntegrationTest()
        {
            var tokenizerSpec = File.ReadAllText("assets/json_tokens.ebnf");
            var grammarSpec = File.ReadAllText("assets/json_grammar.ebnf");

            var jsonParser = new EbnfParser(tokenizerSpec, grammarSpec);

            var keyStrValue = "{\"key\": \"value\"}";

            Assert.IsTrue(jsonParser.TryMatch(keyStrValue));

            jsonParser.TryBuildAstTree(keyStrValue, out var tokens, out var astTree);

            var dump = jsonParser.Dump(keyStrValue, tokens, astTree);    

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

            var keyValueListItemNode = keyValueListNode.Children[0];

            Assert.IsTrue(keyValueListItemNode.Children != null);
            Assert.IsTrue(keyValueListItemNode.Children.Count == 2);

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

            Assert.IsTrue(keyValuePairListRestNode.Children == null);           
        }
    }
}
