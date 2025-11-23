// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.rules;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.tests.core
{
    [TestClass]
    public class RuleGraphTests
    {
        [TestMethod]
        public void CreateEmptyGraph_Findrule_ExpectNull()
        {
            var graph = new MutableRuleGraph<char>();
            var foundRule = graph.FindRule("nonexistent");
            IsNull(foundRule);
        }

        [TestMethod]
        public void CreateEmptyGraphAndDataRule_Register_ExpectSameRuleRegistered()
        {
            var graph = new MutableRuleGraph<char>();
            var originalRule = new MatchDataSequence<char>("original", ['a', 'b', 'c']);

            var registeredRule = graph.FindOrRegisterRuleAndSubRules(originalRule);

            IsTrue(registeredRule == originalRule);
        }

        [TestMethod]
        public void CreateEmptyGraphAndDataRule_RegisterTwice_ExpectFirstRuleOnSecondRegistration()
        {
            var graph = new MutableRuleGraph<char>();
            var firstRule = new MatchDataSequence<char>("original", ['a', 'b', 'c']);
            var secondRule = new MatchDataSequence<char>("original", ['a', 'b', 'c']);

            var registeredRule = graph.FindOrRegisterRuleAndSubRules(firstRule);

            IsTrue(registeredRule == firstRule);

            registeredRule = graph.FindOrRegisterRuleAndSubRules(secondRule);

            IsTrue(registeredRule == firstRule);
        }

        [TestMethod]
        public void CreateEmptyGraphAndMetaRule_RegisterSubjectTwice_ExpectNewMetaRule()
        {
            var graph = new MutableRuleGraph<char>();
            var firstRule = new MatchDataSequence<char>("original", ['a', 'b', 'c']);
            var secondRule = new MatchDataSequence<char>("original", ['a', 'b', 'c']);
            var metaRule = new MatchNot<char>("match_not", AnnotationPruning.None, 0, secondRule);

            graph.FindOrRegisterRuleAndSubRules(firstRule);

            var registeredRule = graph.FindOrRegisterRuleAndSubRules(metaRule);

            // should be a clone, so not the original meta rule because register had to create 
            // a new meta rule which uses the registered firstRule as subject
            IsTrue(registeredRule != metaRule);
            IsTrue(registeredRule.Subject == firstRule);
        }

        [TestMethod]
        public void CreateEmptyGraphAndRuleComposition_RegisterSubruleTwice_ExpectNewRuleComposition()
        {
            var graph = new MutableRuleGraph<char>();
            var firstRule = new MatchDataSequence<char>("original", ['a', 'b', 'c']);
            var secondRule = new MatchDataSequence<char>("original", ['a', 'b', 'c']);
            var fooRule = new MatchDataSequence<char>("other_rule", ['f', 'o', 'o']);
            var composition = new MatchRuleSequence<char>("match_sequence", AnnotationPruning.None, 0, secondRule, fooRule);

            graph.FindOrRegisterRuleAndSubRules(firstRule);

            var registeredRule = graph.FindOrRegisterRuleAndSubRules(composition);

            // should be a clone, so not the original composition because register had to create 
            // a new meta rule which uses the registered firstRule as subject
            IsTrue(registeredRule != composition);
            IsTrue(registeredRule[0] == firstRule);
            // foo rule is new so should be the same
            IsTrue(registeredRule[1] == fooRule);
        }

        [TestMethod]
        public void CreateEmptyGraphAndComplexRuleComposition_RegisterSubruleTwice_ExpectNewRuleComposition()
        {
            var graph = new MutableRuleGraph<char>();
            var firstRule = new MatchDataSequence<char>("original", ['a', 'b', 'c']);
            var secondRule = new MatchDataSequence<char>("original", ['a', 'b', 'c']);
            var fooRule1 = new MatchDataSequence<char>("foo_rule", ['f', 'o', 'o']);
            var fooRule2 = new MatchDataSequence<char>("foo_rule", ['f', 'o', 'o']);
            var metaRule = new MatchNot<char>("match_not", AnnotationPruning.None, 0, secondRule);
            var subComposition = new MatchRuleSequence<char>("sub_sequence", AnnotationPruning.None, 0, fooRule1);
            var composition = new MatchRuleSequence<char>("match_sequence", AnnotationPruning.None, 0, metaRule, subComposition, fooRule2);

            graph.FindOrRegisterRuleAndSubRules(firstRule);

            var registeredRule = graph.FindOrRegisterRuleAndSubRules(composition);

            // should be a clone, so not the original composition because register had to create 
            // a new meta rule which uses the registered firstRule as subject
            IsTrue(registeredRule != composition);
            // meta rule is using a copy of first rule, so it should be replaced witha clone
            IsTrue(registeredRule[0] != metaRule);
            IsTrue(((IMetaRule)registeredRule[0]).Subject == firstRule);
            // subComposition is new so should be the same
            IsTrue(registeredRule[1] == subComposition);
            // foo2 was already registered via  subComposition so it should be the already registered one
            IsTrue(registeredRule[2] == fooRule1);
        }

        [TestMethod]
        public void CreateGraphWithSingleRule_Replace_ExpectRuleToBeReplaced()
        {
            var graph = new MutableRuleGraph<char>();
            var originalRule = new MatchDataSequence<char>("original", ['a', 'b', 'c']);
            
            graph.Register(originalRule);
            
            var replacementRule = new MatchDataSequence<char>("original", ['a', 'b', 'c']);;
            graph.ReplaceRule(originalRule, replacementRule);
            
            var foundRule = graph.FindRule("original") as MatchDataSequence<char>;
            IsNotNull(foundRule);
            IsTrue(foundRule == replacementRule);
        }

        [TestMethod]
        public void CreateGraphWithCompositionRule_Replace_ExpectRuleToBeReplaced()
        {
            var graph = new MutableRuleGraph<char>();
            var originalRule = new MatchDataSequence<char>("original", ['a', 'b', 'c']);
            var composition = new MatchRuleSequence<char>("composition", AnnotationPruning.None, 0, originalRule);

            graph.Register(originalRule);
            graph.Register(composition);

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
            var graph = new MutableRuleGraph<char>();
            var composition1 = new MatchRuleSequence<char>("original", AnnotationPruning.None, 0);
            var composition2 = new MatchRuleSequence<char>("composition", AnnotationPruning.None, 0, composition1);

            composition1.MutateComposition([composition2]);

            graph.Register(composition1);
            graph.Register(composition2);

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
            var graph = new MutableRuleGraph<char>();
            var composition1 = new MatchRuleSequence<char>("original", AnnotationPruning.None, 0);
            
            composition1.MutateComposition([composition1]);

            graph.Register(composition1);          

            var replacementRule = new MatchRuleSequence<char>("original", AnnotationPruning.None, 0, composition1);
            graph.ReplaceRule(composition1, replacementRule);

            var foundRule = graph.FindRule("original") as MatchRuleSequence<char>;
            IsNotNull(foundRule);
            IsTrue(foundRule == replacementRule);
            IsTrue(foundRule[0] == replacementRule);
        }
    }
}
