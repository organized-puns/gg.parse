using gg.parse.ebnf;
using gg.parse.rulefunctions;

namespace gg.parse.tests.ebnf
{
    /// <summary>
    /// See if include statements work as intended
    /// </summary>
    [TestClass]
    public class EbnfParserIncludesTests
    {
        /// <summary>
        /// See if the tokenizer / parser breaks the include command down correctly in tokens
        /// / astTree.
        /// </summary>
        [TestMethod]
        public void SetupTokenizerParser_ParseCompiledRule_ExpectTokensAndAstTreeToMatchExpectations()
        {
            var includeCommand = "include 'assets/string_tokenization.ebnf'; string_ref = string;";

            var tokenizer = new EbnfTokenizer();
            var tokenizerParser = new EbnfTokenParser(tokenizer);

            var tokenizerTokens = tokenizer.Tokenize(includeCommand).Annotations;
            var expectedIds = new int[] {
                                tokenizer.FindRule(CommonTokenNames.Include).Id,
                                tokenizer.FindRule(CommonTokenNames.SingleQuotedString).Id,
                                tokenizer.FindRule(CommonTokenNames.EndStatement).Id,
                                tokenizer.FindRule(CommonTokenNames.Identifier).Id,
                                tokenizer.FindRule(CommonTokenNames.Assignment).Id,
                                tokenizer.FindRule(CommonTokenNames.Identifier).Id,
                                tokenizer.FindRule(CommonTokenNames.EndStatement).Id
            };
            var tokenIds = tokenizerTokens.Select(t => t.FunctionId).ToArray();

            Assert.IsTrue(tokenIds.SequenceEqual(expectedIds));

            var tokenizerAstTree = tokenizerParser.Parse(tokenizerTokens).Annotations;

            // expect an 'include' and a 'rule'
            var expectedAstNodes = new int[] {
                tokenizerParser.Include.Id,
                tokenizerParser.MatchRule.Id
            };

            var astNodeIds = tokenizerAstTree.Select(t => t.FunctionId).ToArray();

            Assert.IsTrue(astNodeIds.SequenceEqual(expectedAstNodes));
        }

        /// <summary>
        /// Trivial include in a tokenizer
        /// </summary>
        [TestMethod]
        public void CreateEbnfParser_FindRule_ExpectIncludedRulesToExist()
        {
            var includeCommand = "include 'assets/string_tokenization.ebnf';";
            var jsonParser = new EbnfParser(includeCommand, null);

            // should have loaded the string rule from the included file
            Assert.IsTrue(jsonParser.EbnfTokenizer.FindRule("string") != null);
        }

        /// <summary>
        /// See if the included token can actually be used
        /// </summary>
        [TestMethod]
        public void CreateEbnfParser_ParseCompiledRule_ExpectRuleToFindMatch()
        {
            var includeCommand = "include 'assets/string_tokenization.ebnf'; string_ref = string;";
            var jsonParser = new EbnfParser(includeCommand, null);

            // should have loaded the string rule from the included file
            Assert.IsTrue(jsonParser.EbnfTokenizer.FindRule("string") != null);

            var stringRef = jsonParser.EbnfTokenizer.FindRule("string_ref");
            Assert.IsTrue(stringRef != null);

            var parseResult = stringRef.Parse("\"this is a string\"".ToCharArray(), 0);
            Assert.IsTrue(parseResult.FoundMatch);
            Assert.IsTrue(parseResult.Annotations != null);
            Assert.IsTrue(parseResult.Annotations[0].FunctionId == stringRef.Id);
        }

        /// <summary>
        /// See if the duplicate includes are ignored 
        /// (xxx however until we report warnings, this cannot be detected from the outside
        /// directly and we just have to see it doesn't throw an error because of duplicate 
        /// function being registeres)
        /// </summary>
        [TestMethod]
        public void CreateEbnfParser_ParseCompiledRule_ExpectDuplicateIncludesToBeIgnored()
        {
            var includeCommand = "include 'assets/string_tokenization.ebnf'; include 'assets/string_tokenization.ebnf'; string_ref = string;";
            var jsonParser = new EbnfParser(includeCommand, null);

            // should have loaded the string rule from the included file
            Assert.IsTrue(jsonParser.EbnfTokenizer.FindRule("string") != null);

            var stringRef = jsonParser.EbnfTokenizer.FindRule("string_ref");
            Assert.IsTrue(stringRef != null);

            var parseResult = stringRef.Parse("\"this is a string\"".ToCharArray(), 0);
            Assert.IsTrue(parseResult.FoundMatch);
            Assert.IsTrue(parseResult.Annotations != null);
            Assert.IsTrue(parseResult.Annotations[0].FunctionId == stringRef.Id);
        }

        /// <summary>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidProgramException))]
        public void CreateEbnfParser_ParseCompiledRule_ExpectExceptionBecauseOfCircularDependencies()
        {
            var includeCommand = "include 'assets/include_circular_1.ebnf';";
            // this should throw and exception
            new EbnfParser(includeCommand, null);
        }
    }
}
