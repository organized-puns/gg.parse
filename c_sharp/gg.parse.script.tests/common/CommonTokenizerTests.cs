using gg.parse.script.common;

using gg.parse.script.parser;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.script.tests.common
{
    /// <summary>
    /// Go over the common tokenizer rules and validate they behave as expected.
    /// </summary>
    [TestClass]
    public class CommonTokenizerTests
    {

        [TestMethod]
        public void DigitSequenceTokenTests()
        {
            RunTokenizerTest(
                ruleFactory: (tokenizer, name) => tokenizer.DigitSequence(name),
                testNames: GenerateTestNames("digitSequence"),
                validSamples: ["1", "0123", "9876543210"],
                invalidSamples: ["", "a123", "$123", " 123"]
            );
        }

        [TestMethod]
        public void BooleanTokenTests()
        {
            RunTokenizerTest(
                ruleFactory: (tokenizer, name) => tokenizer.Boolean(name),
                testNames: GenerateTestNames("bool"),
                validSamples: ["true", "false"],
                invalidSamples: ["", " T", "True", " false"]
            );
        }

        [TestMethod]
        public void IdentifierTokenTests()
        {
            RunTokenizerTest(
                ruleFactory:    (tokenizer, name) => tokenizer.Identifier(name),
                testNames:      GenerateTestNames("identifier"),
                validSamples:   ["foo", "_bar", "A123_abcd09"],
                invalidSamples: ["", "123abc", "$abc", " foo"]
            );
        }

        [TestMethod]
        public void IntegerTokenTests()
        {
            RunTokenizerTest(
                ruleFactory: (tokenizer, name) => tokenizer.Integer(name),
                testNames: GenerateTestNames("int"),
                validSamples: ["123", "-123", "1", "-009", "+123456789"],
                invalidSamples: ["", "*123", "a123", "_000", " 123"]
            );
        }

        [TestMethod]
        public void FloatTokenTests()
        {
            RunTokenizerTest(
                ruleFactory: (tokenizer, name) => tokenizer.Float(name),
                testNames: GenerateTestNames("float"),
                validSamples: ["123.0", "-123.1", "1e3", "-2.0E-43", "+12345.6789", "123.3E+3"],
                invalidSamples: ["", "*123.2", "a123.3", "_00.0", "123.", "123.3e", "123.3E+x"]
            );
        }

        [TestMethod]
        public void LiteralTokenTests()
        {
            RunTokenizerTest(
                ruleFactory: (tokenizer, name) => tokenizer.Literal(name, "foo"),
                testNames: GenerateTestNames("literal"),
                validSamples: ["foo"],
                invalidSamples: ["", "*foo", "Foo", "fo", "bar"]
            );
        }

        [TestMethod]
        public void KeywordTests()
        {
            RunTokenizerTest(
                ruleFactory: (tokenizer, name) => tokenizer.Keyword(name, "keyword"),
                testNames: GenerateTestNames("keywordToken"),
                validSamples: ["keyword", "keyword ", "keyword!", "keyword("],
                invalidSamples: ["", "kyword", "keywords", " keyword"]
            );
        }

        [TestMethod]
        public void MatchStringTests()
        {
            RunTokenizerTest(
                ruleFactory: (tokenizer, name) => tokenizer.MatchString(name, '\''),
                testNames: GenerateTestNames("string"),
                validSamples: ["''", "'str''", "'str'", "'\\'str\\''", "'\\\\'", "'\\abc'" ],
                invalidSamples: ["", "'str", "'\\'", " 'str''"]
            );
        }


        [TestMethod]
        public void MultiLineCommentTests()
        {
            RunTokenizerTest(
                ruleFactory: (tokenizer, name) => tokenizer.MultiLineComment(name),
                testNames: GenerateTestNames("comment"),
                validSamples: ["/* foo */", "/**/", "/** // */", "/** */"],
                invalidSamples: ["", "/*", "/*/"]
            );
        }

        [TestMethod]
        public void SingleLineCommentTests()
        {
            RunTokenizerTest(
                ruleFactory: (tokenizer, name) => tokenizer.SingleLineComment(name),
                testNames: GenerateTestNames("comment"),
                validSamples: ["// foo", "//bar\n", "//"],
                invalidSamples: ["", "/foo", "/"]
            );
        }

        // -- private / utility functions -----------------------------------------------------------------------------

        private static string[] GenerateTestNames(string baseName) =>
            [
                null,
                baseName,
                AnnotationPruningToken.None + baseName,
                AnnotationPruningToken.All + baseName,
                AnnotationPruningToken.Children + baseName,
                AnnotationPruningToken.Root + baseName
            ];

        private static void ValidatePruning(RuleBase<char> rule, string name)
        {
            // validate the implicit pruning is correct
            if (name != null)
            {
                if (name.StartsWith(AnnotationPruningToken.All))
                {
                    IsTrue(rule.Prune == AnnotationPruning.All);
                }
                else if (name.StartsWith(AnnotationPruningToken.Children))
                {
                    IsTrue(rule.Prune == AnnotationPruning.Children);
                }
                else if (name.StartsWith(AnnotationPruningToken.Root))
                {
                    IsTrue(rule.Prune == AnnotationPruning.Root);
                }
                else if ((name[0] >= 'a' && name[0] <= 'z') || (name[0] >= 'A' && name[0] <= 'Z'))
                {
                    IsTrue(rule.Prune == AnnotationPruning.None);
                }
                else
                {
                    Fail();
                }
            }
            else
            {
                // no name is provided, it's therefore considered unnamed / anonymous so pruning should default to All
                IsTrue(rule.Prune == AnnotationPruning.All);
            }
        }

        private static void RunTokenizerTest(
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
                    var (expectedName, output) = name.SplitNameAndPruning();
                    
                    IsTrue(rule.Name == expectedName);
                    IsTrue(rule.Prune == output);
                }
                else
                {
                    // no name provided, there should be a non null default name
                    IsTrue(rule.Prune == AnnotationPruning.All);
                }

                IsTrue(tokenizer.FindRule(rule.Name) == rule);

                ValidatePruning(rule, name);

                // invoke the rule again - make sure the rule is not added to the rulegraph again
                // as it uses the same name
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

                    if (rule.Prune == AnnotationPruning.All)
                    {
                        IsTrue(result.Annotations == null);
                    }
                }

                // go over the valid samples
                foreach (var sample in invalidSamples)
                {
                    var result = rule.Parse(sample);

                    IsFalse(result.FoundMatch);
                }
            }
        }
    }
}
