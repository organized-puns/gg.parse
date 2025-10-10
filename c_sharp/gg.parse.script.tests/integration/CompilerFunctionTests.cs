using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

using gg.parse.rules;
using gg.parse.script.parser;
using gg.parse.script.compiler;
using gg.parse.script.pipeline;

namespace gg.parse.script.tests.integration
{
    [TestClass]
    public class CompilerFunctionTests
    {
        // -- Static Methods ------------------------------------------------------------------------------------------

        /// <summary>
        /// Takes a script text, tokenizes, parses and compiles it returning the compiled rule graph.
        /// Every step is validated.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static RuleGraph<char> Compile(string text)
        {
            var tokenizer = new ScriptTokenizer();
            var parser = new ScriptParser(tokenizer);

            var result = tokenizer.Tokenize(text);

            IsTrue(result.FoundMatch);
            IsTrue(result.Annotations != null && result.Annotations.Count > 0);

            var tokens = result.Annotations;

            result = parser.Root!.Parse(tokens);

            IsTrue(result.FoundMatch);
            IsTrue(result.Annotations != null && result.Annotations.Count > 0);
            IsTrue(result.Annotations[0].Rule != parser.UnknownInputError);

            var syntaxTree = result.Annotations;
            var compiler =
                new RuleCompiler(ScriptPipeline.CreateRuleOutputMapping(parser))
                    .RegisterTokenizerCompilerFunctions(parser);

            return compiler.Compile<char>(text, tokens, syntaxTree);
        }

        // -- Tests ---------------------------------------------------------------------------------------------------

        [TestMethod]
        public void TokenizeParseCompileLitRule_ExpectSuccess()
        {
            var table = Compile("lit_rule = 'foo';");

            var litRule = table.FindRule("lit_rule") as MatchDataSequence<char>;

            IsNotNull(litRule);
            IsTrue(litRule.Output == RuleOutput.Self);
            IsTrue(litRule.DataArray.SequenceEqual("foo".ToArray()));
        }

        [TestMethod]
        public void TokenizeParseCompileSetRule_ExpectSuccess()
        {
            var table = Compile("~set_rule = {'abc'};");

            var setRule = table.FindRule("set_rule") as MatchDataSet<char>;

            IsNotNull(setRule);
            IsTrue(setRule.Output == RuleOutput.Void);
            IsTrue(setRule.MatchingValues.SequenceEqual("abc".ToArray()));
        }

        [TestMethod]
        public void TokenizeParseCompileRangeRule_ExpectSuccess()
        {
            var table = Compile("range_rule = {'a' .. 'z'};");

            var rangeRule = table.FindRule("range_rule") as MatchDataRange<char>;

            IsNotNull(rangeRule);
            IsTrue(rangeRule.MinDataValue == 'a');
            IsTrue(rangeRule.MaxDataValue == 'z');
        }

        [TestMethod]
        public void TokenizeParseCompileSequenceRule_ExpectSuccess()
        {
            var table = Compile("#sequence_rule = 'foo', 'bar';");

            var sequenceRule = table.FindRule("sequence_rule") as MatchRuleSequence<char>;

            IsNotNull(sequenceRule);
            IsTrue(sequenceRule.Output == RuleOutput.Children);

            var fooLit = sequenceRule.SequenceRules[0] as MatchDataSequence<char>;
            IsNotNull(fooLit);
            IsTrue(fooLit.DataArray.SequenceEqual("foo".ToArray()));

            var barLit = sequenceRule.SequenceRules[1] as MatchDataSequence<char>;
            IsNotNull(barLit);
            IsTrue(barLit.DataArray.SequenceEqual("bar".ToArray()));
        }

        [TestMethod]
        public void TokenizeParseCompileOptionRule_ExpectSuccess()
        {
            var table = Compile("option_rule = 'foo' | 'bar';");

            var optionRule = table.FindRule("option_rule") as MatchOneOf<char>;

            IsNotNull(optionRule);
            IsTrue(optionRule.Output == RuleOutput.Self);

            var fooLit = optionRule.RuleOptions[0] as MatchDataSequence<char>;
            IsNotNull(fooLit);
            IsTrue(fooLit.DataArray.SequenceEqual("foo".ToArray()));

            var barLit = optionRule.RuleOptions[1] as MatchDataSequence<char>;
            IsNotNull(barLit);
            IsTrue(barLit.DataArray.SequenceEqual("bar".ToArray()));
        }

        [TestMethod]
        public void TokenizeParseCompileGroupRule_ExpectSuccess()
        {
            var table = Compile("group_rule = ('foo', 'bar') | 'baz';");

            var groupRule = table.FindRule("group_rule") as MatchOneOf<char>;

            IsNotNull(groupRule);
            var sequenceRule = groupRule.RuleOptions[0] as MatchRuleSequence<char>;
            var litRule = groupRule.RuleOptions[1] as MatchDataSequence<char>;

            IsNotNull(sequenceRule);
            IsNotNull(litRule);
            IsTrue(litRule.DataArray.SequenceEqual("baz".ToArray()));

            var fooLit = sequenceRule.SequenceRules[0] as MatchDataSequence<char>;
            IsNotNull(fooLit);
            IsTrue(fooLit.DataArray.SequenceEqual("foo".ToArray()));

            var barLit = sequenceRule.SequenceRules[1] as MatchDataSequence<char>;
            IsNotNull(barLit);
            IsTrue(barLit.DataArray.SequenceEqual("bar".ToArray()));
        }

