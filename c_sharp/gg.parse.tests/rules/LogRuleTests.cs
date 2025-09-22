#nullable disable

using gg.parse.rules;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.tests.rules
{
    [TestClass]
    public class LogRuleTests
    {
        /// <summary>
        /// LogRule without an conditions, should match any input
        /// </summary>
        [TestMethod]
        public void CreateLogRuleWithoutConditions_Parse_ShouldMatchAnyInput()
        {
            var logRule = new LogRule<char>("log", AnnotationProduct.Annotation, "description");

            IsTrue(logRule.Condition == null);
            IsTrue(logRule.Level == LogLevel.Info);

            var parseFooResult = logRule.Parse("foo".ToCharArray(), 0);

            IsTrue(parseFooResult.FoundMatch);
            IsTrue(parseFooResult.MatchedLength == 0);
            IsTrue(parseFooResult.Annotations != null && parseFooResult.Annotations.Count == 1);
            IsTrue(parseFooResult.Annotations[0].Rule == logRule);
            
            IsTrue(logRule.Parse("".ToCharArray(), 0).FoundMatch);
            IsTrue(logRule.Parse("".ToCharArray(), 0).MatchedLength == 0);
        }

        /// <summary>
        /// LogRule with a condition, should match only input which matches the condition.
        /// </summary>
        [TestMethod]
        public void CreateLogRuleWithLiteralCondition_Parse_ShouldMatchLiteral()
        {
            var matchFoo = new MatchDataSequence<char>("matchFoo", "foo".ToCharArray());
            var logRule = new LogRule<char>("log", AnnotationProduct.Annotation, "description", matchFoo, LogLevel.Error);

            IsTrue(logRule.Condition == matchFoo);
            IsTrue(logRule.Level == LogLevel.Error);

            var parseFooResult = logRule.Parse("foo".ToCharArray(), 0);

            // positive case
            IsTrue(parseFooResult.FoundMatch);
            IsTrue(parseFooResult.MatchedLength == 3);
            IsTrue(parseFooResult.Annotations != null && parseFooResult.Annotations.Count == 1);
            IsTrue(parseFooResult.Annotations[0].Rule == logRule);
            IsTrue(parseFooResult.Annotations[0].Children == null);

            // negative cases
            IsFalse(logRule.Parse("".ToCharArray(), 0).FoundMatch);
            IsFalse(logRule.Parse("Foo".ToCharArray(), 0).FoundMatch);
            IsFalse(logRule.Parse("bar".ToCharArray(), 0).FoundMatch);
        }

        /// <summary>
        /// LogRule with a fatal condition, should throw an exception when the condition is met.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FatalConditionException<char>))]
        public void CreateLogRuleWithFatalCondition_Parse_ShouldThrowExceptionWhenConditionIsMet()
        {
            var matchFoo = new MatchDataSequence<char>("matchFoo", "foo".ToCharArray());
            var logRule = new LogRule<char>("log", AnnotationProduct.Annotation, "description", matchFoo, LogLevel.Fatal);

            IsTrue(logRule.Condition == matchFoo);
            IsTrue(logRule.Level == LogLevel.Fatal);

            // negative cases
            IsFalse(logRule.Parse("".ToCharArray(), 0).FoundMatch);
            IsFalse(logRule.Parse("Foo".ToCharArray(), 0).FoundMatch);
            IsFalse(logRule.Parse("bar".ToCharArray(), 0).FoundMatch);

            // should throw exception
            logRule.Parse("foo".ToCharArray(), 0);           
        }
    }
}
