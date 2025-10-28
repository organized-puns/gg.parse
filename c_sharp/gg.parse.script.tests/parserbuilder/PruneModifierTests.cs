using gg.parse.rules;
using gg.parse.script.parser;

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
            );

            var root = parser.TokenGraph.FindRule("root") as MatchRuleSequence<char>;

            // validate the run and its outputs
            IsTrue(root != null);

            IsTrue((root[0] as RuleReference<char>).Reference == "foo");
            IsTrue(root[0].Prune == AnnotationPruning.None);

            IsTrue((root[1] as RuleReference<char>).Reference == "foo");
            IsTrue(root[1].Prune == AnnotationPruning.Root);

            IsTrue((root[2] as RuleReference<char>).Reference == "foo");
            IsTrue(root[2].Prune == AnnotationPruning.All);

            IsTrue((root[3] as RuleReference<char>).Reference == "foo");
            IsTrue(root[3].Prune == AnnotationPruning.Children);

            var (result, _) = parser.Parse("foofoofoofoo");

            IsTrue(result);
            IsTrue(result.MatchLength == 12);

            // the only foo to show up is the first and last one, the second returns the children of foo 
            // which a data sequence doesn't have and the third will drop the results (void)
            IsTrue(result[0].Count == 2);           
        }

        [TestMethod]
        public void SetupReferencesWithDifferentRuleOutputs_ParseInput_ExpectRulesToMatchSpecifiedOutput()
        {
            var parser = new ParserBuilder().From($"{pr}tokens=*(foo|bar);foo='foo';bar='bar';", $"root=foo, {pr}bar, {pa}foo;");

            var root = parser.GrammarGraph.FindRule("root") as MatchRuleSequence<int>;

            // validate the run and its outputs
            IsTrue(root != null);

            IsTrue((root[0] as RuleReference<int>).Reference == "foo");
            IsTrue(root[0].Prune == AnnotationPruning.None);

            IsTrue((root[1] as RuleReference<int>).Reference == "bar");
            IsTrue(root[1].Prune == AnnotationPruning.Root);

            IsTrue((root[2] as RuleReference<int>).Reference == "foo");
            IsTrue(root[2].Prune == AnnotationPruning.All);

            var (_, syntaxTree) = parser.Parse("foobarfoo");

            IsTrue(syntaxTree);

            IsTrue(syntaxTree.Count == 1);

            // the only foo to show up is the first one, the second returns the children of foo 
            // which a data sequence doesn't have and the third will drop the results (void)
            IsTrue(syntaxTree[0].Count == 1);
        }

        [TestMethod]
        public void XSetupReferencesWithDifferentRuleOutputs_TokenizeInput_ExpectRulesToMatchSpecifiedOutput()
        {
            var parser = new ParserBuilder().From(
                $"root = foo, {pr}foo, {pa}foo;" +
                "foo = 'foo' | 'bar';"
            );

            var root = parser.TokenGraph.FindRule("root") as MatchRuleSequence<char>;

            // validate the run and its outputs
            IsTrue(root != null);

            IsTrue((root[0] as RuleReference<char>).Reference == "foo");
            IsTrue(root[0].Prune == AnnotationPruning.None);

            IsTrue((root[1] as RuleReference<char>).Reference == "foo");
            IsTrue(root[1].Prune == AnnotationPruning.Root);

            IsTrue((root[2] as RuleReference<char>).Reference == "foo");
            IsTrue(root[2].Prune == AnnotationPruning.All);

            var (result, _) = parser.Parse("foobarfoo");

            IsTrue(result);
            IsTrue(result.MatchLength == 9);
            IsTrue(result[0].Count == 2);
            // the first foo is declared with (implicit) 'self' in root, so the foo rule should appear
            // in the result
            IsTrue(result[0][0].Rule is MatchOneOf<char>);

            // the second foo is declared with 'children' in root, so the foo literal should appear
            // in the result
            IsTrue(result[0][1].Rule is MatchDataSequence<char>);
        }

        /// <summary>
        /// Inline count rules are were created incorrectly with their subrules set to prune root. Instead
        /// it was fixed so they return with subrule pruning set to none. This verifies that fix.
        /// </summary>
        [TestMethod]
        public void CreateCountDataRuleInsideBinaryRule_ParseWithMatchingInput_ExpectCorrectProductionModifiers()
        {
            var builder = new ParserBuilder().From("sequence = +'foo', +'bar';");
            var (result, _) = builder.Parse("foobar");

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
            var builder = new ParserBuilder().From("sequence = +('fooz' | 'foo'), +('barz' | 'bar');");
            var (result, _) = builder.Parse("foozbarz");

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
            var builder = new ParserBuilder().From("sequence = !'bar', 'foo';");
            var (result, _) = builder.Parse("foo");

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
            var builder = new ParserBuilder().From("sequence = not_bar, 'foo'; not_bar = !'bar';");
            var (result, _) = builder.Parse("foo");

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
    }
}
