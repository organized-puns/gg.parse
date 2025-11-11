// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.rules;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.doc.examples.test
{
    [TestClass]
    public class MatchEvaluationTests
    {
        private const string EvaluationName = "evaluation";
        private const string AddOpName = "addOperation";
        private const string NumberTokenName = "numberToken";
        private const string PlusTokenName = "plusToken";
        private const string MultOpName = "multOperation";
        private const string MultTokenName = "multToken";

        [TestMethod]
        public void MatchSimpleEquationWithStandardPrecedence()
        {
            var (tokenizer, parser) = CreateTokenizerAndParser(multPrecedence: 200, addPrecedence: 100);
            var tokens = tokenizer.Root.Parse("3 + 5 * 2");

            IsTrue(tokens.FoundMatch);
            IsTrue(tokens.Count == 5);

            var tokenIds = tokens.Annotations!.SelectRuleIds();
            var syntaxTree = parser.Root.Parse(tokenIds, 0);

            IsTrue(syntaxTree.FoundMatch);

            // expect the syntax tree to be:

            // evaluation
            //     |
            //    add
            //    /|\
            //   3 + mult
            //       /|\
            //      5 * 2    

            IsTrue(syntaxTree[0].Rule.Name == EvaluationName);
            IsTrue(syntaxTree[0][0].Rule.Name == AddOpName);
            IsTrue(syntaxTree[0][0][0].Rule.Name == NumberTokenName);
            IsTrue(syntaxTree[0][0][1].Rule.Name == PlusTokenName);
            IsTrue(syntaxTree[0][0][2].Rule.Name == MultOpName);
            IsTrue(syntaxTree[0][0][2][0].Rule.Name == NumberTokenName);
            IsTrue(syntaxTree[0][0][2][1].Rule.Name == MultTokenName);
            IsTrue(syntaxTree[0][0][2][2].Rule.Name == NumberTokenName);
        }

        [TestMethod]
        public void MatchSimpleEquationWithNonStandardPrecedence()
        {
            var (tokenizer, parser) = CreateTokenizerAndParser(multPrecedence: 200, addPrecedence: 400);
            var tokens = tokenizer.Root.Parse("3 + 5 * 2");

            IsTrue(tokens.FoundMatch);
            IsTrue(tokens.Count == 5);

            var tokenIds = tokens.Annotations!.SelectRuleIds();
            var syntaxTree = parser.Root.Parse(tokenIds, 0);

            IsTrue(syntaxTree.FoundMatch);

            // expect the syntax tree to be:
            //  evaluation
            //       |
            //      mult
            //      /|\
            //   add * 2
            //   /|\
            //  3 + 5    

            IsTrue(syntaxTree[0].Rule.Name == EvaluationName);
            
            IsTrue(syntaxTree[0][0].Rule.Name == MultOpName);
            
            IsTrue(syntaxTree[0][0][0].Rule.Name == AddOpName);
            IsTrue(syntaxTree[0][0][1].Rule.Name == MultTokenName);
            IsTrue(syntaxTree[0][0][2].Rule.Name == NumberTokenName);
            
            IsTrue(syntaxTree[0][0][0][0].Rule.Name == NumberTokenName);
            IsTrue(syntaxTree[0][0][0][1].Rule.Name == PlusTokenName);
            IsTrue(syntaxTree[0][0][0][2].Rule.Name == NumberTokenName);
        }

        /// <summary>
        /// Create a very simple math tokenizer and parser. 
        /// </summary>
        /// <param name="multPrecedence"></param>
        /// <param name="addPrecedence"></param>
        /// <returns></returns>
        private static (MutableRuleGraph<char> tokenizer, MutableRuleGraph<int> parser) CreateTokenizerAndParser(int multPrecedence, int addPrecedence)
        {
            var whitespace = new MatchDataSet<char>("whitespace", setValues: " \t\r\n".ToCharArray(), output: AnnotationPruning.All);

            var digit = new MatchDataRange<char>("digit", '0', '9');
            var number = new MatchCount<char>("number", AnnotationPruning.None, 0, digit, min: 1, max: 0);

            var plus = new MatchSingleData<char>("plus", '+');
            var mult = new MatchSingleData<char>("mult", '*');

            var tokenEnumeration = new MatchOneOf<char>("tokens", AnnotationPruning.Root, 0, rules: [whitespace, number, plus, mult]);
            var tokenStream = new MatchCount<char>("tokenStream", AnnotationPruning.Root, 0, tokenEnumeration, min: 1, max: 0);

            var tokenizer = new MutableRuleGraph<char>();

            tokenizer.RegisterRule(whitespace);
            tokenizer.RegisterRule(digit);
            tokenizer.RegisterRule(plus);
            tokenizer.RegisterRule(mult);
            tokenizer.RegisterRule(tokenEnumeration);
            tokenizer.RegisterRule(tokenStream);

            tokenizer.Root = tokenStream;

            var numberToken = new MatchSingleData<int>(NumberTokenName, number.Id);

            var plusToken = new MatchSingleData<int>(PlusTokenName, plus.Id);
            var multToken = new MatchSingleData<int>(MultTokenName, mult.Id);

            var addOperation = new MatchRuleSequence<int>(AddOpName, pruning: AnnotationPruning.None, precedence: addPrecedence, numberToken, plusToken, numberToken);
            var multOperation = new MatchRuleSequence<int>(MultOpName, pruning: AnnotationPruning.None, precedence: multPrecedence, numberToken, multToken, numberToken);

            var evaluation = new MatchEvaluation<int>(EvaluationName, pruning: AnnotationPruning.None, precedence: 0, addOperation, multOperation);

            var parser = new MutableRuleGraph<int>();

            parser.RegisterRule(numberToken);
            parser.RegisterRule(plusToken);
            parser.RegisterRule(multToken);
            parser.RegisterRule(addOperation);
            parser.RegisterRule(multOperation);
            parser.RegisterRule(evaluation);

            parser.Root = evaluation;

            return (tokenizer, parser);
        }
    }
}
