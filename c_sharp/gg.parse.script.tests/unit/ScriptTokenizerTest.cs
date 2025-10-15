using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

using gg.parse.script.common;
using gg.parse.script.parser;

namespace gg.parse.script.tests.unit
{
    [TestClass]
    public class ScriptTokenizerTests
    {
        [TestMethod]
        public void TestEmptyObject_ExpectTwoAnnotations()
        {
            var tokenizer = new ScriptTokenizer();
            var rule = "rule_name = *'literal';";

            var (isSuccess, charactersRead, annotations) = tokenizer.Tokenize(rule);

            IsTrue(isSuccess);
            IsTrue(charactersRead == rule.Length);
            IsTrue(annotations!.Count == 5);
            IsTrue(annotations[0].Rule == tokenizer.FindRule(CommonTokenNames.Identifier));
            IsTrue(annotations[1].Rule == tokenizer.FindRule(CommonTokenNames.Assignment));
            IsTrue(annotations[2].Rule == tokenizer.FindRule(CommonTokenNames.ZeroOrMoreOperator));
            IsTrue(annotations[3].Rule == tokenizer.FindRule(CommonTokenNames.SingleQuotedString));
            IsTrue(annotations[4].Rule == tokenizer.FindRule(CommonTokenNames.EndStatement));
        }

        [TestMethod]
        public void CreateEvalRule_Tokenize_ExpectOptionWithPrecedenceTokens()
        {
            var tokenizer = new ScriptTokenizer();
            var rule = "rule_name = 'foo' / 'bar' / 'baz';";

            var (isSuccess, charactersRead, annotations) = tokenizer.Tokenize(rule);

            IsTrue(isSuccess);
            IsTrue(charactersRead == rule.Length);
            IsTrue(annotations!.Count == 8);
            IsTrue(annotations[0].Rule == tokenizer.FindRule(CommonTokenNames.Identifier));
            IsTrue(annotations[1].Rule == tokenizer.FindRule(CommonTokenNames.Assignment));
            IsTrue(annotations[2].Rule == tokenizer.FindRule(CommonTokenNames.SingleQuotedString));
            IsTrue(annotations[3].Rule == tokenizer.FindRule(CommonTokenNames.OptionWithPrecedence));
            IsTrue(annotations[4].Rule == tokenizer.FindRule(CommonTokenNames.SingleQuotedString));
            IsTrue(annotations[5].Rule == tokenizer.FindRule(CommonTokenNames.OptionWithPrecedence));
            IsTrue(annotations[6].Rule == tokenizer.FindRule(CommonTokenNames.SingleQuotedString));
            IsTrue(annotations[7].Rule == tokenizer.FindRule(CommonTokenNames.EndStatement));
        }

        [TestMethod]
        public void DefineIfMatchRule_Tokenize_ExpectValidMatchAnyTokens()
        {
            var tokenizer = new ScriptTokenizer();
            var rule = "rule_name = if 'literal';";

            var (isSuccess, charactersRead, annotations) = tokenizer.Tokenize(rule);

            IsTrue(isSuccess);
            IsTrue(charactersRead == rule.Length);
            IsTrue(annotations!.Count == 5);
            IsTrue(annotations[0].Rule == tokenizer.FindRule(CommonTokenNames.Identifier));
            IsTrue(annotations[1].Rule == tokenizer.FindRule(CommonTokenNames.Assignment));
            IsTrue(annotations[2].Rule == tokenizer.FindRule(CommonTokenNames.If));
            IsTrue(annotations[3].Rule == tokenizer.FindRule(CommonTokenNames.SingleQuotedString));
            IsTrue(annotations[4].Rule == tokenizer.FindRule(CommonTokenNames.EndStatement));
        }

