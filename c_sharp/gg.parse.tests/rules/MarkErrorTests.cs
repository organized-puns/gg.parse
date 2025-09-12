using gg.parse.rulefunctions;

using gg.parse.rulefunctions.datafunctions;
using gg.parse.rulefunctions.rulefunctions;

namespace gg.parse.tests.rules
{
    [TestClass]
    public class MarkErrorTests
    {
        /// <summary>
        /// Complex test, create a sequence which doesn't succeed but has a fallback
        /// to report an error.
        /// </summary>
        [TestMethod]
        public void FallbackTest_ExpectFallbackCalled()
        {
            // simplified version of a string
            // "'", *(!"'", .), "'"
            var delimiter       = new MatchDataSequence<char>("delimiter", ['\''], AnnotationProduct.None);
            var notDelimiter    = new MatchNotFunction<char>("not_delimiter", AnnotationProduct.None, delimiter);
            var stringCharacter = new MatchFunctionSequence<char>("string_character", AnnotationProduct.None, 0,
                                        notDelimiter,
                                        new MatchAnyData<char>("any_char"));
            var stringSequence  = new MatchFunctionSequence<char>("string",
                                        AnnotationProduct.Annotation, 0,
                                        delimiter,
                                        new MatchFunctionCount<char>("string_characters", stringCharacter, min: 0, max: 0),
                                        delimiter);

            stringSequence.Id = 1;

            // verify the sequence green path
            Assert.IsTrue(stringSequence.Parse("'foo'".ToCharArray(), 0).FoundMatch);
            Assert.IsTrue(stringSequence.Parse("''".ToCharArray(), 0).FoundMatch);
            // missing closing delimiter
            Assert.IsFalse(stringSequence.Parse("'foo".ToCharArray(), 0).FoundMatch);

            // create a fallback (ie skip until EOF is encountered)
            var skip = new MatchNotFunction<char>("EOF", AnnotationProduct.None, new MatchAnyData<char>("any"));            
            var matchError = new MarkError<char>("string_not_closed", AnnotationProduct.Annotation, testFunction: skip);

            matchError.Id = 42;

            var stringWithFallback = new MatchOneOfFunction<char>("string_with_fallback", AnnotationProduct.Transitive, 0,
                                        stringSequence, matchError);

            // normal string should still match
            var result = stringWithFallback.Parse("'foo'".ToCharArray(), 0);
            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.Annotations != null);
            Assert.IsTrue(result.Annotations[0].RuleId == stringSequence.Id);

            var stringWithError = "'foo";
            result = stringWithFallback.Parse(stringWithError.ToCharArray(), 0);

            Assert.IsTrue(result.FoundMatch);
            Assert.IsTrue(result.Annotations != null);
            Assert.IsTrue(result.Annotations[0].RuleId == matchError.Id);
            Assert.IsTrue(result.Annotations[0].Start == 0);
            Assert.IsTrue(result.Annotations[0].Length == stringWithError.Length);
        }
    }
}
