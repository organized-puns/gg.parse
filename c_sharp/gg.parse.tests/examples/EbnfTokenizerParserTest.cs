using gg.parse.ebnf;
using gg.parse.examples;
using gg.parse.rulefunctions;

using static gg.parse.tests.TestUtils;

namespace gg.parse.tests.examples
{
    [TestClass]
    public class EbnfTokenizerParserTest
    {
        [TestMethod]
        public void ParseRule_ExpectSucess()
        {
            var parser = new EbnfTokenParser();

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
        public void TokenizeParseCompileLitRule_ExpectSuccess()
        {
            var (_, _, _, table) = SetupTokenizeParseCompile("lit_rule = 'foo';");

            var litRule = table.FindRule("lit_rule") as MatchDataSequence<char>;

            Assert.IsNotNull(litRule);
            Assert.IsTrue(litRule.Production == AnnotationProduct.Annotation);
            Assert.IsTrue(litRule.DataArray.SequenceEqual("foo".ToArray()));
        }

        [TestMethod]
        public void TokenizeParseCompileSetRule_ExpectSuccess()
        {
            var (_, _, _, table) = SetupTokenizeParseCompile("~set_rule = {'abc'};");

            var setRule = table.FindRule("set_rule") as MatchDataSet<char>;

            Assert.IsNotNull(setRule);
            Assert.IsTrue(setRule.Production == AnnotationProduct.None);
            Assert.IsTrue(setRule.MatchingValues.SequenceEqual("abc".ToArray()));
        }

        [TestMethod]
        public void TokenizeParseCompileRangeRule_ExpectSuccess()
        {
            var (_, _, _, table) = SetupTokenizeParseCompile("range_rule = {'a' .. 'z'};");

            var rangeRule = table.FindRule("range_rule") as MatchDataRange<char>;

            Assert.IsNotNull(rangeRule);
            Assert.IsTrue(rangeRule.MinDataValue == 'a');
            Assert.IsTrue(rangeRule.MaxDataValue == 'z');
        }

        [TestMethod]
        public void TokenizeParseCompileSequenceRule_ExpectSuccess()
        {
            var (_, _, _, table) = SetupTokenizeParseCompile("#sequence_rule = 'foo', 'bar';");

            var sequenceRule = table.FindRule("sequence_rule") as MatchFunctionSequence<char>;

            Assert.IsNotNull(sequenceRule);
            Assert.IsTrue(sequenceRule.Production == AnnotationProduct.Transitive);

            var fooLit = sequenceRule.SequenceSubfunctions[0] as MatchDataSequence<char>;
            Assert.IsNotNull(fooLit);
            Assert.IsTrue(fooLit.DataArray.SequenceEqual("foo".ToArray()));

            var barLit = sequenceRule.SequenceSubfunctions[1] as MatchDataSequence<char>;
            Assert.IsNotNull(barLit);
            Assert.IsTrue(barLit.DataArray.SequenceEqual("bar".ToArray()));
        }

        [TestMethod]
        public void TokenizeParseCompileOptionRule_ExpectSuccess()
        {
            var (_, _, _, table) = SetupTokenizeParseCompile("option_rule = 'foo' | 'bar';");

            var optionRule = table.FindRule("option_rule") as MatchOneOfFunction<char>;

            Assert.IsNotNull(optionRule);
            Assert.IsTrue(optionRule.Production == AnnotationProduct.Annotation);

            var fooLit = optionRule.RuleOptions[0] as MatchDataSequence<char>;
            Assert.IsNotNull(fooLit);
            Assert.IsTrue(fooLit.DataArray.SequenceEqual("foo".ToArray()));

            var barLit = optionRule.RuleOptions[1] as MatchDataSequence<char>;
            Assert.IsNotNull(barLit);
            Assert.IsTrue(barLit.DataArray.SequenceEqual("bar".ToArray()));
        }

        [TestMethod]
        public void TokenizeParseCompileGroupRule_ExpectSuccess()
        {
            var (_, _, _, table) = SetupTokenizeParseCompile("group_rule = ('foo', 'bar') | 'baz';");

            var groupRule = table.FindRule("group_rule") as MatchOneOfFunction<char>;

            Assert.IsNotNull(groupRule);
            var sequenceRule = groupRule.RuleOptions[0] as MatchFunctionSequence<char>;
            var litRule = groupRule.RuleOptions[1] as MatchDataSequence<char>;

            Assert.IsNotNull(sequenceRule);
            Assert.IsNotNull(litRule);
            Assert.IsTrue(litRule.DataArray.SequenceEqual("baz".ToArray()));

            var fooLit = sequenceRule.SequenceSubfunctions[0] as MatchDataSequence<char>;
            Assert.IsNotNull(fooLit);
            Assert.IsTrue(fooLit.DataArray.SequenceEqual("foo".ToArray()));

            var barLit = sequenceRule.SequenceSubfunctions[1] as MatchDataSequence<char>;
            Assert.IsNotNull(barLit);
            Assert.IsTrue(barLit.DataArray.SequenceEqual("bar".ToArray()));
        }

        [TestMethod]
        public void TokenizeParseCompileReferenceRule_ExpectSuccess()
        {
            var (_, _, _, table) = SetupTokenizeParseCompile("sequence_rule = foo, bar; foo = 'foo'; bar = 'bar';");

            var sequenceRule = table.FindRule("sequence_rule") as MatchFunctionSequence<char>;
            Assert.IsNotNull(sequenceRule);

            var fooLitRef = sequenceRule.SequenceSubfunctions[0] as RuleReference<char>;
            Assert.IsNotNull(fooLitRef);

            var fooLit = fooLitRef.Rule as MatchDataSequence<char>;
            Assert.IsNotNull(fooLit);
                        
            Assert.IsTrue(fooLit.DataArray.SequenceEqual("foo".ToArray()));
            Assert.IsTrue(table.FindRule("foo") == fooLit);

            var barLitRef = sequenceRule.SequenceSubfunctions[1] as RuleReference<char>;
            Assert.IsNotNull(barLitRef);

            var barLit = barLitRef.Rule as MatchDataSequence<char>; 
            Assert.IsNotNull(barLit);
            
            Assert.IsTrue(barLit.DataArray.SequenceEqual("bar".ToArray()));
            Assert.IsTrue(table.FindRule("bar") == barLit);
        }

        [TestMethod]
        public void TokenizeParseCompileZeroOrMoreRule_ExpectSuccess()
        {
            var (_, _, _, table) = SetupTokenizeParseCompile("zero_or_more_rule = *'bar';");

            var zeroOrMore = table.FindRule("zero_or_more_rule") as MatchFunctionCount<char>;
            Assert.IsNotNull(zeroOrMore);
            Assert.IsTrue(zeroOrMore.Min == 0);
            Assert.IsTrue(zeroOrMore.Max == 0);
            Assert.IsTrue(zeroOrMore.Function == table.FindRule("zero_or_more_rule(Literal)"));
        }

        [TestMethod]
        public void TokenizeParseCompileOneOrMoreRule_ExpectSuccess()
        {
            var (_, _, _, table) = SetupTokenizeParseCompile("one_or_more_rule = +'bar';");

            var oneOrMore = table.FindRule("one_or_more_rule") as MatchFunctionCount<char>;
            Assert.IsNotNull(oneOrMore);
            Assert.IsTrue(oneOrMore.Min == 1);
            Assert.IsTrue(oneOrMore.Max == 0);          
            Assert.IsTrue(oneOrMore.Function == table.FindRule("one_or_more_rule(Literal)"));
        }

        [TestMethod]
        public void TokenizeParseCompileZeroOrOneRule_ExpectSuccess()
        {
            var (_, _, _, table) = SetupTokenizeParseCompile("zero_or_one_rule = ?'bar';");

            var oneOrMore = table.FindRule("zero_or_one_rule") as MatchFunctionCount<char>;
            Assert.IsNotNull(oneOrMore);
            Assert.IsTrue(oneOrMore.Min == 0);
            Assert.IsTrue(oneOrMore.Max == 1);
            Assert.IsTrue(oneOrMore.Function == table.FindRule("zero_or_one_rule(Literal)"));
        }

        [TestMethod]
        public void TokenizeParseCompileErrorRule_ExpectSuccess()
        {
            var (_, _, _, table) = SetupTokenizeParseCompile("error_rule = error 'msg' !'bar';");

            var error = table.FindRule("error_rule") as MarkError<char>;
            Assert.IsNotNull(error);
            Assert.IsTrue(error.Message == "msg");
            Assert.IsTrue(error.TestFunction == table.FindRule("error_rule skip_until Not"));
            var matchFunction = error.TestFunction as MatchNotFunction<char>;
            Assert.IsNotNull(matchFunction);
            Assert.IsTrue(matchFunction.Rule == table.FindRule("error_rule skip_until Not(not Literal)"));
            Assert.IsTrue(matchFunction.Rule is MatchDataSequence<char>);
        }

        [TestMethod]
        public void TokenizeParseCompileAnyRule_ExpectSuccess()
        {
            var (_, _, _, table) = SetupTokenizeParseCompile("any_rule = .;");

            var matchAny = table.FindRule("any_rule") as MatchAnyData<char>;
            Assert.IsNotNull(matchAny);
            Assert.IsTrue(matchAny.MinLength == 1);
            Assert.IsTrue(matchAny.MaxLength == 1);
        }

        [TestMethod]
        public void TokenizeParseCompileFile_ExpectSuccess()
        {
            var (text,tokens,astNodes,table) = SetupTokenizeParseCompile(File.ReadAllText("assets/json_tokens.ebnf"));

            // write the tokens to insepct (debug)
            Directory.CreateDirectory("output");
            File.WriteAllText("output/tpc_json_tokenizer_tokens.html",
                new EbnfTokenizer().AnnotateTextUsingHtml(text, tokens, AnnotationMarkup.CreateTokenStyleLookup()));

            Assert.IsTrue(table.Root != null);
            Assert.IsTrue(table.Root.Name == "json_tokens");

            var stringRule = table.FindRule("string");

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

            // test if whitespace is there
            var whiteSpace = table.FindRule("white_space");
            Assert.IsNotNull(whiteSpace);
            Assert.IsTrue(whiteSpace.Production == AnnotationProduct.None);

            // test parsing
            result = table.Root.Parse("{\"key\": 123, \"key\": null }".ToArray(), 0);
            Assert.IsTrue(result.FoundMatch);
            var expectedTokens = new[] {
                "scope_start", "string", "kv_separator", "int", "item_separator",
                "string", "kv_separator", "null", "scope_end" };

            Assert.IsTrue(result.Annotations.Count == expectedTokens.Length);

            for (var i = 0; i < expectedTokens.Length; i++)
            {
                Assert.IsTrue(result.Annotations[i].FunctionId == table.FindRule(expectedTokens[i]).Id);
            }
        }


        [TestMethod]
        public void TestEbnfSpecificationError_Handling()
        {
            var (text, tokens, astNodes, table) = SetupTokenizeParseCompile(File.ReadAllText("assets/json_tokens.ebnf"));

            var result = table.Root.Parse("{\"key\": <bunch of errors> 123 }".ToArray(), 0);
            Assert.IsTrue(result.FoundMatch);
            var expectedTokens = new[] {
                "scope_start", "string", "kv_separator", "unknown_token", "int", "scope_end" };
            Assert.IsTrue(result.Annotations.Count == expectedTokens.Length);

            for (var i = 0; i < expectedTokens.Length; i++)
            {
                Assert.IsTrue(result.Annotations[i].FunctionId == table.FindRule(expectedTokens[i]).Id);
            }
        }


    }
}

