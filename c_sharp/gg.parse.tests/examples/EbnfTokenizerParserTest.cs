using gg.parse.examples;
using gg.parse.rulefunctions;

namespace gg.parse.tests.examples
{
    [TestClass]
    public class EbnfTokenizerParserTest
    {
        [TestMethod]
        public void ParseRule_ExpectSucess()
        {
            var parser = new EbnfTokenizerParser();

            // try parsing a literal
            var (tokens, nodes) = parser.Parse("rule = 'foo';");

            Assert.IsTrue(tokens != null && tokens.Count > 0);
            Assert.IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children.Count == 2);

            var name = parser.FindRule(nodes[0].Children[0].FunctionId).Name;
            Assert.IsTrue(name == "RuleName");
            name = parser.FindRule(nodes[0].Children[1].FunctionId).Name;
            Assert.IsTrue(name == "Literal");

            // try parsing a set
            (tokens, nodes) = parser.Parse("rule = { \"abc\" };");

            Assert.IsTrue(tokens != null && tokens.Count > 0);
            Assert.IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children.Count == 2);

            // try parsing a character range
            (tokens, nodes) = parser.Parse("rule = { 'a' .. 'z' };");

            Assert.IsTrue(tokens != null && tokens.Count > 0);
            Assert.IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children.Count == 2);
            name = parser.FindRule(nodes[0].Children[1].FunctionId).Name;
            Assert.IsTrue(name == "CharacterRange");

            // try parsing a sequence
            (tokens, nodes) = parser.Parse("rule = \"abc\", 'def', { '123' };");

            Assert.IsTrue(tokens != null && tokens.Count > 0);
            Assert.IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children.Count == 2);

            name = parser.FindRule(nodes[0].Children[1].FunctionId).Name;
            Assert.IsTrue(name == "Sequence");

            // try parsing an option
            (tokens, nodes) = parser.Parse("rule = \"abc\"|'def' | { '123' };");

            Assert.IsTrue(tokens != null && tokens.Count > 0);
            Assert.IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children.Count == 2);
            name = parser.FindRule(nodes[0].Children[1].FunctionId).Name;
            Assert.IsTrue(name == "Option");

            // try parsing a group  
            (tokens, nodes) = parser.Parse("rule = ('123', {'foo'});");

            Assert.IsTrue(tokens != null && tokens.Count > 0);
            Assert.IsTrue(nodes != null && nodes.Count == 1 && nodes[0].Children.Count == 2);

            name = parser.FindRule(nodes[0].Children[1].FunctionId).Name;
            Assert.IsTrue(name == "Sequence");

            name = parser.FindRule(nodes[0].Children[1].Children[0].FunctionId).Name;
            Assert.IsTrue(name == "Literal");

            name = parser.FindRule(nodes[0].Children[1].Children[1].FunctionId).Name;
            Assert.IsTrue(name == "CharacterSet");

            // try parsing zero or more
            (tokens, nodes) = parser.Parse("rule = *('123'|{'foo'});");

            name = parser.FindRule(nodes[0].Children[1].FunctionId).Name;
            Assert.IsTrue(name == "ZeroOrMore");

            name = parser.FindRule(nodes[0].Children[1].Children[0].FunctionId).Name;
            Assert.IsTrue(name == "Option");

            // try parsing a transitive rule
            (tokens, nodes) = parser.Parse("#rule = !('123',{'foo'});");

            name = parser.FindRule(nodes[0].Children[0].FunctionId).Name;

            name = parser.FindRule(nodes[0].Children[0].FunctionId).Name;
            Assert.IsTrue(name == "TransitiveSelector");

            name = parser.FindRule(nodes[0].Children[1].FunctionId).Name;
            Assert.IsTrue(name == "RuleName");

            name = parser.FindRule(nodes[0].Children[2].FunctionId).Name;
            Assert.IsTrue(name == "Not");

            // try parsing a no production rule
            (tokens, nodes) = parser.Parse("~rule = ?('123',{'foo'});");

            name = parser.FindRule(nodes[0].Children[0].FunctionId).Name;
            Assert.IsTrue(name == "NoProductSelector");

            name = parser.FindRule(nodes[0].Children[1].FunctionId).Name;
            Assert.IsTrue(name == "RuleName");

            name = parser.FindRule(nodes[0].Children[2].FunctionId).Name;
            Assert.IsTrue(name == "ZeroOrOne");

            // try parsing an identifier
            (tokens, nodes) = parser.Parse("rule = +(one, two, three);");

            var node = nodes[0].Children[1];
            name = parser.FindRule(node.FunctionId).Name;
            Assert.IsTrue(name == "OneOrMore");

            node = node.Children[0];
            name = parser.FindRule(node.FunctionId).Name;
            Assert.IsTrue(name == "Sequence");

            node = node.Children[0];
            name = parser.FindRule(node.FunctionId).Name;
            Assert.IsTrue(name == "Identifier");
        }

        [TestMethod]
        public void CompileRule_ExpectSuccess()
        {
            // try compiling a literal rule
            var parser = new EbnfTokenizerParser();
            var table = parser.Compile("lit_rule = 'foo';");
            var litRule = table.FindRule("lit_rule") as MatchDataSequence<char>;
            
            Assert.IsNotNull(litRule);
            Assert.IsTrue(litRule.Production == AnnotationProduct.Annotation);
            Assert.IsTrue(litRule.DataArray.SequenceEqual("foo".ToArray()));

            // try compiling a set rule
            table = parser.Compile("~set_rule = {'abc'};");
            var setRule= table.FindRule("set_rule") as MatchDataSet<char>;

            Assert.IsNotNull(setRule);
            Assert.IsTrue(setRule.Production == AnnotationProduct.None);
            Assert.IsTrue(setRule.MatchingValues.SequenceEqual("abc".ToArray()));

            // try compiling a range rule
            table = parser.Compile("range_rule = {'a' .. 'z'};");
            var rangeRule = table.FindRule("range_rule") as MatchDataRange<char>;

            Assert.IsNotNull(rangeRule);
            Assert.IsTrue(rangeRule.MinDataValue == 'a');
            Assert.IsTrue(rangeRule.MaxDataValue == 'z');

            // try compiling a transitive sequence rule
            table = parser.Compile("#sequence_rule = 'foo', 'bar';");
            var sequenceRule = table.FindRule("sequence_rule") as MatchFunctionSequence<char>;

            Assert.IsNotNull(sequenceRule);
            Assert.IsTrue(sequenceRule.Production == AnnotationProduct.Transitive);
            
            var fooLit = sequenceRule.Sequence[0] as MatchDataSequence<char>;
            Assert.IsNotNull(fooLit);
            Assert.IsTrue(fooLit.DataArray.SequenceEqual("foo".ToArray()));

            var barLit = sequenceRule.Sequence[1] as MatchDataSequence<char>;
            Assert.IsNotNull(barLit);
            Assert.IsTrue(barLit.DataArray.SequenceEqual("bar".ToArray()));

            // try compiling a option rule
            table = parser.Compile("option_rule = 'foo' | 'bar';");
            var optionRule = table.FindRule("option_rule") as MatchOneOfFunction<char>;

            Assert.IsNotNull(optionRule);
            Assert.IsTrue(optionRule.Production == AnnotationProduct.Annotation);

            fooLit = optionRule.Options[0] as MatchDataSequence<char>;
            Assert.IsNotNull(fooLit);
            Assert.IsTrue(fooLit.DataArray.SequenceEqual("foo".ToArray()));

            barLit = optionRule.Options[1] as MatchDataSequence<char>;
            Assert.IsNotNull(barLit);
            Assert.IsTrue(barLit.DataArray.SequenceEqual("bar".ToArray()));

            // try compiling a group rule
            table = parser.Compile("group_rule = ('foo', 'bar') | 'baz';");
            var groupRule = table.FindRule("group_rule") as MatchOneOfFunction<char>;

            Assert.IsNotNull(groupRule);
            sequenceRule = groupRule.Options[0] as MatchFunctionSequence<char>;
            litRule = groupRule.Options[1] as MatchDataSequence<char>;

            Assert.IsNotNull(sequenceRule);
            Assert.IsNotNull(litRule);
            Assert.IsTrue(litRule.DataArray.SequenceEqual("baz".ToArray()));

            fooLit = sequenceRule.Sequence[0] as MatchDataSequence<char>;
            Assert.IsNotNull(fooLit);
            Assert.IsTrue(fooLit.DataArray.SequenceEqual("foo".ToArray()));

            barLit = sequenceRule.Sequence[1] as MatchDataSequence<char>;
            Assert.IsNotNull(barLit);
            Assert.IsTrue(barLit.DataArray.SequenceEqual("bar".ToArray()));

            // try compiling (and resolving an identifier/reference)
            table = parser.Compile("sequence_rule = foo, bar; foo = 'foo'; bar = 'bar';");
            sequenceRule = table.FindRule("sequence_rule") as MatchFunctionSequence<char>;
            Assert.IsNotNull(sequenceRule);
            
            fooLit = sequenceRule.Sequence[0] as MatchDataSequence<char>;
            Assert.IsNotNull(fooLit);
            Assert.IsTrue(fooLit.DataArray.SequenceEqual("foo".ToArray()));
            Assert.IsTrue(table.FindRule("foo") == fooLit);

            barLit = sequenceRule.Sequence[1] as MatchDataSequence<char>;
            Assert.IsNotNull(barLit);
            Assert.IsTrue(barLit.DataArray.SequenceEqual("bar".ToArray()));
            Assert.IsTrue(table.FindRule("bar") == barLit);

            // try compiling a zero or more rule
            table = parser.Compile("zero_or_more_rule = *'bar';");
            var zeroOrMore = table.FindRule("zero_or_more_rule") as MatchFunctionCount<char>;
            Assert.IsNotNull(zeroOrMore);
            Assert.IsTrue(zeroOrMore.Min == 0);
            Assert.IsTrue(zeroOrMore.Max == 0);
            Assert.IsTrue(zeroOrMore.Function == table.FindRule("zero_or_more_rule(function)"));

        }
    }
}
