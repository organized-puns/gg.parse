#nullable disable

using gg.parse.rulefunctions;
using gg.parse.rulefunctions.datafunctions;
using gg.parse.rulefunctions.rulefunctions;
using gg.parse.script.parsing;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.script.tests.unit
{
    [TestClass]
    public class EbnfTokenParserTest
    {
        [TestMethod]
        public void ParseRule_ExpectSucess()
        {
            var parser = new EbnfTokenParser();

            // try parsing a literal
            var (tokens, nodes) = parser.Parse("rule = 'foo';");

            IsTrue(tokens != null && tokens.Count > 0);
            IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children!.Count == 2);

            var name = nodes[0].Children![0].Rule!.Name;
            IsTrue(name == "RuleName");
            name = nodes[0].Children[1].Rule!.Name;
            IsTrue(name == "Literal");

            // try parsing a set
            (tokens, nodes) = parser.Parse("rule = { \"abc\" };");

            IsTrue(tokens != null && tokens.Count > 0);
            IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children.Count == 2);

            // try parsing a character range
            (tokens, nodes) = parser.Parse("rule = { 'a' .. 'z' };");

            IsTrue(tokens != null && tokens.Count > 0);
            IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children.Count == 2);
            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == "CharacterRange");

            // try parsing a sequence
            (tokens, nodes) = parser.Parse("rule = \"abc\", 'def', { '123' };");

            IsTrue(tokens != null && tokens.Count > 0);
            IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children.Count == 2);

            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == "Sequence");

            // try parsing an option
            (tokens, nodes) = parser.Parse("rule = \"abc\"|'def' | { '123' };");

            IsTrue(tokens != null && tokens.Count > 0);
            IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children.Count == 2);
            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == "Option");

            // try parsing a group  
            (tokens, nodes) = parser.Parse("rule = ('123', {'foo'});");

            IsTrue(tokens != null && tokens.Count > 0);
            IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children.Count == 2);

            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == "Sequence");

            name = nodes[0].Children[1].Children[0].Rule.Name;
            IsTrue(name == "Literal");

            name = nodes[0].Children[1].Children[1].Rule.Name;
            IsTrue(name == "CharacterSet");

            // try parsing zero or more
            (tokens, nodes) = parser.Parse("rule = *('123'|{'foo'});");

            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == "ZeroOrMore");

            name = nodes[0].Children[1].Children[0].Rule.Name;
            IsTrue(name == "Option");

            // try parsing a transitive rule
            (tokens, nodes) = parser.Parse("#rule = !('123',{'foo'});");

            name = nodes[0].Children[0].Rule.Name;

            name = nodes[0].Children[0].Rule.Name;
            IsTrue(name == "TransitiveSelector");

            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == "RuleName");

            name = nodes[0].Children[2].Rule.Name;
            IsTrue(name == "Not");

            // try parsing a no production rule
            (tokens, nodes) = parser.Parse("~rule = ?('123',{'foo'});");

            name = nodes[0].Children[0].Rule.Name;
            IsTrue(name == "NoProductSelector");

            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == "RuleName");

            name = nodes[0].Children[2].Rule.Name;
            IsTrue(name == "ZeroOrOne");

            // try parsing an identifier
            (tokens, nodes) = parser.Parse("rule = +(one, two, three);");

            var node = nodes[0].Children[1];
            name = node.Rule.Name;
            IsTrue(name == "OneOrMore");

            node = node.Children[0];
            name = node.Rule.Name;
            IsTrue(name == "Sequence");

            node = node.Children[0];
            name = node.Rule.Name;
            IsTrue(name == "Identifier");

            
            // try parsing a try match 
            (tokens, nodes) = parser.Parse("rule = try \"lit\";");

            IsTrue(nodes != null);
            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == "TryMatch");

            // try parsing a try match with eoln
            (tokens, nodes) = parser.Parse("rule = try\n\"lit\";");

            IsTrue(nodes != null);
            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == "TryMatch");

            // try parsing a try match with out space, should result in an unknown error
            try
            {
                (tokens, nodes) = parser.Parse("rule = tryy \"lit\";");
                Fail();
            }
            catch (ParseException)
            {
            }

            // try parsing a try match shorthand
            (tokens, nodes) = parser.Parse("rule = >\"lit\";");

            IsTrue(nodes != null);
            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == "TryMatch");
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