        [TestMethod]
        public void TokenizeParseCompileReferenceRule_ExpectSuccess()
        {
            var table = Compile("sequence_rule = foo, bar; foo = 'foo'; bar = 'bar';");

            var sequenceRule = table.FindRule("sequence_rule") as MatchRuleSequence<char>;
            IsNotNull(sequenceRule);

            var fooLitRef = sequenceRule.SequenceRules[0] as RuleReference<char>;
            IsNotNull(fooLitRef);

            var fooLit = fooLitRef.Rule as MatchDataSequence<char>;
            IsNotNull(fooLit);

            IsTrue(fooLit.DataArray.SequenceEqual("foo".ToArray()));
            IsTrue(table.FindRule("foo") == fooLit);

            var barLitRef = sequenceRule.SequenceRules[1] as RuleReference<char>;
            IsNotNull(barLitRef);

            var barLit = barLitRef.Rule as MatchDataSequence<char>;
            IsNotNull(barLit);

            IsTrue(barLit.DataArray.SequenceEqual("bar".ToArray()));
            IsTrue(table.FindRule("bar") == barLit);
        }

        [TestMethod]
        public void TokenizeParseCompileZeroOrMoreRule_ExpectSuccess()
        {
            var table = Compile("zero_or_more_rule = *'bar';");

            var zeroOrMore = table.FindRule("zero_or_more_rule") as MatchCount<char>;
            IsNotNull(zeroOrMore);
            IsTrue(zeroOrMore.Min == 0);
            IsTrue(zeroOrMore.Max == 0);
            IsTrue(zeroOrMore.Rule == table.FindRule("zero_or_more_rule of Literal"));
        }

        [TestMethod]
        public void TokenizeParseCompileOneOrMoreRule_ExpectSuccess()
        {
            var table = Compile("one_or_more_rule = +'bar';");

            var oneOrMore = table.FindRule("one_or_more_rule") as MatchCount<char>;
            IsNotNull(oneOrMore);
            IsTrue(oneOrMore.Min == 1);
            IsTrue(oneOrMore.Max == 0);
            IsTrue(oneOrMore.Rule == table.FindRule("one_or_more_rule of Literal"));
        }

        [TestMethod]
        public void TokenizeParseCompileZeroOrOneRule_ExpectSuccess()
        {
            var table = Compile("zero_or_one_rule = ?'bar';");

            var oneOrMore = table.FindRule("zero_or_one_rule") as MatchCount<char>;
            IsNotNull(oneOrMore);
            IsTrue(oneOrMore.Min == 0);
            IsTrue(oneOrMore.Max == 1);
            IsTrue(oneOrMore.Rule == table.FindRule("zero_or_one_rule of Literal"));
        }

        [TestMethod]
        public void TokenizeParseCompileErrorRule_ExpectSuccess()
        {   
            var table = Compile("error_rule = error 'msg' if !'bar';");

            var error = table.FindRule("error_rule") as LogRule<char>;
            IsNotNull(error);
            IsTrue(error.Text == "msg");
            IsTrue(error.Level == LogLevel.Error);
            IsTrue(error.Condition == table.FindRule("error_rule condition: Not"));
            var matchFunction = error.Condition as MatchNot<char>;
            IsNotNull(matchFunction);
            IsTrue(matchFunction.Rule == table.FindRule("error_rule condition: Not, type: Not(Literal)"));
            IsTrue(matchFunction.Rule is MatchDataSequence<char>);
        }

        [TestMethod]
        public void TokenizeParseCompileAnyRule_ExpectSuccess()
        {
            var table = Compile("any_rule = .;");

            var matchAny = table.FindRule("any_rule") as MatchAnyData<char>;
            IsNotNull(matchAny);
        }


        [TestMethod]
        public void SetupTokenizeParseCompileWithPrecedence_TestPrecedence_ExpectPredenceToMatchInput()
        {
            var table = Compile("rule1 100= .;#rule2 200 = .; ~ rule_three -1 = .;");

            var rule1 = table.FindRule("rule1") as MatchAnyData<char>;
            IsNotNull(rule1);
            IsTrue(rule1.Precedence == 100);
            IsTrue(rule1.Output == RuleOutput.Self);

            var rule2 = table.FindRule("rule2") as MatchAnyData<char>;
            IsNotNull(rule2);
            IsTrue(rule2.Precedence == 200);
            IsTrue(rule2.Output == RuleOutput.Children);

            var ruleThree = table.FindRule("rule_three") as MatchAnyData<char>;
            IsNotNull(ruleThree);
            IsTrue(ruleThree.Precedence == -1);
            IsTrue(ruleThree.Output == RuleOutput.Void);
        }

    }
}
