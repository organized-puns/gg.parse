// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.rules;
using gg.parse.script.compiler;
using gg.parse.script.parser;
using gg.parse.util;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.script.tests.compiler
{
    /// <summary>
    /// Tests the integration between the grammar compiler tests and the 
    /// parser / tokenizer
    /// </summary>
    [TestClass]
    public class GrammarCompilerTests
    {
        [TestMethod]
        public void CreateAnyRuleScript_Compile_ExpectRuleCreated()
        {
            // setup
            var grammarScript = "root = .;";
            var grammarRule = SetupRule<MatchAnyData<int>>(grammarScript);

            // act - grammar 
            var anyToken = 42;
            var grammarResult = grammarRule.Parse([anyToken], 0);

            // test - grammar 
            IsTrue(grammarResult);
            IsTrue(grammarResult.Count == 1);
            IsTrue(grammarResult[0].Rule == grammarRule);
        }

        [TestMethod]
        public void CreateBreakRuleScript_Compile_ExpectRuleCreated()
        {
            // setup
            var grammarScript = "root = break foo;";
            var grammarRule = SetupRule<BreakPointRule<int>>(grammarScript);

            // act - grammar 
            // break is pointing to a rule that doesn't exist, so set the subject manually
            // otherwise this will fail with an exception.
            var fooToken = 42;
            ReplaceSubject(grammarRule, fooToken);
            // the rule should capture the break which is only testable in the debugger
            var grammarResult = grammarRule.Parse([fooToken], 0);

            // test - grammar 
            IsTrue(grammarResult);
            IsTrue(grammarResult.Count == 1);
            IsTrue(grammarResult[0].Rule == grammarRule);
        }

        [TestMethod]
        public void CreateCountRuleScript_Compile_ExpectRuleCreated()
        {
            // setup
            var grammarRule = SetupRule<MatchCount<int>>("root = [2..3]foo;");  
            
            // act - grammar 
            var fooToken = 42;
            ReplaceSubject(grammarRule, fooToken);

            // test
            ExpectTrue(grammarRule, [fooToken, fooToken], [fooToken, fooToken, fooToken]);
            ExpectFalse(grammarRule, [fooToken], [fooToken, 43, fooToken]);
        }

        [TestMethod]
        public void CreateEvaluationRuleScript_Compile_ExpectRuleCreated()
        {
            // setup
            var grammarRule = SetupRule<MatchEvaluation<int>>("root = foo / foo / foo;");

            // act - grammar 
            var fooToken = 42;
            ReplaceRules(grammarRule, 3, fooToken);

            // test
            ExpectTrue(grammarRule, [fooToken]);
            ExpectFalse(grammarRule, [], [43]);
        }

        [TestMethod]
        public void CreateFindRuleScript_Compile_ExpectRuleCreated()
        {
            // setup
            var grammarRule = SetupRule<SkipRule<int>>("root = find foo;");

            // act - grammar 
            var fooToken = 42;
            ReplaceSubject(grammarRule, fooToken);

            // test
            ExpectTrue(grammarRule, [fooToken], [43, fooToken]);
            ExpectFalse(grammarRule, [], [43]);
        }

        [TestMethod]
        public void CreateGroupRuleScript_Compile_ExpectRuleCreated()
        {
            // setup
            var grammarRule = SetupRule<MatchAnyData<int>>("root = (.);");

            // test
            ExpectTrue(grammarRule, [43]);
            ExpectFalse(grammarRule, []);
        }

        [TestMethod]
        public void CreateIfRuleScript_Compile_ExpectRuleCreated()
        {
            // setup
            var grammarRule = SetupRule<MatchCondition<int>>("root = if foo;");

            // act - grammar 
            var fooToken = 42;
            ReplaceSubject(grammarRule, fooToken);

            // test
            ExpectTrue(grammarRule, [fooToken]);
            ExpectFalse(grammarRule, [], [43]);

            IsTrue(grammarRule.Parse([42], 0));
            IsTrue(grammarRule.Parse([42], 0).MatchLength == 0);
            IsTrue(grammarRule.Parse([43], 0).MatchLength == 0);
        }

        [TestMethod]
        public void CreateLogRuleScript_Compile_ExpectRuleCreated()
        {
            TestLogRule(LogLevel.Info);
            TestLogRule(LogLevel.Debug);
            TestLogRule(LogLevel.Error);
            TestLogRule(LogLevel.Warning);

            try
            {
                TestLogRule(LogLevel.Fatal);
                Fail();
            }
            catch (Exception)
            {
            }
        }

        [TestMethod]
        public void CreateMatchOneOfRuleScript_Compile_ExpectRuleCreated()
        {
            // setup
            var grammarRule = SetupRule<MatchOneOf<int>>("root = a | b | c;");

            // act - grammar 
            var tokens = new int[] { 42, 43, 44 };
            ReplaceRules(grammarRule, tokens);

            // test
            ExpectTrue(grammarRule, [tokens[0]], [tokens[1]], [tokens[2]]);
            ExpectFalse(grammarRule, [], [40], [45]);
        }

        [TestMethod]
        public void CreateNotRuleScript_Compile_ExpectRuleCreated()
        {
            // setup
            var grammarRule = SetupRule<MatchNot<int>>("root = !foo;");

            // act - grammar 
            var fooToken = 42;
            ReplaceSubject(grammarRule, fooToken);

            // test
            ExpectTrue(grammarRule, [], [fooToken+1]);
            ExpectFalse(grammarRule, [fooToken]);

            IsTrue(grammarRule.Parse([43], 0).MatchLength == 0);
        }

        [TestMethod]
        public void CreateOneOrMoreRuleScript_Compile_ExpectRuleCreated()
        {
            // setup
            var grammarRule = SetupRule<MatchCount<int>>("root = +foo;");

            // act - grammar 
            var fooToken = 42;
            ReplaceSubject(grammarRule, fooToken);

            // test
            ExpectTrue(grammarRule, [fooToken], [fooToken, fooToken]);
            ExpectFalse(grammarRule, [fooToken + 1], []);

            IsTrue(grammarRule.Parse([fooToken], 0).MatchLength == 1);
            IsTrue(grammarRule.Parse([fooToken, fooToken], 0).MatchLength == 2);
        }

        [TestMethod]
        public void CreateReferenceRuleScript_Compile_ExpectRuleCreated()
        {
            // setup
            var grammarRule = SetupRule<RuleReference<int>>("root = foo;");

            // act - grammar 
            var fooToken = 42;
            ReplaceSubject(grammarRule, fooToken);

            // test
            ExpectTrue(grammarRule, [fooToken]);
            ExpectFalse(grammarRule, [fooToken + 1], []);
        }

        [TestMethod]
        public void CreateStopAfterRuleScript_Compile_ExpectRuleCreated()
        {
            // setup
            var grammarRule = SetupRule<SkipRule<int>>("root = stop_after foo;");

            // act - grammar 
            var fooToken = 42;
            ReplaceSubject(grammarRule, fooToken);

            // test
            ExpectTrue(grammarRule, [fooToken], [], [fooToken + 1]);

            IsTrue(grammarRule.Parse([fooToken, 1], 0).MatchLength == 1);
            IsTrue(grammarRule.Parse([43, fooToken], 0).MatchLength == 2);
            IsTrue(grammarRule.Parse([41, 43, 44], 0).MatchLength == 3);
        }

        [TestMethod]
        public void CreateStopAtRuleScript_Compile_ExpectRuleCreated()
        {
            // setup
            var grammarRule = SetupRule<SkipRule<int>>("root = stop_at foo;");

            // act - grammar 
            var fooToken = 42;
            ReplaceSubject(grammarRule, fooToken);

            // test
            ExpectTrue(grammarRule, [fooToken], [], [fooToken + 1]);

            IsTrue(grammarRule.Parse([fooToken, 1], 0).MatchLength == 0);
            IsTrue(grammarRule.Parse([43, fooToken], 0).MatchLength == 1);
            IsTrue(grammarRule.Parse([43, 44, fooToken], 0).MatchLength == 2);
            IsTrue(grammarRule.Parse([41, 43, 44], 0).MatchLength == 3);
        }

        [TestMethod]
        public void CreateSequenceAtRuleScript_Compile_ExpectRuleCreated()
        {
            // setup
            var grammarRule = SetupRule<MatchRuleSequence<int>>("root = a, b, c;");

            // act - grammar 
            var a = 42;
            var b = 43;
            var c = 44;
            ReplaceRules(grammarRule, [a, b, c]);

            // test
            ExpectTrue(grammarRule, [a, b, c]);
            ExpectFalse(grammarRule, [a, b], [], [a,c]);
        }

        [TestMethod]
        public void CreateZeroOrMoreRuleScript_Compile_ExpectRuleCreated()
        {
            // setup
            var grammarRule = SetupRule<MatchCount<int>>("root = *foo;");

            // act - grammar 
            var fooToken = 42;
            ReplaceSubject(grammarRule, fooToken);

            // test
            ExpectTrue(grammarRule, [], [fooToken, fooToken], [fooToken + 1]);

            IsTrue(grammarRule.Parse([], 0).MatchLength == 0);
            IsTrue(grammarRule.Parse([fooToken], 0).MatchLength == 1);
            IsTrue(grammarRule.Parse([fooToken, fooToken], 0).MatchLength == 2);
        }

        [TestMethod]
        public void CreateZeroOrOneRuleScript_Compile_ExpectRuleCreated()
        {
            // setup
            var grammarRule = SetupRule<MatchCount<int>>("root = ?foo;");

            // act - grammar 
            var fooToken = 42;
            ReplaceSubject(grammarRule, fooToken);

            // test
            ExpectTrue(grammarRule, [], [fooToken, fooToken], [fooToken + 1]);

            IsTrue(grammarRule.Parse([], 0).MatchLength == 0);
            IsTrue(grammarRule.Parse([fooToken], 0).MatchLength == 1);
            IsTrue(grammarRule.Parse([fooToken, fooToken], 0).MatchLength == 1);
        }

        // -- Private methods -----------------------------------------------------------------------------------------

        private static void ExpectTrue(IRule rule, params int[][] input)
        {
            input.ForEach(i =>
            {
                var result = rule.Parse(i, 0);
                IsTrue(result);
                IsTrue(result.Count == 1);
                IsTrue(result[0].Rule == rule);
            });
        }

        private static void ExpectFalse(IRule rule, params int[][] input)
        {
            input.ForEach(i => IsFalse(rule.Parse(i, 0)));
        }

        private static T SetupRule<T>(string grammarScript) where T : IRule
        {
            var (tokens, syntaxTree) = new ScriptParser().Parse(grammarScript);
            var session = new RuleCompilationContext(grammarScript, tokens, syntaxTree);

            var grammarCompiler = new GrammarCompiler();
            var grammarRule = grammarCompiler.Compile<T>(syntaxTree[0], session);

            IsNotNull(grammarRule);

            return grammarRule;
        }

        /// the rule may be pointing to a rule that doesn't exist, as we don't resolve
        /// references so set the subject manually
        /// otherwise this will fail with an exception.
        private static IRule ReplaceSubject(IMetaRule rule, int tokenId = 42, IRule subject = null)
        {
            var replacement = subject ?? new MatchSingleData<int>("foo", tokenId);

            rule.MutateSubject(replacement);

            return replacement;
        }

        /// the rule may be pointing to a rule that doesn't exist, as we don't resolve
        /// references so set the subject manually
        /// otherwise this will fail with an exception.
        private static IEnumerable<IRule> ReplaceRules(
            IRuleComposition rule, 
            params int[] tokenIds
        )
        {
            rule.MutateComposition(
                new List<IRule>().Fill(
                    idx => new MatchSingleData<int>("foo", tokenIds[idx]), 
                    tokenIds.Length
                )
            );

            return rule.Rules;
        }

        private static IEnumerable<IRule> ReplaceRules(
            IRuleComposition rule,
            int count,
            int tokenId
        )
        {
            rule.MutateComposition(
                new List<IRule>()
                    .Fill(new MatchSingleData<int>("foo", tokenId), count));

            return rule.Rules;
        }

        private static void TestLogRule(LogLevel level)
        {
            // setup
            var grammarRule = SetupRule<LogRule<int>>($"root = {level.ToString().ToLower()} 'foo';");

            // test
            ExpectTrue(grammarRule, [-1]);

            var result = grammarRule.Parse([42], 0);

            IsTrue(result);
            IsTrue(result.MatchLength == 0);

            var logRule = result[0].Rule as LogRule<int>;

            IsNotNull(logRule);
            IsTrue(logRule.Text == "foo");
            IsTrue(logRule.Level == level);
        }
    }
}