        [TestMethod]
        public void DefineMatchCharacterSet_Parse_ExpectValidTokens()
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
        public void DefineEscapeTokensInString_Tokenize_ExpectValidTokens()
        {
            var tokenizer = new ScriptTokenizer();
            var rule = "rule_name = \"\\abc\", \"def\", '\\\\', '123\\\\';";

            var (isSuccess, charactersRead, annotations) = tokenizer.Tokenize(rule);

            IsTrue(isSuccess);
            IsTrue(charactersRead == rule.Length);
            IsTrue(annotations!.Count == 10);
            IsTrue(annotations[0].Rule == tokenizer.FindRule(CommonTokenNames.Identifier));
            IsTrue(annotations[1].Rule == tokenizer.FindRule(CommonTokenNames.Assignment));
            // \abc
            IsTrue(annotations[2].Rule == tokenizer.FindRule(CommonTokenNames.DoubleQuotedString));
            IsTrue(annotations[3].Rule == tokenizer.FindRule(CommonTokenNames.CollectionSeparator));
            // def
            IsTrue(annotations[4].Rule == tokenizer.FindRule(CommonTokenNames.DoubleQuotedString));
            IsTrue(annotations[5].Rule == tokenizer.FindRule(CommonTokenNames.CollectionSeparator));
            // \
            IsTrue(annotations[6].Rule == tokenizer.FindRule(CommonTokenNames.SingleQuotedString));
            IsTrue(annotations[7].Rule == tokenizer.FindRule(CommonTokenNames.CollectionSeparator));
            // 123\
            IsTrue(annotations[8].Rule == tokenizer.FindRule(CommonTokenNames.SingleQuotedString));

            IsTrue(annotations[9].Rule == tokenizer.FindRule(CommonTokenNames.EndStatement));
        }


        [TestMethod]
        public void DefineRuleWithoutPrecedence_Tokenize_ExpectValidRuleDeclarations()
        {
            var tokenizer = new ScriptTokenizer();
            
            // no precedence defined
            var rule = "rule_name = .;";

            var (isSuccess, charactersRead, annotations) = tokenizer.Tokenize(rule);

            IsTrue(isSuccess);
            IsTrue(charactersRead == rule.Length);
            IsTrue(annotations!.Count == 4);
            IsTrue(annotations[0].Rule == tokenizer.FindRule(CommonTokenNames.Identifier));
            IsTrue(annotations[1].Rule == tokenizer.FindRule(CommonTokenNames.Assignment));
            IsTrue(annotations[2].Rule == tokenizer.FindRule(CommonTokenNames.AnyCharacter));
            IsTrue(annotations[3].Rule == tokenizer.FindRule(CommonTokenNames.EndStatement));
        }

        [TestMethod]
        public void DefineRuleWithPrecedence_Tokenize_ExpectValidRuleDeclarations()
        {
            var tokenizer = new ScriptTokenizer();

            // simple precedence defined
            var ruleDefinitions = new string[] {
                // try different rulename, spacings and precedence values
                "rule_name   100= .;",
                "rule_name 42  = .;",
                "rule_name1 -1  = .;",
            };

            var expectedPrecedences = new int[] { 100, 42, -1 };

            for (var i = 0; i < ruleDefinitions.Length; i++)
            {
                var rule = ruleDefinitions[i];
                var (isSuccess, charactersRead, annotations) = tokenizer.Tokenize(rule);

                IsTrue(isSuccess);
                IsTrue(charactersRead == rule.Length);
                IsTrue(annotations!.Count == 5);
                IsTrue(annotations[0].Rule == tokenizer.FindRule(CommonTokenNames.Identifier));
                IsTrue(annotations[1].Rule == tokenizer.FindRule(CommonTokenNames.Integer));

                IsTrue(annotations[1].Rule == tokenizer.FindRule(CommonTokenNames.Integer));
                var value = ruleDefinitions[i].AsSpan(annotations[1].Range.Start, annotations[1].Range.Length).ToString();
                IsTrue(int.Parse(value) == expectedPrecedences[i]);

                IsTrue(annotations[2].Rule == tokenizer.FindRule(CommonTokenNames.Assignment));
                IsTrue(annotations[3].Rule == tokenizer.FindRule(CommonTokenNames.AnyCharacter));
                IsTrue(annotations[4].Rule == tokenizer.FindRule(CommonTokenNames.EndStatement));
            }
        }
    }
}
