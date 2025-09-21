#nullable disable

using gg.parse.script.parser;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.tests.examples
{
    [TestClass]
    public class EbnfTokenParserTest
    {
        [TestMethod]
        public void ParseRule_ExpectSucess()
        {
            var parser = new ScriptParser();

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

    }
}

