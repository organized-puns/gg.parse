using gg.parse.ebnf;
using gg.parse.rulefunctions.datafunctions;
using gg.parse.rulefunctions.rulefunctions;

namespace gg.parse.tests.examples
{
    [TestClass]
    public class EbnfParserTest
    {

        [TestMethod]
        public void CreateParser_ValidateGeneratedTokenizer_ExpectUniqueRules()
        {
            var tokenizerSpec = File.ReadAllText("assets/json_tokens.ebnf");
            var grammarSpec = File.ReadAllText("assets/json_grammar_basic.ebnf");

            var jsonParser = new ScriptPipeline(tokenizerSpec, grammarSpec);

            var generatedTokenizer = jsonParser.EbnfTokenizer;

            // basic checks
            Assert.IsTrue(generatedTokenizer != null);
            Assert.IsTrue(generatedTokenizer.Root != null);

            // at least one rule created ?
            Assert.IsTrue(generatedTokenizer.Count() > 0);

            // check if all created rules have a unique id and name
            Assert.IsTrue(generatedTokenizer.All(r => r.Id >= 0));
            var uniqueIds = new HashSet<int>(generatedTokenizer.Select(r => r.Id));

            Assert.IsTrue(uniqueIds.Count() == generatedTokenizer.Count());

            var uniqueNames = new HashSet<string>(generatedTokenizer.Select(r => r.Name));

            Assert.IsTrue(uniqueNames.Count() == generatedTokenizer.Count());
        }

        [TestMethod]
        public void CreateParser_FindSpecificRules_RulesToMatchExpectations()
        {
            var tokenizerSpec = File.ReadAllText("assets/json_tokens.ebnf");
            var grammarSpec = File.ReadAllText("assets/json_grammar_basic.ebnf");

            var jsonParser = new ScriptPipeline(tokenizerSpec, grammarSpec);

            var generatedTokenizer = jsonParser.EbnfTokenizer;

            // spot check of some compiled rules

            // check json tokens as defined in the tokenizer spec (ie #json_tokens	= *valid_token;)
            var jsonTokensRule = generatedTokenizer.FindRule("json_tokens") as MatchFunctionCount<char>;
            Assert.IsNotNull(jsonTokensRule);
            Assert.IsTrue(jsonTokensRule.Production == AnnotationProduct.Transitive);
            Assert.IsTrue(jsonTokensRule.Min == 0);
            Assert.IsTrue(jsonTokensRule.Max == 0);

            var jsonTokensRuleFunction = jsonTokensRule.Function as RuleReference<char>;

            Assert.IsNotNull(jsonTokensRuleFunction);
            Assert.IsTrue(jsonTokensRuleFunction.Production == AnnotationProduct.Transitive);
            Assert.IsTrue(jsonTokensRuleFunction.IsPartOfComposition);

            // check valid_token rule, ie #valid_token	= json_token | white_space | unknown_token;
            var validTokenRule = generatedTokenizer.FindRule("valid_token") as MatchOneOfFunction<char>;

            Assert.IsNotNull(validTokenRule);
            Assert.IsTrue(jsonTokensRule.Production == AnnotationProduct.Transitive);
            Assert.IsTrue(validTokenRule.RuleOptions.Length == 3);

            Assert.IsTrue(jsonTokensRuleFunction.Rule == validTokenRule);
        }

