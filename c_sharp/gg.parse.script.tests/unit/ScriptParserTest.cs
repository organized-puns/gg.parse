using gg.parse.rules;
using gg.parse.script.common;
using gg.parse.script.parser;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.script.tests.unit
{
    [TestClass]
    public class ScriptParserTest
    {
        [TestMethod]
        public void CreateSkipScript_Parse_ExpectSkipNodes()
        {
            var parser = new ScriptParser();

            var (tokens, nodes) = parser.Parse("rule = >>> 'foo';");

            IsTrue(tokens != null && tokens.Count > 0);
            IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children!.Count == 2);

            var ruleNameRule = nodes[0][0].Rule;
            IsTrue(ruleNameRule == parser.MatchRuleName);

            var skipRule = nodes[0][1].Rule;
            IsTrue(skipRule == parser.MatchSkipOperator);

            var fooLiteral = nodes[0][1][0].Rule;
            IsTrue(fooLiteral.Name == ScriptParser.Names.Literal);
        }

        [TestMethod]
        public void CreateFindScript_Parse_ExpectFindNodes()
        {
            var parser = new ScriptParser();

            var (tokens, nodes) = parser.Parse("rule = >> 'foo';");

            IsTrue(tokens != null && tokens.Count > 0);
            IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children!.Count == 2);

            var ruleNameRule = nodes[0][0].Rule;
            IsTrue(ruleNameRule == parser.MatchRuleName);

            var skipRule = nodes[0][1].Rule;
            IsTrue(skipRule == parser.MatchFindOperator);

            var fooLiteral = nodes[0][1][0].Rule;
            IsTrue(fooLiteral.Name == ScriptParser.Names.Literal);
        }

        [TestMethod]
        public void CreateRuleAsOptionScript_Parse_ExpectToFindRuleNameAndOption()
        {
            var parser = new ScriptParser();

            var (tokens, nodes) = parser.Parse("rule = 'foo' | 'bar';");

            IsTrue(tokens != null && tokens.Count > 0);
            IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children!.Count == 2);

            var ruleNameRule = nodes[0][0].Rule;
            IsTrue(ruleNameRule == parser.MatchRuleName);

            var optionRule = nodes[0][1].Rule;
            IsTrue(optionRule.Name == parser.MatchOption.Name);
            IsTrue(optionRule.GetType() == typeof(MatchRuleSequence<int>));
            
            var option1 = nodes[0][1][0].Rule;
            IsTrue(option1.Name == parser.MatchLiteral.Name);

            var option2 = nodes[0][1][1].Rule;
            IsTrue(option2.Name == parser.MatchLiteral.Name);
        }

        [TestMethod]
        public void DefineMatchCharacterSet_Parse_ExpectValidTokens()
        {
            var parser = new ScriptParser();
            var rule = "rule_name = {\"_-~()[]{}+=@!#$%&'`\"};";

            var (_, syntaxTree) = parser.Parse(rule);

            IsTrue(syntaxTree != null);

            var optionRule = syntaxTree[0][1].Rule;
            IsTrue(optionRule.Name == parser.MatchCharacterSet.Name);
        }

        [TestMethod]
        public void DefineMatchCharacterSet_Tokenize_ExpectValidTokens()
        {
            var tokenizer = new ScriptTokenizer();
            var rule = "rule_name = {\"_-~()[]{}+=@!#$%&'`\"};";

            var (isSuccess, charactersRead, annotations) = tokenizer.Tokenize(rule);

            IsTrue(isSuccess);
            IsTrue(charactersRead == rule.Length);
            IsTrue(annotations!.Count == 6);
            IsTrue(annotations[0].Rule == tokenizer.FindRule(CommonTokenNames.Identifier));
            IsTrue(annotations[1].Rule == tokenizer.FindRule(CommonTokenNames.Assignment));
            IsTrue(annotations[2].Rule == tokenizer.FindRule(CommonTokenNames.ScopeStart));
            IsTrue(annotations[3].Rule == tokenizer.FindRule(CommonTokenNames.DoubleQuotedString));
            IsTrue(annotations[4].Rule == tokenizer.FindRule(CommonTokenNames.ScopeEnd));
            IsTrue(annotations[5].Rule == tokenizer.FindRule(CommonTokenNames.EndStatement));
        }



        [TestMethod]
        public void ParseRule_ExpectSuccess()
        {
            var parser = new ScriptParser();

            // try parsing a literal
            var (tokens, nodes) = parser.Parse("rule = 'foo';");

            IsTrue(tokens != null && tokens.Count > 0);
            IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children!.Count == 2);

            var name = nodes[0].Children![0].Rule!.Name;
            IsTrue(name == "ruleName");
            name = nodes[0].Children[1].Rule!.Name;
            IsTrue(name == ScriptParser.Names.Literal);

            // try parsing a set
            (tokens, nodes) = parser.Parse("rule = { \"abc\" };");

            IsTrue(tokens != null && tokens.Count > 0);
            IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children.Count == 2);

            // try parsing a character range
            (tokens, nodes) = parser.Parse("rule = { 'a' .. 'z' };");

            IsTrue(tokens != null && tokens.Count > 0);
            IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children.Count == 2);
            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == ScriptParser.Names.CharacterRange);

            // try parsing a sequence
            (tokens, nodes) = parser.Parse("rule = \"abc\", 'def', { '123' };");

            IsTrue(tokens != null && tokens.Count > 0);
            IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children.Count == 2);

            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == ScriptParser.Names.Sequence);

            // try parsing an option
            (tokens, nodes) = parser.Parse("rule = \"abc\"|'def' | { '123' };");

            IsTrue(tokens != null && tokens.Count > 0);
            IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children.Count == 2);
            
            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == ScriptParser.Names.Option);

            // try parsing a group  
            (tokens, nodes) = parser.Parse("rule = ('123', {'foo'});");

            IsTrue(tokens != null && tokens.Count > 0);
            IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children.Count == 2);

            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == ScriptParser.Names.Sequence);

            name = nodes[0].Children[1].Children[0].Rule.Name;
            IsTrue(name == ScriptParser.Names.Literal);

            name = nodes[0].Children[1].Children[1].Rule.Name;
            IsTrue(name == ScriptParser.Names.CharacterSet);

            // try parsing zero or more
            (tokens, nodes) = parser.Parse("rule = *('123'|{'foo'});");

            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == ScriptParser.Names.ZeroOrMore);

            name = nodes[0].Children[1].Children[0].Rule.Name;
            IsTrue(name == ScriptParser.Names.Option);

            // try parsing a transitive rule
            (tokens, nodes) = parser.Parse("#rule = !('123',{'foo'});");

            name = nodes[0].Children[0].Rule.Name;

            name = nodes[0].Children[0].Rule.Name;
            IsTrue(name == "TransitiveSelector");

            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == "ruleName");

            name = nodes[0].Children[2].Rule.Name;
            IsTrue(name == ScriptParser.Names.Not);

            // try parsing a no output rule
            (tokens, nodes) = parser.Parse("~rule = ?('123',{'foo'});");

            name = nodes[0].Children[0].Rule.Name;
            IsTrue(name == "NoProductSelector");

            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == "ruleName");

            name = nodes[0].Children[2].Rule.Name;
            IsTrue(name == ScriptParser.Names.ZeroOrOne);

            // try parsing an identifier
            (tokens, nodes) = parser.Parse("rule = +(one, two, three);");

            var node = nodes[0].Children[1];
            name = node.Rule.Name;
            IsTrue(name == ScriptParser.Names.OneOrMore);

            node = node.Children[0];
            name = node.Rule.Name;
            IsTrue(name == ScriptParser.Names.Sequence);

            node = node.Children[0];
            name = node.Rule.Name;
            IsTrue(name == ScriptParser.Names.Reference);

            
            // try parsing a try match 
            (tokens, nodes) = parser.Parse("rule = if \"lit\";");

            IsTrue(nodes != null);
            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == ScriptParser.Names.If);

            // try parsing a try match with eoln
            (tokens, nodes) = parser.Parse("rule = if\n\"lit\";");

            IsTrue(nodes != null);
            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == ScriptParser.Names.If);

            // try parsing a try match with out space, should result in an unknown error
            try
            {
                (tokens, nodes) = parser.Parse("rule = iff \"lit\";");
                Fail();
            }
            catch (ScriptException)
            {
            }
        }

        /*
            xxx move to json example

        [TestMethod]
        public void TokenizeParseCompileFile_ExpectSuccess()
        {
            var (text,tokens,astNodes,table) = SetupTokenizeParseCompile(File.ReadAllText("assets/json_tokens.ebnf"));

            // write the tokens to insepct (debug)
            Directory.CreateDirectory("output");
            File.WriteAllText("output/tpc_json_tokenizer_tokens.html",
                new EbnfTokenizer().AnnotateTextUsingHtml(text, tokens, AnnotationMarkup.CreateTokenStyleLookup()));

            IsTrue(table.Root != null);
            IsTrue(table.Root.Name == "json_tokens");

            var stringRule = table.FindRule("string");

            IsNotNull(stringRule);

            // test single quote
            var result = stringRule.Parse("'foo'".ToArray(), 0);

            IsTrue(result.FoundMatch);
            IsTrue(result.Annotations[0].Range.Start == 0);
            IsTrue(result.Annotations[0].Range.Length == "'foo'".Length);

            // test double quote
            result = stringRule.Parse("\"foo\"".ToArray(), 0);

            IsTrue(result.FoundMatch);
            IsTrue(result.Annotations[0].Range.Start == 0);
            IsTrue(result.Annotations[0].Range.Length == "\"foo\"".Length);

            // test if whitespace is there
            var whiteSpace = table.FindRule("white_space");
            IsNotNull(whiteSpace);
            IsTrue(whiteSpace.Production == AnnotationProduct.None);

            // test parsing
            result = table.Root.Parse("{\"key\": 123, \"key\": null }".ToArray(), 0);
            IsTrue(result.FoundMatch);
            var expectedTokens = new[] {
                "scope_start", "string", "kv_separator", "int", "item_separator",
                "string", "kv_separator", "null", "scope_end" };

            IsTrue(result.Annotations.Count == expectedTokens.Length);

            for (var i = 0; i < expectedTokens.Length; i++)
            {
                IsTrue(result.Annotations[i].Rule == table.FindRule(expectedTokens[i]));
            }
        }


        [TestMethod]
        public void TestEbnfSpecificationError_Handling()
        {
            var (text, tokens, astNodes, table) = SetupTokenizeParseCompile(File.ReadAllText("assets/json_tokens.ebnf"));

            var result = table.Root.Parse("{\"key\": <bunch of errors> 123 }".ToArray(), 0);
            IsTrue(result.FoundMatch);
            var expectedTokens = new[] {
                "scope_start", "string", "kv_separator", "unknown_token", "int", "scope_end" };
            IsTrue(result.Annotations.Count == expectedTokens.Length);

            for (var i = 0; i < expectedTokens.Length; i++)
            {
                IsTrue(result.Annotations[i].Rule == table.FindRule(expectedTokens[i]));
            }
        }*/
    }
}

