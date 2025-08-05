using gg.parse.examples;
using gg.parse.rulefunctions;
using static System.Net.Mime.MediaTypeNames;

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

            // try parsing a simple error
            (tokens, nodes) = parser.Parse("rule = error \"message\" skip_until;");

            node = nodes[0].Children[0];
            name = parser.FindRule(node.FunctionId).Name;
            Assert.IsTrue(name == "RuleName");

            node = nodes[0].Children[1];
            name = parser.FindRule(node.FunctionId).Name;
            Assert.IsTrue(name == "Error");

            node = nodes[0].Children[1].Children[0];
            name = parser.FindRule(node.FunctionId).Name;
            Assert.IsTrue(name == "ErrorKeyword");

            node = nodes[0].Children[1].Children[1];
            name = parser.FindRule(node.FunctionId).Name;
            Assert.IsTrue(name == "Literal");

            node = nodes[0].Children[1].Children[2];
            name = parser.FindRule(node.FunctionId).Name;
            Assert.IsTrue(name == "Identifier");

            // try parsing an error with a complex skip rule
            (tokens, nodes) = parser.Parse("rule = error \"message\" ( foo | \"lit\");");

            node = nodes[0].Children[1].Children[2];
            name = parser.FindRule(node.FunctionId).Name;
            Assert.IsTrue(name == "Option");
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
            var setRule = table.FindRule("set_rule") as MatchDataSet<char>;

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

            // try compiling an error
            table = parser.Compile("error_rule = error 'msg' !'bar';");
            var error = table.FindRule("error_rule") as MarkError<char>;
            Assert.IsNotNull(error);
            Assert.IsTrue(error.Message == "msg");
            Assert.IsTrue(error.TestFunction == table.FindRule("error_rule(skip)"));
            Assert.IsTrue(error.TestFunction is MatchNotFunction<char>);
        }

        [TestMethod]
        public void CompileFile_ExpectSuccess()
        {
            // try compiling a literal rule
            var parser = new EbnfTokenizerParser();
            var text = File.ReadAllText("assets/json_tokens.ebnf");
            var (tokens, rule) = parser.Parse(text);

            Directory.CreateDirectory("output");

            File.WriteAllText("output/json_tokenizer_tokens.html",
                parser.Tokenizer.AnnotateTextUsingHtml(text, tokens, AnnotationMarkup.CreateTokenStyleLookup()));

            var rules = parser.Compile(text, tokens, rule);

            Assert.IsTrue(rules.Root != null);
            Assert.IsTrue(rules.Root.Name == "json_tokens");

            var stringRule = rules.FindRule("string");

            Assert.IsNotNull(stringRule);

            // test single quote
            var result = stringRule.Parse("'foo'".ToArray(), 0);

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.Annotations[0].Range.Start == 0);
            Assert.IsTrue(result.Annotations[0].Range.Length == "'foo'".Length);

            // test double quote
            result = stringRule.Parse("\"foo\"".ToArray(), 0);

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.Annotations[0].Range.Start == 0);
            Assert.IsTrue(result.Annotations[0].Range.Length == "\"foo\"".Length);

            // test parsing
            result = rules.Root.Parse("{\"key\": 123, \"key\": null }".ToArray(), 0);
            Assert.IsTrue(result.FoundMatch);
            var expectedTokens = new[] {
                "scope_start", "string", "kv_separator", "int", "item_separator",
                "string", "kv_separator", "null", "scope_end" };

            Assert.IsTrue(result.Annotations.Count == expectedTokens.Length);

            for (var i = 0; i < expectedTokens.Length; i++)
            {
                Assert.IsTrue(result.Annotations[i].FunctionId == rules.FindRule(expectedTokens[i]).Id);
            }
        }

        [TestMethod]
        public void TestEbnfSpecificationError_Handling()
        {
            var parser = new EbnfTokenizerParser();
            var text = File.ReadAllText("assets/json_tokens.ebnf");
            var (tokens, rule) = parser.Parse(text);
            var rules = parser.Compile(text, tokens, rule);

            var result = rules.Root.Parse("{\"key\": <bunch of errors> 123 }".ToArray(), 0);
            Assert.IsTrue(result.FoundMatch);
            var expectedTokens = new[] {
                "scope_start", "string", "kv_separator", "unknown_token", "int", "scope_end" };
            Assert.IsTrue(result.Annotations.Count == expectedTokens.Length);

            for (var i = 0; i < expectedTokens.Length; i++)
            {
                Assert.IsTrue(result.Annotations[i].FunctionId == rules.FindRule(expectedTokens[i]).Id);
            }
        }
    }
}