        [TestMethod]
        public void CreateParser_TestTokenization_ExpectAllInputToHaveTokens()
        {
            var tokenizerSpec = File.ReadAllText("assets/json_tokens.ebnf");
            var grammarSpec = File.ReadAllText("assets/json_grammar_basic.ebnf");

            var jsonParser = new ScriptPipeline(tokenizerSpec, grammarSpec);

            var generatedTokenizer = jsonParser.EbnfTokenizer;
         
            // test the full set of tokens
            var validTokens = "{ } [ ] , : \"key\" 123 123.0 true false null @";

            var tokens = generatedTokenizer.Root.Parse([.. validTokens], 0);

            Assert.IsTrue(tokens.FoundMatch);
            Assert.IsTrue(tokens.Annotations != null);
            Assert.IsTrue(tokens.Annotations.Count == 13);
            Assert.IsTrue(generatedTokenizer.FindRule("scope_start") != null);
            Assert.IsTrue(generatedTokenizer.FindRule("scope_end") != null);
            Assert.IsTrue(generatedTokenizer.FindRule("unknown_token") != null);
            Assert.IsTrue(tokens.Annotations[0].RuleId == generatedTokenizer.FindRule("scope_start")!.Id);
            Assert.IsTrue(tokens.Annotations[1].RuleId == generatedTokenizer.FindRule("scope_end")!.Id);
            Assert.IsTrue(tokens.Annotations[12].RuleId == generatedTokenizer.FindRule("unknown_token")!.Id);
        }

        
        [TestMethod]
        public void CreateParser_TestWhiteSpaceRule_ExpectToMatchWhiteSpaceChars()
        {
            var tokenizerSpec = File.ReadAllText("assets/json_tokens.ebnf");
            var grammarSpec = File.ReadAllText("assets/json_grammar_basic.ebnf");

            var jsonParser = new ScriptPipeline(tokenizerSpec, grammarSpec);

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
            var grammarSpec = File.ReadAllText("assets/json_grammar_basic.ebnf");

            var jsonParser = new ScriptPipeline(tokenizerSpec, grammarSpec);

            var keyStrValue = "{\"key\": \"value\"}";

            Assert.IsTrue(jsonParser.TryMatch(keyStrValue));

            jsonParser.TryBuildAstTree(keyStrValue, out var tokens, out var astTree);

            var dump = jsonParser.Dump(keyStrValue, tokens, astTree);

            Assert.IsTrue(astTree.FoundMatch);
            Assert.IsTrue(astTree.MatchedLength == 5);
            Assert.IsTrue(astTree.Annotations != null);
            Assert.IsTrue(astTree.Annotations.Count == 1);
            Assert.IsTrue(jsonParser.FindParserRule("json") != null);
            Assert.IsTrue(astTree.Annotations[0].RuleId == jsonParser.FindParserRule("json")!.Id);

            var jsonNode = astTree.Annotations[0];
            var jsonNodeText = jsonNode.GetText(keyStrValue, tokens.Annotations!);
            Assert.IsTrue(jsonNodeText == keyStrValue);

            Assert.IsTrue(jsonNode.Children != null);
            Assert.IsTrue(jsonNode.Children.Count == 1);
            Assert.IsTrue(jsonParser.FindParserRule("object") != null);
            Assert.IsTrue(jsonNode.Children[0].RuleId == jsonParser.FindParserRule("object")!.Id);

            var objectNode = jsonNode.Children[0];
            var objectNodeText = objectNode.GetText(keyStrValue, tokens.Annotations!);

            Assert.IsTrue(objectNodeText == keyStrValue);

            Assert.IsTrue(objectNode.Children != null);
            Assert.IsTrue(objectNode.Children.Count == 3);

            Assert.IsTrue(jsonParser.FindParserRule("scope_start") != null);
            Assert.IsTrue(objectNode.Children[0].RuleId == jsonParser.FindParserRule("scope_start")!.Id);

            Assert.IsTrue(jsonParser.FindParserRule("scope_end") != null);
            Assert.IsTrue(objectNode.Children[2].RuleId == jsonParser.FindParserRule("scope_end")!.Id);

            var keyValueListNode = objectNode.Children[1];
            var keyValueListText = keyValueListNode.GetText(keyStrValue, tokens.Annotations!);

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
            var grammarSpec = File.ReadAllText("assets/json_grammar.ebnf");
            var jsonParser = new ScriptPipeline(tokenizerSpec, grammarSpec);

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
            var jsonParser = new ScriptPipeline("asterix='*';", "#rule=asterix;");

            Assert.IsTrue(jsonParser.TryMatch("*"));
        }
    }
}