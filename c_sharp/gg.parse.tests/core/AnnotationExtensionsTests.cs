using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.tests.core
{
    [TestClass]
    public class AnnotationExtensionsTests
    {
        [TestMethod]
        public void CreateAnnotationWithChildren_FilterByRuleId_ExpectResultToMatchRuleId()
        {
            var annotation = new Annotation(1, new(0, 1), [
                new Annotation(2, new(0,0), [
                    new Annotation(3, new(0,0))
                ]),
                new Annotation(4, new(0,0), [
                    new Annotation(5, new(0,0)),
                    new Annotation(6, new(0,0))
                ]),
            ]);

            // filter for rules with an even ruleId
            var filteredChildren = annotation.FilterChildren(a => a.RuleId % 2 == 0);

            IsTrue(filteredChildren != null);
            IsTrue(filteredChildren[0]!.RuleId == 2);
            IsTrue(filteredChildren[0]!.Children!.Count == 0);

            IsTrue(filteredChildren[1]!.RuleId == 4);
            IsTrue(filteredChildren[1]!.Children!.Count == 1);
            IsTrue(filteredChildren[1]![0]!.RuleId == 6);
        }

        [TestMethod]
        public void CreateAnnotationCollectionWithChildren_FilterByRuleId_ExpectResultToMatchRuleId()
        {
            var annotations = new List<Annotation>() {
                new Annotation(1, new(0, 1)),
                new Annotation(2, new(0, 1), [
                    new Annotation(2, new(0,0), [
                        new Annotation(3, new(0,0))
                    ]),
                    new Annotation(4, new(0,0), [
                        new Annotation(5, new(0,0)),
                        new Annotation(6, new(0,0))
                    ]),
                ]),
                new Annotation(8, new(0, 1), [
                    new Annotation(10, new(0,0), [])
                ])
            };

            // filter for rules with an even ruleId
            var filteredChildren = annotations.Filter(a => a.RuleId % 2 == 0);

            IsTrue(filteredChildren != null);
            IsTrue(filteredChildren.Count == 2);
            IsTrue(filteredChildren[0]!.RuleId == 2);
            IsTrue(filteredChildren[0]!.Children!.Count == 2);
            IsTrue(filteredChildren[0][0]!.RuleId == 2);
            IsTrue(filteredChildren[0][0]!.Children!.Count == 0);

            IsTrue(filteredChildren[0][1]!.RuleId == 4);
            IsTrue(filteredChildren[0][1]!.Children!.Count == 1);
            IsTrue(filteredChildren[0][1]![0]!.RuleId == 6);

            IsTrue(filteredChildren[1]!.RuleId == 8);
        }
    }
}
