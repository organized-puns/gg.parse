#nullable disable

using gg.parse.json;
using gg.parse.rules;
using gg.parse.script;

namespace gg.parse.tests.examples
{
    [TestClass]
    public class JsonParserTest
    {

        [TestMethod]
        public void CreateParser_ValidateGeneratedTokenizer_ExpectUniqueRules()
        {
            var tokenizerSpec = File.ReadAllText("assets/json_tokens.ebnf");
            var grammarSpec = File.ReadAllText("assets/json_grammar_basic.ebnf");

            var jsonParser = new ParserBuilder()
                            .From(tokenizerSpec, grammarSpec);

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

            var jsonParser = new ParserBuilder().From(tokenizerSpec, grammarSpec);

            var generatedTokenizer = jsonParser.Tokenizer;

            // spot check of some compiled rules

            // check json tokens as defined in the tokenizer spec (ie #json_tokens	= *valid_token;)
            var jsonTokensRule = generatedTokenizer.FindRule("json_tokens") as MatchCount<char>;
            Assert.IsNotNull(jsonTokensRule);
            Assert.IsTrue(jsonTokensRule.Production == IRule.Output.Children);
            Assert.IsTrue(jsonTokensRule.Min == 0);
            Assert.IsTrue(jsonTokensRule.Max == 0);

            var jsonTokensRuleFunction = jsonTokensRule.Rule as RuleReference<char>;

            Assert.IsNotNull(jsonTokensRuleFunction);
            Assert.IsTrue(jsonTokensRuleFunction.Production == IRule.Output.Children);
            Assert.IsTrue(jsonTokensRuleFunction.DeferResultToReference);

            // check valid_token rule, ie #valid_token	= json_token | white_space | unknown_token;
            var validTokenRule = generatedTokenizer.FindRule("valid_token") as MatchOneOf<char>;

            Assert.IsNotNull(validTokenRule);
            Assert.IsTrue(jsonTokensRule.Production == IRule.Output.Children);
            Assert.IsTrue(validTokenRule.RuleOptions.Length == 3);

            Assert.IsTrue(jsonTokensRuleFunction.Rule == validTokenRule);
        }

        [TestMethod]
        public void CreateParser_TestTokenization_ExpectAllInputToHaveTokens()
        {
            var tokenizerSpec = File.ReadAllText("assets/json_tokens.ebnf");
            var grammarSpec = File.ReadAllText("assets/json_grammar_basic.ebnf");

            var jsonParser = new ParserBuilder().From(tokenizerSpec, grammarSpec);

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

            var jsonParser = new ParserBuilder().From(tokenizerSpec, grammarSpec);

            var tokenizer = jsonParser.Tokenizer;
            var whiteSpaceRule = tokenizer.FindRule("white_space") as MatchDataSet<char>;

            Assert.IsTrue(whiteSpaceRule != null);
            Assert.IsTrue(whiteSpaceRule.Production == IRule.Output.Void);
            Assert.IsTrue(whiteSpaceRule.MatchingValues.Length == 4);
            Assert.IsTrue(whiteSpaceRule.MatchingValues.SequenceEqual(" \t\r\n".ToArray()));
        }

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
            var result = parser.Parse(tokens);

        }
    }
}