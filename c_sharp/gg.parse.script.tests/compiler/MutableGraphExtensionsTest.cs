// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.rules;
using gg.parse.script.compiler;
using gg.parse.script.parser;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.script.tests.compiler
{
    [TestClass]
    public class MutableGraphExtensionsTest
    {
        [TestMethod]
        public void CompileSyntaxTreeWithReference_ResolveReferences_ExpectReferencesResolved()
        {
            // setup
            var graph = SetupReferenceTest("root = foo; foo = .;");

            // verify root is a reference that hasn't been resolved yet
            var root = (RuleReference<int>)graph["root"];
            IsTrue(root.ReferenceName == "foo");
            IsTrue(root.Subject == null);

            // act
            graph.ResolveReferences();

            // test
            IsTrue(root.Subject == graph["foo"]);
        }

        [TestMethod]
        public void CompileSyntaxTreeWithMetaReference_ResolveReferences_ExpectReferencesResolved()
        {
            // setup
            var graph = SetupReferenceTest("root = !foo; foo = .;");

            // verify root is a reference that hasn't been resolved yet
            var root = (RuleReference<int>)((MatchNot<int>)graph["root"]).Subject;
            IsTrue(root.ReferenceName == "foo");
            IsTrue(root.Subject == null);

            // act
            graph.ResolveReferences();

            // test
            IsTrue(root.Subject == graph["foo"]);
        }

        [TestMethod]
        public void CompileSyntaxTreeRuleCompisition_ResolveReferences_ExpectReferencesResolved()
        {
            // setup
            var graph = SetupReferenceTest("root = foo, foo, foo; foo = .;");

            // verify root is a reference that hasn't been resolved yet
            var subRules = ((MatchRuleSequence<int>)graph["root"]).Rules;

            foreach (var rule in subRules.Cast<RuleReference<int>>())
            {
                IsTrue(rule.ReferenceName == "foo");
                IsTrue(rule.Subject == null);
            }

            // act
            graph.ResolveReferences();

            // test
            foreach (var rule in subRules.Cast<RuleReference<int>>())
            {
                IsTrue(rule.Subject == graph["foo"]);
            }
        }

        [TestMethod]
        public void CompileMissing_ResolveReferences_ExpectException()
        {
            // setup
            var graph = SetupReferenceTest("root = foo;");
            var subRules = (RuleReference<int>)graph["root"];

            // act
            try
            {
                graph.ResolveReferences();
                Fail();
            }
            catch (KeyNotFoundException)
            {
            }
        }

        private static MutableRuleGraph<int> SetupReferenceTest(string script)
        {
            var (tokens, syntaxTree) = new ScriptParser().Parse(script);
            var session = new RuleCompilationContext(script, tokens, syntaxTree);

            var grammarCompiler = new GrammarCompiler();
            var graph = new MutableRuleGraph<int>();

            grammarCompiler.Compile(null, session, graph);

            return graph;
        }

    }
}
