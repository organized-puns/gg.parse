using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

using gg.parse.script.common;

namespace gg.parse.script.tests.unit
{
    [TestClass]
    public class CommonTokenizerTests
    {
        [TestMethod]
        public void DigitSequenceTokenTests()
        {
            RunTokenizerTest(
                ruleFactory: (tokenizer, name) => tokenizer.DigitSequence(name),
                testNames: [null, "digitSequence", "~digitSequence", "#digitSequence"],
                validSamples: ["1", "0123", "9876543210"],
                invalidSamples: ["", "a123", "$123", " 123"]
            );
        }

        [TestMethod]
        public void IdentifierTokenTests()
        {
            RunTokenizerTest(
                ruleFactory:    (tokenizer, name) => tokenizer.Identifier(name),
                testNames:      [null, "identifier", "~identifier", "#identifier"],
                validSamples:   ["foo", "_bar", "A123_abcd09"],
                invalidSamples: ["", "123abc", "$abc", " foo"]
            );
        }

        [TestMethod]
        public void IntegerTokenTests()
        {
            RunTokenizerTest(
                ruleFactory: (tokenizer, name) => tokenizer.Integer(name),
                testNames: [null, "int", "~int", "#int"],
                validSamples: ["123", "-123", "1", "-009", "+123456789"],
                invalidSamples: ["", "*123", "a123", "_000", " 123"]
            );
        }

        [TestMethod]
        public void KeywordTests()
        {
            RunTokenizerTest(
                ruleFactory: (tokenizer, name) => tokenizer.Keyword(name, "keyword"),
                testNames: [null, "keywordToken", "~keywordToken", "#keywordToken"],
                validSamples: ["keyword", "keyword ", "keyword!", "keyword("],
                invalidSamples: ["", "kyword", "keywords", " keyword"]
            );
        }

        [TestMethod]
        public void MatchStringTests()
        {
            RunTokenizerTest(
                ruleFactory: (tokenizer, name) => tokenizer.MatchString(name, '\''),
                testNames: [null, "string", "#string", "~string" ],
                validSamples: ["''", "'str''", "'str'", "'\\'str\\''"],
                invalidSamples: ["", "'str", "'\\'", " 'str''"]
            );
        }

        [TestMethod]
        public void MultiLineCommentTests()
        {
            RunTokenizerTest(
                ruleFactory: (tokenizer, name) => tokenizer.MultiLineComment(name),
                testNames: [null, "comment", "#comment", "~comment"],
                validSamples: ["/* foo */", "/**/", "/** // */", "/** */"],
                invalidSamples: ["", "/*", "/*/"]
            );
        }

        [TestMethod]
        public void SingleLineCommentTests()
        {
            RunTokenizerTest(
                ruleFactory: (tokenizer, name) => tokenizer.SingleLineComment(name),
                testNames: [null, "comment", "#comment", "~comment"],
                validSamples: ["// foo", "//bar\n", "//"],
                invalidSamples: ["", "/foo", "/"]
            );
        }

        // -- private / utility functions -----------------------------------------------------------------------------

        private void RunTokenizerTest(
            Func<CommonTokenizer, string, RuleBase<char>> ruleFactory, 
            string[] testNames,
            string[] validSamples, 
            string[] invalidSamples
        )
        {
            foreach (var name in testNames)
            {
                var tokenizer = new CommonTokenizer();
                var rule = ruleFactory(tokenizer, name);
                var ruleCount = tokenizer.Count;

                if (name != null)
                {
                    var (expectedName, production) = name.SplitNameAndOutput();
                    
                    IsTrue(rule.Name == expectedName);
                    IsTrue(rule.Production == production);
                }
                else
                {
                    // no name provided, there should be a non null default name
                    IsNotNull(rule.Name);
                    IsTrue(rule.Production == IRule.Output.Void);
                }

                IsTrue(tokenizer.FindRule(rule.Name) == rule);
                IsTrue(tokenizer.FindRule(rule.Id) == rule);

                // invoke the rule again - make sure the rule is not added again
                ruleFactory(tokenizer, name);

                IsTrue(ruleCount == tokenizer.Count);

                // invoke the rule with a different name - this rule should be added
                ruleFactory(tokenizer, "differentName");

                IsTrue(ruleCount < tokenizer.Count);

                // go over the valid samples
                foreach (var sample in validSamples)
                {
                    var result = rule.Parse(sample);

                    IsTrue(result.FoundMatch);
                    IsTrue(result.MatchLength > 0);

                    if (rule.Production == IRule.Output.Void)
                    {
                        IsTrue(result.Annotations == null);
                    }
                }

                // go over the valid samples
                foreach (var sample in invalidSamples)
                {
                    var result = rule.Parse(sample);

                    IsTrue(!result.FoundMatch);
                }
            }
        }
    }
}
