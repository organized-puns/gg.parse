// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.rules;
using gg.parse.script.common;
using gg.parse.script.compiler;
using gg.parse.script.parser;
using gg.parse.util;
using Newtonsoft.Json.Linq;
using static gg.parse.tests.TestAnnotation;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.script.tests.compiler
{
    [TestClass]
    public class TokenizerCompilerTests
    {
        [TestMethod]
        public void CreateAnyRuleAnnotation_Compile_ExpectRuleCreated()
        {
            CompileRuleTest<MatchAnyData<char>>(
                CreateAnnotationTree(ScriptParser.Names.Any, "."),
                ".", 
                ["x", "1", "_" ],
                [""]
            );
        }

        [TestMethod]
        public void CreateLiteralRuleAnnotation_Compile_ExpectRuleCreated()
        {
            var literal = "literal";
            var script = $"'{literal}'";
            var tree = CreateAnnotationTree(ScriptParser.Names.Literal, script);

            CompileRuleTest<MatchDataSequence<char>>(
                tree, 
                script,
                [literal],
                ["Lit", "Literal", "iteral", ""]
            );
        }

        [TestMethod]
        public void CreateCharacterSetRuleAnnotation_Compile_ExpectRuleCreated()
        {
            var set = "'abc'";
            var script = $"{{{set}}}";
            var tree =
                CreateAnnotationTree(
                    ScriptParser.Names.CharacterSet,
                    script,
                    NewAnnotation(ScriptParser.Names.Literal, 5, set.Length)
                );

            CompileRuleTest<MatchDataSet<char>>(
                tree,
                script,
                ["a", "b", "c"],
                ["A", "d", "1", ""]
            );
        }

        [TestMethod]
        public void CreateCharacterRangeTree_Compile_ExpectRuleCreated()
        {
            var min = "'a'";
            var max = "'z'";
            var range = $"{min}..{max}";
            var script = $"{{{range}}}";
            var tree =
                CreateAnnotationTree(
                    ScriptParser.Names.CharacterRange,
                    script,
                    NewAnnotation(ScriptParser.Names.Literal, 5, min.Length),
                    NewAnnotation(ScriptParser.Names.Literal, 10, max.Length)
                );

            CompileRuleTest<MatchDataRange<char>>(
                tree,
                script,
                ["a", "k", "z"],
                ["A", "Z", "0", ""]
            );
        }

        [TestMethod]
        public void CreateReferenceTree_Compile_ExpectRuleCreated()
        {
            var script = $"reference";
            var tree = CreateAnnotationTree(ScriptParser.Names.Reference, script);
            var session = new RuleCompilationContext($"foo={script};", [tree]);
            var compiledRule = new TokenizerCompiler().Compile<RuleReference<char>>(tree, session);

            IsFalse(session.Logs.Contains(LogLevel.Error | LogLevel.Fatal));
            IsNotNull(compiledRule);
            IsTrue(compiledRule.Name == "foo");
            IsTrue(compiledRule.ReferenceName == script);
        }

        [TestMethod]
        public void CreateSequenceTree_Compile_ExpectRuleCreated()
        {
            BinaryOperatorTest<MatchRuleSequence<char>>(
                ScriptParser.Names.Sequence,
                ",",
                ["abc"],
                ["a", "ab", "bc", ""]
            );
        }

        [TestMethod]
        public void CreateOneOfTree_Compile_ExpectRuleCreated()
        {
            BinaryOperatorTest<MatchOneOf<char>>(
                ScriptParser.Names.MatchOneOf,
                "|",
                ["a", "b", "c"],
                ["d", "1", "A", ""]
            );
        }

        [TestMethod]
        public void CreateEvaluationTree_Compile_ExpectRuleCreated()
        {
            BinaryOperatorTest<MatchEvaluation<char>>(
                ScriptParser.Names.Evaluation,
                "/",
                ["a", "b", "c"],
                ["d", "1", "A", ""]
            );
        }

        [TestMethod]
        public void CreateGroupAnnotationTree_Compile_ExpectRuleCreated()
        {
            var literal = "'foo'";
            var script = $"({literal})";
            var tree =
                CreateAnnotationTree(
                    ScriptParser.Names.Group,
                    script,
                    NewAnnotation(ScriptParser.Names.Literal, 5, literal.Length)
                );

            CompileRuleTest<MatchDataSequence<char>>(
                tree,
                script,
                ["foo"],
                ["", "Foo",  ""]
            );
        }

        [TestMethod]
        public void CreateRangedCountAnnotationTree_Compile_ExpectRuleCreated()
        {
            var literal = "'foo'";
            var script = $"[2..3]{literal})";
            var tree =
                CreateAnnotationTree(
                    ScriptParser.Names.Count,
                    script,
                    NewAnnotation(CommonTokenNames.Integer, 5, 1),
                    NewAnnotation(CommonTokenNames.Integer, 8, 1),
                    NewAnnotation(ScriptParser.Names.Literal, 10, literal.Length)
                );

            CompileRuleTest<MatchCount<char>>(
                tree,
                script,
                ["foofoo", "foofoofoo"],
                ["", "foo", "foofo", "fooFoo"]
            );
        }

        [TestMethod]
        public void CreateZeroOrMoreAnnotationTree_Compile_ExpectRuleCreated()
        {
            var literal = "'foo'";
            var script = $"*{literal})";
            var tree =
                CreateAnnotationTree(
                    ScriptParser.Names.ZeroOrMore,
                    script,
                    NewAnnotation(ScriptParser.Names.Literal, 5, literal.Length)
                );

            var countRule = CompileRuleTest<MatchCount<char>>(
                tree,
                script,
                ["foofoo", "foofoofoo"],
                null
            );

            IsTrue(countRule.Min == 0);
            IsTrue(countRule.Max == 0);
        }

        [TestMethod]
        public void CreateOneOrMoreAnnotationTree_Compile_ExpectRuleCreated()
        {
            var literal = "'foo'";
            var script = $"+{literal})";
            var tree =
                CreateAnnotationTree(
                    ScriptParser.Names.OneOrMore,
                    script,
                    NewAnnotation(ScriptParser.Names.Literal, 5, literal.Length)
                );

            var countRule = CompileRuleTest<MatchCount<char>>(
                tree,
                script,
                ["foofoo", "foofoofoo"],
                ["fo", ""]
            );

            IsTrue(countRule.Min == 1);
            IsTrue(countRule.Max == 0);
        }

        [TestMethod]
        public void CreateZeroOrOneAnnotationTree_Compile_ExpectRuleCreated()
        {
            var literal = "'foo'";
            var script = $"?{literal})";
            var tree =
                CreateAnnotationTree(
                    ScriptParser.Names.ZeroOrOne,
                    script,
                    NewAnnotation(ScriptParser.Names.Literal, 5, literal.Length)
                );

            var countRule = CompileRuleTest<MatchCount<char>>(
                tree,
                script,
                ["foo", "", "xxx"],
                null
            );

            IsTrue(countRule.Min == 0);
            IsTrue(countRule.Max == 1);
            IsTrue(countRule.Parse("foofoo").MatchLength == 3);
            IsTrue(countRule.Parse("xxx").MatchLength == 0);
        }

        [TestMethod]
        public void CreateNotAnnotationTree_Compile_ExpectRuleCreated()
        {
            var literal = "'foo'";
            var script = $"!{literal})";
            var tree =
                CreateAnnotationTree(
                    ScriptParser.Names.Not,
                    script,
                    NewAnnotation(ScriptParser.Names.Literal, 5, literal.Length)
                );

            var notRule = CompileRuleTest<MatchNot<char>>(
                tree,
                script,
                ["bar", "", "xxx"],
                ["foo"]
            );

            IsTrue(notRule.Parse("foo").MatchLength == 0);
            IsTrue(notRule.Parse("xxx").MatchLength == 0);
        }

        [TestMethod]
        public void CreateStopAtAnnotationTree_Compile_ExpectRuleCreated()
        {
            // xxx we need tokens for this
            var token = "stop_at";
            var literal = "'foo'";
            var script = $"{token} {literal})";
            var tree =
                CreateAnnotationTree(
                    ScriptParser.Names.StopAt,
                    script,
                    NewAnnotation(ScriptParser.Names.Literal, 4 + token.Length + 1, literal.Length)
                );

            var stopAtRule = CompileRuleTest<SkipRule<char>>(
                tree,
                script,
                // stop at always succeeds
                ["bar", "", "xxx", "foo"],
                null
            );

            IsTrue(stopAtRule.Parse("foo").MatchLength == 0);
            IsTrue(stopAtRule.Parse("xxxfoo").MatchLength == 3);
            IsTrue(stopAtRule.Parse("xxxfo").MatchLength == 5);
        }

        [TestMethod]
        public void CreateStopAfterAnnotationTree_Compile_ExpectRuleCreated()
        {
            // xxx we need tokens for this
            var token = "stop_after";
            var literal = "'foo'";
            var script = $"{token} {literal})";
            var tree =
                CreateAnnotationTree(
                    ScriptParser.Names.StopAfter,
                    script,
                    NewAnnotation(ScriptParser.Names.Literal, 4 + token.Length + 1, literal.Length)
                );

            var stopAfterRule = CompileRuleTest<SkipRule<char>>(
                tree,
                script,
                // stop after always succeeds
                ["bar", "", "xxx", "foo"],
                null
            );

            IsTrue(stopAfterRule.Parse("foo").MatchLength == 3);
            IsTrue(stopAfterRule.Parse("xxxfoo").MatchLength == 6);
            IsTrue(stopAfterRule.Parse("xxxfoobar").MatchLength == 6);
            IsTrue(stopAfterRule.Parse("xxxfo").MatchLength == 5);
        }

        [TestMethod]
        public void CreateFindAnnotationTree_Compile_ExpectRuleCreated()
        {
            // xxx we need tokens for this
            var token = "find";
            var literal = "'foo'";
            var script = $"{token} {literal})";
            var tree =
                CreateAnnotationTree(
                    ScriptParser.Names.Find,
                    script,
                    NewAnnotation(ScriptParser.Names.Literal, 4 + token.Length + 1, literal.Length)
                );

            var findRule = CompileRuleTest<SkipRule<char>>(
                tree,
                script,
                // find only succeeds if the subject is found
                ["foo", "123foo", "xfoox"],
                ["", "123fo", "xx"]
            );

            IsTrue(findRule.Parse("xfoox").MatchLength == 1);
            IsTrue(findRule.Parse("xxxfoo").MatchLength == 3);
            IsTrue(findRule.Parse("xxxfoobar").MatchLength == 3);
        }

        [TestMethod]
        public void CreateIfAnnotationTree_Compile_ExpectRuleCreated()
        {
            // xxx we need tokens for this
            var token = "if";
            var literal = "'foo'";
            var script = $"{token} {literal})";
            var tree =
                CreateAnnotationTree(
                    ScriptParser.Names.If,
                    script,
                    NewAnnotation(ScriptParser.Names.Literal, 4 + token.Length + 1, literal.Length)
                );

            var ifRule = CompileRuleTest<MatchCondition<char>>(
                tree,
                script,
                ["foo" ],
                ["", "123foo", "xfoo"]
            );

            IsTrue(ifRule.Parse("xfoox", 1));
            IsTrue(ifRule.Parse("xfoox", 1).MatchLength == 0);

            IsTrue(ifRule.Parse("xxxfoo", 3));
            IsTrue(ifRule.Parse("xxxfoo", 3).MatchLength == 0);
        }

        [TestMethod]
        public void CreateLogAnnotationTree_Compile_ExpectRuleCreated()
        {
            LogTest(LogLevel.Info);
            LogTest(LogLevel.Debug);
            LogTest(LogLevel.Warning);
            LogTest(LogLevel.Error);

            try
            {
                LogTest(LogLevel.Fatal);
                Fail();
            }
            catch (FatalConditionException<char>)
            {

            }
        }

        [TestMethod]
        public void CreateConditionalLogAnnotationTree_Compile_ExpectRuleCreated()
        {            
            var logLevel = "info";
            var message = "'foo'";
            var script = $"{logLevel} {message} if {message}";
            var tree =
                CreateAnnotationTree(
                    ScriptParser.Names.Log,
                    script,
                    // loglevel
                    NewAnnotation(CommonTokenNames.LogInfo, 4, message.Length),
                    // message
                    NewAnnotation(ScriptParser.Names.Literal, 4 + logLevel.Length + 1, message.Length),
                    // conditional
                    NewAnnotation(ScriptParser.Names.Literal, 4 + logLevel.Length + 1 + message.Length + 4, message.Length)
                );

            var logRule = CompileRuleTest<LogRule<char>>(
                tree,
                script,
                ["foo"],
                ["", "bar"]
            );

            var result = logRule.Parse("foo");
            IsTrue(result);
            IsNotNull(result.Annotations);
            IsTrue(result.Annotations.Count == 1);
            IsTrue(result.Annotations[0].Rule is LogRule<char>);
        }

        [TestMethod]
        public void CreateBreakAnnotationTree_Compile_ExpectRuleCreated()
        {
            // xxx we need tokens for this
            var token = "break";
            var literal = "'foo'";
            var script = $"{token} {literal})";
            var tree =
                CreateAnnotationTree(
                    ScriptParser.Names.Break,
                    script,
                    NewAnnotation(ScriptParser.Names.Literal, 4 + token.Length + 1, literal.Length)
                );

            // can not really test this - but in debug this will stop the debugger
            CompileRuleTest<BreakPointRule<char>>(
                tree,
                script,
                ["foo"],
                null
            );
        }


        [TestMethod]
        public void CreateNopRuleAnnotationTree_Compile_ExpectWarnings()
        {
            var ruleName = "rule";
            var tree = NewAnnotation(ScriptParser.Names.Rule, 0, 5, NewAnnotation(CommonTokenNames.Identifier, 0, 4));
            var script = $"{ruleName} = ;";

            var session = new RuleCompilationContext(script, [tree]);
            var compiler = new TokenizerCompiler();
            var compiledRule = (IRule) compiler.Compile(null, tree, session);

            IsNotNull(compiledRule);
            IsTrue(compiledRule is NopRule<char>);
            IsTrue(compiledRule.Name == ruleName);
            IsTrue(session.Logs.Count == 1);
            IsTrue(session.Logs[0].Level == LogLevel.Warning);
        }

        // -- Private methods -----------------------------------------------------------------------------------------

        private static void LogTest(LogLevel level)
        {
            var levelString = level.ToString().ToLower();
            var message = "'foo'";
            var script = $"{levelString} {message}";
            var tree =
                CreateAnnotationTree(
                    ScriptParser.Names.Log,
                    script,
                    NewAnnotation(CommonTokenNames.LogInfo, 4, levelString.Length),
                    NewAnnotation(ScriptParser.Names.Literal, 4 + levelString.Length + 1, message.Length)
                );

            var logRule = CompileRuleTest<LogRule<char>>(
                tree,
                script,
                // log rule without condition always succeeds
                ["", "foo"],
                null
            );

            var result = logRule.Parse("");
            IsTrue(result);
            IsNotNull(result.Annotations);
            IsTrue(result.Annotations.Count == 1);
            IsTrue(result.Annotations[0].Rule is LogRule<char>);
            IsTrue(((LogRule<char>)result.Annotations[0].Rule).Level == level);
        }

        private static void BinaryOperatorTest<T>(
            string operatorName, 
            string separator, 
            string[] validCases, 
            string[] invalidCases) where T : IRule
        {
            var a = "'a'";
            var b = "'b'";
            var c = "'c'";
            var script = $"{a}{separator}{b}{separator}{c}";
            
            var tree =
                CreateAnnotationTree(
                    operatorName,
                    script,
                    NewAnnotation(ScriptParser.Names.Literal, 4, a.Length),
                    NewAnnotation(ScriptParser.Names.Literal, 4 + separator.Length + a.Length, b.Length),
                    NewAnnotation(ScriptParser.Names.Literal, 4 + 2 * separator.Length + a.Length + b.Length, c.Length)
                );

            CompileRuleTest<T>(tree, script, validCases, invalidCases);
        }

        private static Annotation CreateAnnotationTree(
            string dataRuleName, 
            string dataRuleScript, 
            params Annotation[] childAnnotations
        )
        {
            return 
                NewAnnotation(ScriptParser.Names.Rule, 0, 5,
                    NewAnnotation(CommonTokenNames.Identifier, 0, 3),
                    NewAnnotation(dataRuleName, 4, dataRuleScript.Length, childAnnotations)
                );
        }

        private static T CompileRuleTest<T>(
            Annotation root,
            string dataRuleScript, 
            string[] validCases,
            string[] invalidCases) where T : IRule
        {
            var session = new RuleCompilationContext($"foo={dataRuleScript};", [root]);
            var compiler = new TokenizerCompiler();
            var compiledRule = compiler.Compile<T>(root, session);

            IsFalse(session.Logs.Contains(LogLevel.Error | LogLevel.Fatal));
            IsNotNull(compiledRule);
            IsTrue(compiledRule.Name == "foo");

            validCases.ForEach(input => IsTrue(compiledRule.Parse(input)));

            invalidCases?.ForEach(input => IsFalse(compiledRule.Parse(input)));

            return compiledRule;
        }
    }
}
