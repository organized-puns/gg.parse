#nullable disable

using gg.parse.rules;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.tests.rules
{
    [TestClass]
    public class SkipRuleTests
    {
        [TestMethod]
        public void CreateSkipUntilFoo_Parse_ExpectSkippedUntilFoo()
        {
            var isFoo = new MatchDataSequence<char>("isFoo", "foo".ToCharArray());
            var skipRule = new SkipRule<char>("testSkip", AnnotationProduct.Annotation, isFoo)
            {
                Id = 42
            };
            var result = skipRule.Parse("abcfoo");

            IsTrue(result.FoundMatch);
            IsTrue(result.MatchedLength == 3);
            IsTrue(result[0]!.Rule == skipRule);
            IsTrue(result[0]!.Range.Equals(new Range(0, 3)));
        }

        [TestMethod]
        public void CreateSkipUntilFoo_Parse_ExpectFailureBecauseEoF()
        {
            var isFoo = new MatchDataSequence<char>("isFoo", "foo".ToCharArray());
            var skipRule = new SkipRule<char>("testSkip", AnnotationProduct.Annotation, isFoo)
            {
                Id = 42
            };
            var result = skipRule.Parse("abc");

            IsFalse(result.FoundMatch);
        }

        [TestMethod]
        public void CreateSkipUntilFoo_Parse_ExpectSucceedDespiteEof()
        {
            var isFoo = new MatchDataSequence<char>("isFoo", "foo".ToCharArray());
            var skipRule = new SkipRule<char>("testSkip", AnnotationProduct.Annotation, isFoo, failOnEof: false)
            {
                Id = 42
            };
            var result = skipRule.Parse("abcfo");

            IsTrue(result.FoundMatch);
            IsTrue(result.MatchedLength == 5);
            IsTrue(result[0]!.Rule == skipRule);
            IsTrue(result[0]!.Range.Equals(new Range(0, 5)));
        }
    }
}
