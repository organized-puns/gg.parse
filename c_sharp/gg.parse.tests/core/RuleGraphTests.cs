// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.rules;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.tests.core
{
    [TestClass]
    public class RuleGraphTests
    {
        [TestMethod]
        public void CreateGraphWithSingleRule_Replace_ExpectRuleToBeReplaced()
        {
            var graph = new RuleGraph<char>();
            var originalRule = new MatchDataSequence<char>("original", ['a', 'b', 'c']);
            
            graph.RegisterRule(originalRule);
            
            var replacementRule = new MatchDataSequence<char>("original", ['a', 'b', 'c']);;
            graph.ReplaceRule(originalRule, replacementRule);
            
            var foundRule = graph.FindRule("original") as MatchDataSequence<char>;
            IsNotNull(foundRule);
            IsTrue(foundRule == replacementRule);
        }

        [TestMethod]
        public void CreateGraphWithCompositionRule_Replace_ExpectRuleToBeReplaced()
        {
            var graph = new RuleGraph<char>();
            var originalRule = new MatchDataSequence<char>("original", ['a', 'b', 'c']);
            var composition = new MatchRuleSequence<char>("composition", AnnotationPruning.None, 0, originalRule);

            graph.RegisterRule(originalRule);
            graph.RegisterRule(composition);

            var replacementRule = new MatchDataSequence<char>("original", ['a', 'b', 'c']); 
            graph.ReplaceRule(originalRule, replacementRule);

            var foundRule = graph.FindRule("original") as MatchDataSequence<char>;
            IsNotNull(foundRule);
            IsTrue(foundRule == replacementRule);

            var foundComposition = graph.FindRule("composition") as MatchRuleSequence<char>;
            IsNotNull(foundComposition);
            IsTrue(foundComposition == composition);

            IsTrue(foundComposition[0] == replacementRule);
        }

        [TestMethod]
        public void CreateGraphWithReferentialCompositionRule_Replace_ExpectRuleToBeReplaced()
        {
            var graph = new RuleGraph<char>();
            var composition1 = new MatchRuleSequence<char>("original", AnnotationPruning.None, 0);
            var composition2 = new MatchRuleSequence<char>("composition", AnnotationPruning.None, 0, composition1);

            composition1.MutateComposition([composition2]);

            graph.RegisterRule(composition1);
            graph.RegisterRule(composition2);

            var replacementRule = new MatchRuleSequence<char>("original", AnnotationPruning.None, 0, composition2); 
            graph.ReplaceRule(composition1, replacementRule);

            var foundRule = graph.FindRule("original") as MatchRuleSequence<char>;
            IsNotNull(foundRule);
            IsTrue(foundRule == replacementRule);

            var foundComposition = graph.FindRule("composition") as MatchRuleSequence<char>;
            IsTrue(foundComposition == composition2);
            IsTrue(foundComposition[0] == replacementRule);
        }

        [TestMethod]
        public void CreateGraphWithSelfReferentialCompositionRule_Replace_ExpectRuleToBeReplaced()
        {
            var graph = new RuleGraph<char>();
            var composition1 = new MatchRuleSequence<char>("original", AnnotationPruning.None, 0);
            
            composition1.MutateComposition([composition1]);

            graph.RegisterRule(composition1);          

            var replacementRule = new MatchRuleSequence<char>("original", AnnotationPruning.None, 0, composition1);
            graph.ReplaceRule(composition1, replacementRule);

            var foundRule = graph.FindRule("original") as MatchRuleSequence<char>;
            IsNotNull(foundRule);
            IsTrue(foundRule == replacementRule);
            IsTrue(foundRule[0] == replacementRule);
        }
    }
}
