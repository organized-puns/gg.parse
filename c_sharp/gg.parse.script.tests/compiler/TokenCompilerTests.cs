// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.rules;
using gg.parse.script.common;
using gg.parse.script.compiler;
using gg.parse.script.parser;
using gg.parse.util;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using static gg.parse.tests.TestAnnotation;

namespace gg.parse.script.tests.compiler
{
    [TestClass]
    public class TokenCompilerTests
    {
        [TestMethod]
        public void CreateAnyRuleAnnotation_Compile_ExpectRuleCreated()
        {
            DataRuleTest<MatchAnyData<char>>(
                CreateAnnotationTree(CommonTokenNames.AnyCharacter, "."),
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
            var tree = CreateAnnotationTree(CommonTokenNames.Literal, script);

            DataRuleTest<MatchDataSequence<char>>(
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
                    CommonTokenNames.Set,
                    script,
                    NewAnnotation(CommonTokenNames.Literal, 5, set.Length)
                );

            DataRuleTest<MatchDataSet<char>>(
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
                    CommonTokenNames.DataRange,
                    script,
                    NewAnnotation(CommonTokenNames.Literal, 5, min.Length),
                    NewAnnotation(CommonTokenNames.Literal, 10, max.Length)
                );

            DataRuleTest<MatchDataRange<char>>(
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

        // -- Private methods -----------------------------------------------------------------------------------------

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
                    NewAnnotation(CommonTokenNames.Literal, 4, a.Length),
                    NewAnnotation(CommonTokenNames.Literal, 4 + separator.Length + a.Length, b.Length),
                    NewAnnotation(CommonTokenNames.Literal, 4 + 2 * separator.Length + a.Length + b.Length, c.Length)
                );

            DataRuleTest<T>(tree, script, validCases, invalidCases);
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

        private static void DataRuleTest<T>(
            Annotation root,
            string dataRuleScript, 
            string[] validCases,
            string[] invalidCases) where T : IRule
        {
            var session = new RuleCompilationContext($"foo={dataRuleScript};", [root]);

            var compiledRule = new TokenizerCompiler().Compile<T>(root, session);

            IsFalse(session.Logs.Contains(LogLevel.Error | LogLevel.Fatal));
            IsNotNull(compiledRule);
            IsTrue(compiledRule.Name == "foo");

            validCases.ForEach(input => IsTrue(compiledRule.Parse(input)));
            invalidCases.ForEach(input => IsFalse(compiledRule.Parse(input)));
        }
    }
}
