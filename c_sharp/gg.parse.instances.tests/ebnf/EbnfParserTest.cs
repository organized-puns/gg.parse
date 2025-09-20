#nullable disable

using gg.parse.rulefunctions.datafunctions;
using gg.parse.rulefunctions.rulefunctions;
using gg.parse.script;

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

            var jsonParser = new ScriptParser()
                            .InitializeFromDefinition(tokenizerSpec, grammarSpec);

            var generatedTokenizer = jsonParser.Tokenizer;

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

            var jsonParser = new ScriptParser().InitializeFromDefinition(tokenizerSpec, grammarSpec);

            var generatedTokenizer = jsonParser.Tokenizer;

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

            var jsonParser = new ScriptParser().InitializeFromDefinition(tokenizerSpec, grammarSpec);

            var generatedTokenizer = jsonParser.Tokenizer;
         
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
            var tokenizerSpec = File.ReadAllText("assets/json_tokens.ebnf");
            var grammarSpec = File.ReadAllText("assets/json_grammar_basic.ebnf");

            var jsonParser = new ScriptParser().InitializeFromDefinition(tokenizerSpec, grammarSpec);

            var tokenizer = jsonParser.Tokenizer;
            var whiteSpaceRule = tokenizer.FindRule("white_space") as MatchDataSet<char>;

            Assert.IsTrue(whiteSpaceRule != null);
            Assert.IsTrue(whiteSpaceRule.Production == AnnotationProduct.None);
            Assert.IsTrue(whiteSpaceRule.MatchingValues.Length == 4);
            Assert.IsTrue(whiteSpaceRule.MatchingValues.SequenceEqual(" \t\r\n".ToArray()));
        }

        [TestMethod]
        public void ReadOptimizedEbnfGrammar_IntegrationTest()
        {
            var tokenizerSpec = File.ReadAllText("assets/json_tokens.ebnf");
            var grammarSpec = File.ReadAllText("assets/json_grammar.ebnf");
            var jsonParser = new ScriptParser().InitializeFromDefinition(tokenizerSpec, grammarSpec);

            // try parsing an object with two kvp
            var keyStrValue = "{\r\n\"key1\": \"value\", \n \"key2\": 123}";

            var (tokensResult, astResult) = jsonParser.Parse(keyStrValue);
           
            // try parsing an array with various values
            var arrayStrValue = "[-123,\"abc\"]";

            (tokensResult, astResult) = jsonParser.Parse(arrayStrValue);
            Assert.IsTrue(tokensResult.FoundMatch && astResult.FoundMatch);

            // try parsing an empty text
            var emptyText = "";

            (tokensResult, astResult) = jsonParser.Parse(emptyText);
            Assert.IsTrue(tokensResult.FoundMatch && astResult.FoundMatch);

            // try an object with all allowed values
            var jsonValuesObject = File.ReadAllText("assets/json_values_object.json");

            (tokensResult, astResult) = jsonParser.Parse(jsonValuesObject);
            Assert.IsTrue(tokensResult.FoundMatch && astResult.FoundMatch);

            // read a full json file covering all cases
            var jsonFile = File.ReadAllText("assets/example.json");

            (tokensResult, astResult) = jsonParser.Parse(jsonFile);
            Assert.IsTrue(tokensResult.FoundMatch && astResult.FoundMatch);


        }
    }
}