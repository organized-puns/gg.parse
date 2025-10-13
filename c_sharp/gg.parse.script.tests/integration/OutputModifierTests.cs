using System.Diagnostics;

using gg.parse.rules;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.script.tests.integration
{
    [TestClass]
    public class OutputModifierTests
    {
        [TestMethod]
        public void SetupReferencesWithDifferentRuleOutputs_TokenizeInput_ExpectRulesToMatchSpecifiedOutput()
        {
            var parser = new ParserBuilder().From(
                "root = foo, #foo, ~foo;" +
                "foo = 'foo';"
            );

            var root = parser.TokenGraph.FindRule("root") as MatchRuleSequence<char>;

            // validate the run and its outputs
            IsTrue(root != null);

            IsTrue((root[0] as RuleReference<char>).Reference == "foo");
            IsTrue(root[0].Output == RuleOutput.Self);

            IsTrue((root[1] as RuleReference<char>).Reference == "foo");
            IsTrue(root[1].Output == RuleOutput.Children);

            IsTrue((root[2] as RuleReference<char>).Reference == "foo");
            IsTrue(root[2].Output == RuleOutput.Void);

            var (result, _) = parser.Parse("foofoofoo");

            IsTrue(result);
            IsTrue(result.MatchLength == 9);

            // the only foo to show up is the first one, the second returns the children of foo 
            // which a data sequence doesn't have and the third will drop the results (void)
            IsTrue(result[0].Count == 1);           
        }

        [TestMethod]
        public void SetupReferencesWithDifferentRuleOutputs_ParseInput_ExpectRulesToMatchSpecifiedOutput()
        {
            var parser = new ParserBuilder().From($"#tokens=*(foo|bar);foo='foo';bar='bar';", "root=foo, #bar, ~foo;");

            var root = parser.GrammarGraph.FindRule("root") as MatchRuleSequence<int>;

            // validate the run and its outputs
            IsTrue(root != null);

            IsTrue((root[0] as RuleReference<int>).Reference == "foo");
            IsTrue(root[0].Output == RuleOutput.Self);

            IsTrue((root[1] as RuleReference<int>).Reference == "bar");
            IsTrue(root[1].Output == RuleOutput.Children);

            IsTrue((root[2] as RuleReference<int>).Reference == "foo");
            IsTrue(root[2].Output == RuleOutput.Void);

            var result = parser.Parse("foobarfoo");

            IsTrue(result.syntaxTree);

            IsTrue(result.syntaxTree.Count == 1);

            // the only foo to show up is the first one, the second returns the children of foo 
            // which a data sequence doesn't have and the third will drop the results (void)
            IsTrue(result.syntaxTree[0].Count == 1);
        }

        [TestMethod]
        public void XSetupReferencesWithDifferentRuleOutputs_TokenizeInput_ExpectRulesToMatchSpecifiedOutput()
        {
            var parser = new ParserBuilder().From(
                "root = foo, #foo, ~foo;" +
                "foo = 'foo' | 'bar';"
            );

            var root = parser.TokenGraph.FindRule("root") as MatchRuleSequence<char>;

            // validate the run and its outputs
            IsTrue(root != null);

            IsTrue((root[0] as RuleReference<char>).Reference == "foo");
            IsTrue(root[0].Output == RuleOutput.Self);

            IsTrue((root[1] as RuleReference<char>).Reference == "foo");
            IsTrue(root[1].Output == RuleOutput.Children);

            IsTrue((root[2] as RuleReference<char>).Reference == "foo");
            IsTrue(root[2].Output == RuleOutput.Void);

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
    }
}
