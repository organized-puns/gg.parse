// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using System.Collections.Immutable;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.tests.core
{
    [TestClass]
    public class AnnotationExtensionsTests
    {
        [TestMethod]
        public void CreateAnnotationWithChildren_PruneByRuleId_ExpectResultToMatchRuleId()
        {
            var annotation = new Annotation(new EmptyRule(1), new(0, 1), [
                new Annotation(new EmptyRule(2), new(0,0), [
                    new Annotation(new EmptyRule(3), new(0,0))
                ]),
                new Annotation(new EmptyRule(4), new(0,0), [
                    new Annotation(new EmptyRule(5), new(0,0)),
                    new Annotation(new EmptyRule(6), new(0,0))
                ]),
            ]);

            // prune annotations where the rule's id is odd
            var filteredChildren = annotation.Prune(a => a.Rule.Id % 2 == 1);

            IsTrue(filteredChildren != null);
            IsTrue(filteredChildren[0]!.Rule.Id == 2);
            IsTrue(filteredChildren[0]!.Children!.Count == 0);

            IsTrue(filteredChildren[1]!.Rule.Id== 4);
            IsTrue(filteredChildren[1]!.Children!.Count == 1);
            IsTrue(filteredChildren[1]![0]!.Rule.Id == 6);
        }

        // testing minor change to see if github rule protections are in place  (3) 

        [TestMethod]
        public void CreateAnnotationCollectionWithChildren_PruneByRuleId_ExpectResultToMatchRuleId()
        {
            var annotations = ImmutableList.Create<Annotation>(
                new(new EmptyRule(1), new(0, 1)),
                new(new EmptyRule(2), new(0, 1), [
                    new Annotation(new EmptyRule(2), new(0,0), [
                        new Annotation(new EmptyRule(3), new(0,0))
                    ]),
                    new Annotation(new EmptyRule(4), new(0,0), [
                        new Annotation(new EmptyRule(5), new(0,0)),
                        new Annotation(new EmptyRule(6), new(0,0))
                    ]),
                ]),
                new(new EmptyRule(8), new(0, 1), [
                    new Annotation(new EmptyRule(10), new(0,0), [])
                ])
            );

            // prune annotations where the rule's id is odd
            var filteredChildren = annotations.Prune(a => a.Rule.Id % 2 == 1);

            IsTrue(filteredChildren != null);
            IsTrue(filteredChildren.Count == 2);
            IsTrue(filteredChildren[0]!.Rule.Id == 2);
            IsTrue(filteredChildren[0]!.Children!.Count == 2);
            IsTrue(filteredChildren[0][0]!.Rule.Id == 2);
            IsTrue(filteredChildren[0][0]!.Children!.Count == 0);

            IsTrue(filteredChildren[0][1]!.Rule.Id == 4);
            IsTrue(filteredChildren[0][1]!.Children!.Count == 1);
            IsTrue(filteredChildren[0][1]![0]!.Rule.Id == 6);

            IsTrue(filteredChildren[1]!.Rule.Id == 8);
        }
    }
}
