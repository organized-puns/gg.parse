using gg.parse.core;
using gg.parse.rules;
using gg.parse.script.parser;
using System.Diagnostics;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.script.tests.parserbuilder
{
    [TestClass]
    public class PruneModifierTests
    {
        // short hands for annotation pruning tokens
        private const string pa = AnnotationPruningToken.All;
        private const string pc = AnnotationPruningToken.Children;
        private const string pr = AnnotationPruningToken.Root;

        [TestMethod]
        public void SetupReferencesWithDifferentRuleOutputs_TokenizeInput_ExpectRulesToMatchSpecifiedOutput()
        {
            var parser = new ParserBuilder().From(
                $"root = foo, {pr}foo, {pa}foo, {pc}foo;" +
                "foo = 'foo';"
            ).Build();

            var root = parser.Tokens["root"] as MatchRuleSequence<char>;

            // validate the run and its outputs
            IsTrue(root != null);

            var foo1 = root[0] as RuleReference<char>;
            IsTrue(foo1.ReferenceName == "foo");
            IsTrue(foo1.Prune == AnnotationPruning.None);
            IsTrue(foo1.ReferencePrune == AnnotationPruning.None);

            var foo2 = root[1] as RuleReference<char>;
            IsTrue(foo2.ReferenceName == "foo");
            IsTrue(foo2.Prune == AnnotationPruning.None);
            IsTrue(foo2.ReferencePrune == AnnotationPruning.Root);

            var foo3 = root[2] as RuleReference<char>;
            IsTrue(foo3.ReferenceName == "foo");
            IsTrue(foo3.Prune == AnnotationPruning.None);
            IsTrue(foo3.ReferencePrune == AnnotationPruning.All);

            var foo4 = root[3] as RuleReference<char>;
            IsTrue(foo4.ReferenceName == "foo");
            IsTrue(foo4.Prune == AnnotationPruning.None);
            IsTrue(foo4.ReferencePrune == AnnotationPruning.Children);

            var result = parser.Tokenize("foofoofoofoo");

            IsTrue(result);
            IsTrue(result.MatchLength == 12);

            // the only foo to show up is the first and last one, the second returns the children of foo 
            // which a data sequence doesn't have and the third will drop the results (void)
            IsTrue(result[0].Count == 2);           
        }

        [TestMethod]
        public void SetupReferencesWithDifferentRuleOutputs_ParseInput_ExpectRulesToMatchSpecifiedOutput()
        {
            var parser = new ParserBuilder()
                .From($"{pr}tokens=*(foo|bar);foo='foo';bar='bar';", $"root=foo, {pr}bar, {pa}foo;")
                .Build();

            var root = parser.Grammar["root"] as MatchRuleSequence<int>;

            // validate the run and its outputs
            IsTrue(root != null);

            var fooRule1 = root[0] as RuleReference<int>;
            IsTrue(fooRule1.ReferenceName == "foo");
            IsTrue(fooRule1.ReferencePrune == AnnotationPruning.None);

            var barRule = root[1] as RuleReference<int>;
            IsTrue(barRule.ReferenceName == "bar");
            IsTrue(barRule.ReferencePrune == AnnotationPruning.Root);

            var fooRule2 = root[2] as RuleReference<int>;
            IsTrue(fooRule2.ReferenceName == "foo");
            IsTrue(fooRule2.ReferencePrune == AnnotationPruning.All);

            var (_, syntaxTree) = parser.Parse("foobarfoo");

            IsTrue(syntaxTree);

            IsTrue(syntaxTree.Count == 1);

            // the only foo to show up is the first one, the second returns the children of foo 
            // which a data sequence doesn't have and the third will drop the results (void)
            IsTrue(syntaxTree[0].Count == 1);
        }

        /// <summary>
        /// Inline count rules are were created incorrectly with their subrules set to prune root. Instead
        /// it was fixed so they return with subrule pruning set to none. This verifies that fix.
        /// </summary>
        [TestMethod]
        public void CreateCountDataRuleInsideBinaryRule_ParseWithMatchingInput_ExpectCorrectProductionModifiers()
        {
            var builder = new ParserBuilder().From("sequence = +'foo', +'bar';").Build();
            var result = builder.Tokenize("foobar");

            IsTrue(result);

            // named rule should be a sequence
            IsTrue(result[0].Rule is MatchRuleSequence<char>);

            // + rule should be pruned, foo and bar should be added
            IsTrue(result[0].Count == 2);
            IsTrue(result[0][0].Rule is MatchDataSequence<char>);
            IsTrue(result[0][1].Rule is MatchDataSequence<char>);
        }

        /// <summary>
        /// Extension of the previous test where there is a binary inside the count. 
        /// Count should pass 'none' pruning to its children, since it's child is an inline binary
        /// function, this should set its pruning to root, so in the end we're left with just the literals.
        /// </summary>
        [TestMethod]
        public void CreateCountBinaryRule_ParseWithMatchingInput_ExpectCorrectProductionModifiers()
        {
            var builder = new ParserBuilder()
                .From("sequence = +('fooz' | 'foo'), +('barz' | 'bar');")
                .Build();
            var result = builder.Tokenize("foozbarz");

            IsTrue(result);

            // named rule should be a sequence
            IsTrue(result[0].Rule is MatchRuleSequence<char>);

            // + rule should be pruned, foo and bar should be added
            IsTrue(result[0].Count == 2);
            IsTrue(result[0][0].Rule is MatchDataSequence<char>);
            IsTrue(result[0][1].Rule is MatchDataSequence<char>);
        }

        /// <summary>
        /// Lookahead rules should be pruned by default if it's inlined.
        /// </summary>
        [TestMethod]
        public void CreateInlineLookaheadRule_ParseWithMatchingInput_ExpectCorrectProductionModifiers()
        {
            var builder = new ParserBuilder().From("sequence = !'bar', 'foo';").Build();
            var result = builder.Tokenize("foo");

            IsTrue(result);

            // named rule should be a sequence
            IsTrue(result[0].Rule is MatchRuleSequence<char>);

            // !'bar' rule should be pruned, only foo should remain
            IsTrue(result[0].Count == 1);
            IsTrue(result[0][0].Rule is MatchDataSequence<char>);
        }

        /// <summary>
        /// Lookahead rules should be pruned by default unless it's a toplevel rule.
        /// </summary>
        [TestMethod]
        public void CreateTopLevelLookaheadRule_ParseWithMatchingInput_ExpectCorrectProductionModifiers()
        {
            var builder = new ParserBuilder().From("sequence = not_bar, 'foo'; not_bar = !'bar';").Build();
            var result = builder.Tokenize("foo");

            IsTrue(result);

            // named rule should be a sequence
            IsTrue(result[0].Rule is MatchRuleSequence<char>);

            // !'bar' rule should be pruned, only foo should remain
            IsTrue(result[0].Count == 2);

            // should contain !bar
            IsTrue(result[0][0].Rule is MatchNot<char>);

            // 'foo'
            IsTrue(result[0][1].Rule is MatchDataSequence<char>);
        }

        /// <summary>
        /// Rule reference can have modifiers both on the rule header and in the rule body.
        /// When defined as a top level rule, this can get complicated as we get a cartesian product
        /// of the header's modifier and the body's modifier. This test verifies all combinations work as expected.
        /// </summary>
        [TestMethod]
        public void CreateTopLevelRuleReference_ParseWithMatchingInput_ExpectCorrectProductionModifiers()
        {
            var modifiers = new (string root, string options)[]
            {
                ("", ""),
                ("", "-a"),
                ("", "-r"),
                ("", "-c"),

                ("-r", ""),
                ("-r", "-a"),
                ("-r", "-r"),
                ("-r", "-c"),

                ("-a", ""),

                ("-c", ""),
                ("-c", "-a"),
                ("-c", "-r"),
                ("-c", "-c"),
            };

            var validateResult = new Action<ParseResult>[]
            {
                // 0. ("", ""),
                (result) => 
                {
                    IsTrue(result[0] == "root");
                    IsTrue(result[0][0] == "options");
                    IsTrue(result[0][0][0].Rule is MatchDataSequence<char>);
                },

                // 1. ("", "-a"),
                (result) =>
                {
                    IsTrue(result[0] == "root");
                    IsTrue(result[0].Count == 0);
                },

                // 2. ("", "-r"),
                (result) =>
                {
                    IsTrue(result[0] == "root");
                    IsTrue(result[0][0].Rule is MatchDataSequence<char>);
                },

                // 3. ("", "-c"),
                (result) =>
                {
                    IsTrue(result[0] == "root");
                    IsTrue(result[0][0] == "options");
                    IsTrue(result[0][0].Count == 0);
                },

                // 4. ("-r", ""),
                (result) =>
                {
                    IsTrue(result[0] == "options");
                    IsTrue(result[0][0].Rule is MatchDataSequence<char>);
                },

                // 5. ("-r", "-a"),
                (result) =>
                {
                    IsTrue(result.Count == 0);
                },

                // 6. ("-r", "-r"),
                (result) =>
                {
                    IsTrue(result[0].Rule is MatchDataSequence<char>);
                },

                // 6. ("-r", "-c"),
                (result) =>
                {
                    IsTrue(result[0] == "options");
                    IsTrue(result[0].Count == 0);
                },

                // 7. ("-a", ""),
                (result) =>
                {
                    IsTrue(result.Annotations == null);
                },

                // 8. ("-c", ""),
                (result) =>
                {
                    IsTrue(result[0] == "root");
                    IsTrue(result[0].Count == 0);
                },

                // 9. ("-c", "-a"),
                (result) =>
                {
                    IsTrue(result[0] == "root");
                    IsTrue(result[0].Count == 0);
                },

                // 10. ("-c", "-r"),
                (result) =>
                {
                    IsTrue(result[0] == "root");
                    IsTrue(result[0].Count == 0);
                },

                // 11. ("-c", "-c"),
                (result) =>
                {
                    IsTrue(result[0] == "root");
                    IsTrue(result[0].Count == 0);
                },
            };

            for (var i = 0; i < modifiers.Length; i++)
            {
                var (pruneRoot, pruneOptions) = modifiers[i];
                var parser = new ParserBuilder()
                    .From($"{pruneRoot} root = {pruneOptions} options; options = 'a' | 'b';")
                    .Build();
                var result = parser.Tokenize("a");

                IsTrue(result);

                Debug.WriteLine($"{i} '{pruneRoot}', '{pruneOptions}':\n{ScriptUtils.PrettyPrintTokens("a", result.Annotations)}");

                validateResult[i](result);
            }
        }
    }
}
