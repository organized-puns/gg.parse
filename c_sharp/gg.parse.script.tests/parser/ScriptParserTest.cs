using gg.parse.rules;
using gg.parse.script.common;
using gg.parse.script.parser;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

using Range = gg.parse.util.Range;

namespace gg.parse.script.tests.parser
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
            IsTrue(optionRule.Name == parser.MatchOneOf.Name);
            IsTrue(optionRule.GetType() == typeof(MatchRuleSequence<int>));
            
            var option1 = nodes[0][1][0].Rule;
            IsTrue(option1.Name == ScriptParser.Names.Literal);

            var option2 = nodes[0][1][1].Rule;
            IsTrue(option2.Name == ScriptParser.Names.Literal);
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
            IsTrue(name == "rule_name");
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
            IsTrue(name == ScriptParser.Names.MatchOneOf);

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
            (_, nodes) = parser.Parse("rule = *('123'|{'foo'});");

            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == ScriptParser.Names.ZeroOrMore);

            name = nodes[0].Children[1].Children[0].Rule.Name;
            IsTrue(name == ScriptParser.Names.MatchOneOf);

            // try parsing a transitive rule
            (_, nodes) = parser.Parse("-r rule = !('123',{'foo'});");
           
            name = nodes[0].Children[0].Rule.Name;
            IsTrue(name == CommonTokenNames.PruneRoot);

            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == "rule_name");

            name = nodes[0].Children[2].Rule.Name;
            IsTrue(name == ScriptParser.Names.Not);

            // try parsing a no output rule
            (_, nodes) = parser.Parse("-a rule = ?('123',{'foo'});");

            name = nodes[0].Children[0].Rule.Name;
            IsTrue(name == CommonTokenNames.PruneAll);

            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == "rule_name");

            name = nodes[0].Children[2].Rule.Name;
            IsTrue(name == ScriptParser.Names.ZeroOrOne);

            // try parsing an identifier
            (_, nodes) = parser.Parse("rule = +(one, two, three);");

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
            (_, nodes) = parser.Parse("rule = if \"lit\";");

            IsTrue(nodes != null);
            name = nodes[0].Children[1].Rule.Name;
            IsTrue(name == ScriptParser.Names.If);

            // try parsing a try match with eoln
            (_, nodes) = parser.Parse("rule = if\n\"lit\";");

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

        /// <summary>
        /// Validate precedence parsing
        /// </summary>
        [TestMethod]
        public void CreateRuleWithPrecedence_ParseRule_ExpectRuleToHaveCorrectPrecedence()
        {
            var tokenizer = new ScriptTokenizer();
            var parser = new ScriptParser(tokenizer);
            var expectedPrecedence = 100;
            var tokenizeResult = tokenizer.Tokenize($"rule {expectedPrecedence} = .;");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);
            IsTrue(tokenizeResult.Annotations.Count == 5);

            var parseResult = parser.Root!.Parse(tokenizeResult.Annotations);

            IsTrue(parseResult.FoundMatch);
            IsNotNull(parseResult.Annotations);
            IsTrue(parseResult.Annotations.Count == 1);

            // rule should have precedence set
            var root = parseResult.Annotations[0];

            IsTrue(root.Children != null && root.Children.Count == 3);

            // header should have name and precedence 
            IsTrue(root.Children[0].Rule == parser.MatchRuleName);
            IsTrue(root.Children[1].Rule == parser.MatchPrecedence);
        }

        [TestMethod]
        public void CreateEvalRule_ParseRule_ExpectEvalRuleAnnotations()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizerParser = new ScriptParser(tokenizer);
            var tokenizeResult = tokenizer.Tokenize($"rule = 'foo' / 'bar' / 'baz';");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);
            IsTrue(tokenizeResult.Annotations.Count == 8);

            var parseResult = tokenizerParser.Root!.Parse(tokenizeResult.Annotations);

            IsTrue(parseResult.FoundMatch);
            IsNotNull(parseResult.Annotations);
            IsTrue(parseResult.Annotations.Count == 1);

            var root = parseResult.Annotations[0];

            IsTrue(root.Rule == tokenizerParser.MatchRule);
            IsTrue(root.Children != null && root.Children.Count == 2);

            // declaration should have name and an eval
            IsTrue(root.Children[0].Rule == tokenizerParser.MatchRuleName);
            IsTrue(root.Children[1].Rule == tokenizerParser.MatchEval);

            var eval = root.Children[1];

            IsTrue(eval.Children != null && eval.Children.Count == 3);

            IsTrue(eval.Children.All(child => child.Rule == tokenizerParser[ScriptParser.Names.Literal]));
        }


        /// <summary>
        /// Try parse a literal with a output qualifier. This should yield an unexpected product error. 
        /// </summary>
        [TestMethod]
        public void CreateTokensForLiteralWithProductModifier_ParseUnexpectedProductError_ExpectMatchFound()
        {
            TestParseUnexpectedProductError(
                "-a 'foo'",
                2,
                [
                    tokenizerParser => tokenizerParser.MatchPruneAllToken.Id,
                    tokenizerParser => tokenizerParser.MatchLiteral.Id,
                    tokenizerParser => tokenizerParser.UnexpectedPrunetokenInBodyError.Id
                ]
            );
        }

        /// <summary>
        /// Try parse a range with a output qualifier. This should yield an unexpected product error. 
        /// </summary>
        [TestMethod]
        public void CreateTokensForRangeWithProductModifier_ParseUnexpectedProductError_ExpectMatchFound()
        {
            TestParseUnexpectedProductError(
                "-a {'0'..'9'}",
                6,
                [
                    tokenizerParser => tokenizerParser.MatchPruneAllToken.Id,
                    tokenizerParser => tokenizerParser.MatchCharacterRange.Id,
                    tokenizerParser => tokenizerParser.UnexpectedPrunetokenInBodyError.Id
                ]
            );
        }

        /// <summary>
        /// Try parse a set with a output qualifier. This should yield an unexpected product error. 
        /// </summary>
        [TestMethod]
        public void CreateTokensForSetWithProductModifier_ParseUnexpectedProductError_ExpectMatchFound()
        {
            TestParseUnexpectedProductError(
                "-r {'abc'}",
                4,
                [
                    tokenizerParser => tokenizerParser.MatchPruneRootToken.Id,
                    tokenizerParser => tokenizerParser.MatchCharacterSet.Id,
                    tokenizerParser => tokenizerParser.UnexpectedPrunetokenInBodyError.Id
                ]
            );
        }

        /// <summary>
        /// Try parse a set with a output qualifier. This should yield an unexpected product error. 
        /// </summary>
        [TestMethod]
        public void CreateTokensForAnyWithProductModifier_ParseUnexpectedProductError_ExpectMatchFound()
        {
            TestParseUnexpectedProductError(
                "-r .",
                2,
                [
                    tokenizerParser => tokenizerParser.MatchPruneRootToken.Id,
                    tokenizerParser => tokenizerParser.MatchAnyToken.Id,
                    tokenizerParser => tokenizerParser.UnexpectedPrunetokenInBodyError.Id
                ]
            );
        }

        /// <summary>
        /// Try parse a group with a output qualifier. This should yield an unexpected product error. 
        /// </summary>
        [TestMethod]
        public void CreateTokensForGroupWithProductModifier_ParseUnexpectedProductError_ExpectMatchFound()
        {
            TestParseUnexpectedProductError(
                "-r ('foo')",
                4,
                [
                    tokenizerParser => tokenizerParser.MatchPruneRootToken.Id,
                    // group is transitive
                    tokenizerParser => tokenizerParser.MatchLiteral.Id,
                    tokenizerParser => tokenizerParser.UnexpectedPrunetokenInBodyError.Id
                ]
            );
        }

        /// <summary>
        /// Try parse a not with a output qualifier. This should yield an unexpected product error. 
        /// </summary>
        [TestMethod]
        public void CreateTokensForNotWithProductModifier_ParseUnexpectedProductError_ExpectMatchFound()
        {
            TestParseUnexpectedProductError(
                "-a !'foo'",
                3,
                [
                    tokenizerParser => tokenizerParser.MatchPruneAllToken.Id,
                    tokenizerParser => tokenizerParser.MatchNotOperator.Id,
                    tokenizerParser => tokenizerParser.UnexpectedPrunetokenInBodyError.Id
                ]
            );
        }

        /// <summary>
        /// Try parse a count with a output qualifier. This should yield an unexpected product error. 
        /// </summary>
        [TestMethod]
        public void CreateTokensForCountWithProductModifier_ParseUnexpectedProductError_ExpectMatchFound()
        {
            TestParseUnexpectedProductError(
                "-r *'foo'",
                3,
                [
                    tokenizerParser => tokenizerParser.MatchPruneRootToken.Id,
                    tokenizerParser => tokenizerParser.MatchZeroOrMoreOperator.Id,
                    tokenizerParser => tokenizerParser.UnexpectedPrunetokenInBodyError.Id
                ]
            );

            TestParseUnexpectedProductError(
                "-a ?'foo'",
                3,
                [
                    tokenizerParser => tokenizerParser.MatchPruneAllToken.Id,
                    tokenizerParser => tokenizerParser.MatchZeroOrOneOperator.Id,
                    tokenizerParser => tokenizerParser.UnexpectedPrunetokenInBodyError.Id
                ]
            );

            TestParseUnexpectedProductError(
                "-a +'foo'",
                3,
                [
                    tokenizerParser => tokenizerParser.MatchPruneAllToken.Id,
                    tokenizerParser => tokenizerParser.MatchOneOrMoreOperator.Id,
                    tokenizerParser => tokenizerParser.UnexpectedPrunetokenInBodyError.Id
                ]
            );
        }

        private static void TestParseUnexpectedProductError(
            string testData,
            int expectedTokenCount,
            Func<ScriptParser, int>[] expectedFunctionIds
        )
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizeResult = tokenizer.Tokenize(testData);

            IsTrue(tokenizeResult.FoundMatch);
            IsTrue(tokenizeResult.Annotations != null && tokenizeResult.Annotations.Count == expectedTokenCount);

            var tokenizerParser = new ScriptParser(tokenizer);
            var errorParseResult = tokenizerParser
                .MatchUnexpectedPruneTokenInBodyError
                .Parse([.. tokenizeResult.Annotations.Select(a => a.Rule.Id)], 0);

            IsTrue(errorParseResult.FoundMatch);
            IsTrue(errorParseResult.MatchLength == expectedTokenCount);
            IsTrue(errorParseResult.Annotations != null
                    && errorParseResult.Annotations.Count == 1
                    && errorParseResult.Annotations[0].Children != null
                    && errorParseResult.Annotations[0].Children!.Count == 3
                    && errorParseResult.Annotations[0].Rule == tokenizerParser.MatchUnexpectedPruneTokenInBodyError);

            for (var i = 0; i < expectedFunctionIds.Length; i++)
            {
                IsTrue(errorParseResult.Annotations[0][i]!.Rule.Id == expectedFunctionIds[i](tokenizerParser));
            }
        }

        /// <summary>
        /// Random # or ~ without following keyword should lead to an error in a rule.
        /// </summary>
        [TestMethod]
        public void CreateRuleWithProductionModifiersInElements_ParseRule_ExpectErrorsInAnnotations()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizerParser = new ScriptParser(tokenizer);
            var tokenizeResult = tokenizer.Tokenize($"rule = -a foo, -r 'bar', -a {{'a'..'z'}};");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var parseResult = tokenizerParser.Root.Parse(tokenizeResult.Annotations);

            IsTrue(parseResult.FoundMatch);
            IsTrue(parseResult.Annotations != null && parseResult.Annotations.Count == 1);

            // rulename & sequence
            IsTrue(parseResult.Annotations[0].Children != null && parseResult.Annotations[0].Children!.Count == 2);

            var sequence = parseResult[0]![1];
            IsTrue(sequence!.Children != null && sequence!.Children!.Count == 3);
            IsTrue(sequence[0]!.Rule == tokenizerParser.MatchReference);
            IsTrue(sequence[1]!.Rule == tokenizerParser.MatchUnexpectedPruneTokenInBodyError);
            IsTrue(sequence[2]!.Rule == tokenizerParser.MatchUnexpectedPruneTokenInBodyError);
        }

        [TestMethod]
        public void CreateRuleWithInvalidRuleDefinition_Parse_ExpectDefintionMarkedWithError()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizerParser = new ScriptParser(tokenizer);
            var tokenizeResult = tokenizer.Tokenize("rule = *; rule2 = 'foo';");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var parseResult = tokenizerParser.Root!.Parse(tokenizeResult.Annotations);

            IsTrue(parseResult.FoundMatch);
            IsTrue(parseResult.Annotations != null && parseResult.Annotations.Count == 2);
            IsTrue(parseResult.Annotations[0] != null
                    && parseResult[0].Children != null
                    && parseResult[0].Children!.Count == 2);
            
            IsTrue(parseResult[0][1].Rule == tokenizerParser.MatchZeroOrMoreOperator);
            IsTrue(parseResult[0][1].Range.Equals(new Range(2, 1)));

            IsTrue(parseResult[0][1][0].Rule == tokenizerParser.MissingUnaryOperatorTerm);
        }

        [TestMethod]
        public void CreateRuleWithMissingEndRule_Parse_ExpectErrorRaised()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizerParser = new ScriptParser(tokenizer);
            var tokenizeResult = tokenizer.Tokenize($"rule = 'bar' rule2 = 'foo'");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var parseResult = tokenizerParser.Root!.Parse(tokenizeResult.Annotations);

            IsTrue(parseResult.FoundMatch);
            IsTrue(parseResult.Annotations != null && parseResult.Annotations.Count == 2);
            IsTrue(parseResult.Annotations[0] != null
                    && parseResult.Annotations[0]!.Children != null
                    && parseResult.Annotations[0]!.Children!.Count == 3);
            IsTrue(parseResult.Annotations[0]!.Children![2].Rule == tokenizerParser.MissingRuleEndError);

            IsTrue(parseResult.Annotations[1] != null
                    && parseResult.Annotations[1]!.Children != null
                    && parseResult.Annotations[1]!.Children!.Count == 3);
            IsTrue(parseResult.Annotations[1]!.Children![2].Rule == tokenizerParser.MissingRuleEndError);
        }

        [TestMethod]
        public void CreateRuleWithMissingRemainderOperator_Parse_ExpectErrorRaised()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizerParser = new ScriptParser(tokenizer);
            var tokenizeResult = tokenizer.Tokenize($"rule = a, b c;");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var parseResult = tokenizerParser.Root!.Parse(tokenizeResult.Annotations);

            IsTrue(parseResult.FoundMatch);

            // expecting: rule[0] / sequence[1] / error[2]
            var errorRule = parseResult[0]![1]![2]!.Rule;
            var expectedRule = tokenizerParser.MissingOperatorError[CommonTokenNames.CollectionSeparator];

            IsTrue(errorRule == expectedRule);
        }

        [TestMethod]
        public void CreateRuleWithDifferentRemainderOperator_Parse_ExpectErrorRaised()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizerParser = new ScriptParser(tokenizer);
            var tokenizeResult = tokenizer.Tokenize($"r1 = a, b |c; r2 = d;");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var parseResult = tokenizerParser.Root!.Parse(tokenizeResult.Annotations);

            IsTrue(parseResult.FoundMatch);

            // should find two rules
            IsTrue(parseResult.Annotations!.Count == 2);

            // expecting: rule[0] / sequence[1] / error[2]

            var errorRule = parseResult[0]![1]![2]!.Rule;

            // name should be error containing an indication what operator we're missing
            var expectedRule = tokenizerParser.WrongOperatorTokenError[CommonTokenNames.CollectionSeparator];

            // name should be error containing an indication what operator we're missing
            IsTrue(errorRule == expectedRule);
        }

        [TestMethod]
        public void CreateRuleWithMissingTermsAfterOperatorInRemainder_Parse_ExpectErrorRaised()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizerParser = new ScriptParser(tokenizer);
            var tokenizeResult = tokenizer.Tokenize($"r1 = a, b,; r2 = d;");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var parseResult = tokenizerParser.Root!.Parse(tokenizeResult.Annotations);

            IsTrue(parseResult.FoundMatch);

            // should find two rules
            IsTrue(parseResult.Annotations!.Count == 2);

            // expecting: rule[0] / sequence[1] / error[2]

            var errorRule = parseResult[0]![1]![2]!.Rule;
            var expectedRule = tokenizerParser.MissingTermAfterOperatorInRemainderError[CommonTokenNames.CollectionSeparator];

            // name should be error containing an indication what operator we're missing
            IsTrue(errorRule == expectedRule);
        }

        [TestMethod]
        public void CreateRuleWithMissingTermsAfterOperator_Parse_ExpectErrorRaised()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizerParser = new ScriptParser(tokenizer);
            var tokenizeResult = tokenizer.Tokenize($"r1 = a,; r2 = d;");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var parseResult = tokenizerParser.Root!.Parse(tokenizeResult.Annotations);

            IsTrue(parseResult.FoundMatch);

            // should find two rules
            IsTrue(parseResult.Annotations!.Count == 2);

            // expecting: rule[0] / sequence[1] / error[2]

            var errorRule = parseResult[0]![1]!.Rule;
            var expectedRule = tokenizerParser.MissingTermAfterOperatorError[CommonTokenNames.CollectionSeparator];

            // name should be error containing an indication what operator we're missing
            IsTrue(errorRule == expectedRule);
        }

        [TestMethod]
        public void CreateLogErrorRuleWithText_ParseWithMatchLog_ExpectMatchFound()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizeResult = tokenizer.Tokenize($"error 'text'");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var tokens = tokenizeResult.CollectRuleIds();

            var tokenizerParser = new ScriptParser(tokenizer);
            var parseResult = tokenizerParser.MatchLog.Parse(tokens, 0);

            IsTrue(parseResult.FoundMatch);
            IsTrue(parseResult[0]!.Children != null && parseResult[0]!.Children!.Count == 2);
        }

        [TestMethod]
        public void CreateLogErrorRuleWithTextAndCondition_ParseWithMatchLog_ExpectMatchFound()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizeResult = tokenizer.Tokenize($"warning 'text' if !'foo'");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var tokens = tokenizeResult.CollectRuleIds();

            var tokenizerParser = new ScriptParser(tokenizer);
            var parseResult = tokenizerParser.MatchLog.Parse(tokens, 0);

            IsTrue(parseResult.FoundMatch);
            IsTrue(parseResult[0]!.Children != null && parseResult[0]!.Children!.Count == 3);
        }

        [TestMethod]
        public void CreateRuleWithNoBody_ParseWithMatchRule_ExpectWarning()
        {
            var tokenizer = new ScriptTokenizer();
            var tokenizeResult = tokenizer.Tokenize($"rule = ;");

            IsTrue(tokenizeResult.FoundMatch);
            IsNotNull(tokenizeResult.Annotations);

            var tokens = tokenizeResult.CollectRuleIds();

            var parser = new ScriptParser(tokenizer);
            var ruleMatcher = parser.MatchRule;
            var parseResult = ruleMatcher.Parse(tokens, 0);

            IsTrue(parseResult.FoundMatch);
            var warning = parseResult[0]![1]!.Rule as LogRule<int>;

            IsNotNull(warning);
            IsNotNull(warning.Level == LogLevel.Warning);
        }

        [TestMethod]
        public void CreateRuleWithReferenceAndProduction_ParseWithMatchRule_ExpectCorrectProductionModifiers()
        {
            var tokenizer = new ScriptTokenizer();
            var parser = new ScriptParser(tokenizer);
            var (_, astNodes) = parser.Parse("foo='foo';sequence = -a foo, -r foo, -c foo, foo;");

            // expect two rules
            IsTrue(astNodes.Count == 2);

            // expect output modifiers for each of the sequence's terms
            var sequenceNode = astNodes[1][1];

            var modifierNode = sequenceNode[0][0];

            IsTrue(modifierNode.Rule == parser.MatchPruneAllToken);

            modifierNode = sequenceNode[1][0];

            IsTrue(modifierNode.Rule == parser.MatchPruneRootToken);

            modifierNode = sequenceNode[2][0];

            IsTrue(modifierNode.Rule == parser.MatchPruneChildrenToken);

            // no modifier selector
            IsTrue(sequenceNode[3][0].Rule == parser.IdentifierToken);
        }

        
    }
}

