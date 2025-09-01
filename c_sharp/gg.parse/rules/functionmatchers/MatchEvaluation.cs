using gg.core.util;
using gg.parse.rulefunctions;

namespace gg.parse.rulefunctions.rulefunctions
{
    public class MatchEvaluation<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        private RuleBase<T>[]? _options;

        public RuleBase<T>[] RuleOptions 
        {
            get => _options!;
            set
            {
                Contract.Requires(value != null);
                Contract.Requires(value!.Any(v => v != null));

                _options = value!;
            }
        }

        public IEnumerable<RuleBase<T>> SubRules => RuleOptions;

        public Func<int, RuleBase<T>> FindRule { get; init; }

        public MatchEvaluation(string name, params RuleBase<T>[] options)
            : base(name, AnnotationProduct.Annotation)
        {
            Contract.Requires(options != null);
            Contract.Requires(options!.Any(v => v != null));

            RuleOptions = options!;
            FindRule = i => _options.FirstOrDefault(r => r.Id == i);
        }

        public MatchEvaluation(string name, AnnotationProduct production, int precedence, params RuleBase<T>[] options)
            : base(name, production, precedence)
        {
            Contract.Requires(options != null);
            Contract.Requires(options!.Any(v => v != null));

            RuleOptions = options!;
            FindRule = i => _options.FirstOrDefault(r => r.Id == i);
        }

        /// <summary>
        /// Try to parse an evaluation expression taking in account operator precedence.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public override ParseResult Parse(T[] input, int start)
        {            
            var previousMatch = FindMatch(input, start);

            if (previousMatch.FoundMatch)
            {
                Contract.Requires(previousMatch.Annotations != null 
                                    && previousMatch.Annotations.Count > 0
                                    && previousMatch.Annotations[0] != null, "No annotations found in result. Evaluation result must have exactly 1 annotation.");
                Contract.Requires(previousMatch.Annotations!.Count == 1, "Multiple annotations found. Evaluation result must have exactly 1 annotation.");

                var result = previousMatch;
                var root = previousMatch.Annotations[0];



                { 
                    var tokenIndex = root.Children != null && root.Children!.Count > 1
                            // move the token pointer to the end of the last child annotation, so we can try to match another 
                            // binary. The assumption here is that the root of this annotation expresses the operator
                            // the first child is the left-hand side of the operator, the second child is the operator and the third
                            // child is the right-hand side of the operator.
                            // so take the third child token position and try to find another match
                            ? root.Children[2].Start
                            // unary operator / expression, continue with the next token
                            : root.End;

                    while (tokenIndex < input.Length)
                    {
                        var nextMatch = FindMatch(input, tokenIndex);

                        if (nextMatch.FoundMatch)
                        {
                            Contract.Requires(nextMatch.Annotations != null
                                    && nextMatch.Annotations.Count > 0
                                    && nextMatch.Annotations[0] != null, "No annotations found in result. Evaluation result must have exactly 1 annotation.");
                            Contract.Requires(nextMatch.Annotations!.Count == 1, "Multiple annotations found. Evaluation result must have exactly 1 annotation.");

                            var ruleId = previousMatch.Annotations[0].FunctionId;
                            var previousMatchRule = FindRule(ruleId);

                            Contract.RequiresNotNull(previousMatchRule, $"Could not find rule({ruleId}) associated with the annotation.");

                            ruleId = nextMatch.Annotations[0].FunctionId;
                            var nextMatchRule = FindRule(ruleId);

                            Contract.RequiresNotNull(nextMatchRule, $"Could not find rule({ruleId}) associated with the annotation.");

                            // update the tree based on precendece, where the higher precendence rule is (should)
                            // be evaluated first.
                            if (previousMatchRule.Precedence >= nextMatchRule.Precedence)
                            {
                                var parent = nextMatch.Annotations![0];
                                var left = previousMatch.Annotations![0];

                                // unary operator/expression
                                if (parent.Children == null || parent.Children!.Count == 0)
                                {
                                    nextMatch.Annotations![0] = new Annotation(
                                        parent.FunctionId,
                                        new Range(left.Start, (left.Length + parent.Length)),
                                        [left]
                                    );

                                    tokenIndex = nextMatch.Annotations![0].End;
                                }
                                // binary operator
                                else if (parent.Children != null && parent.Children!.Count == 3)
                                {
                                    nextMatch.Annotations![0] = new Annotation(
                                        parent.FunctionId,
                                        new Range(left.Start, (left.Length + parent.Length) - 1),
                                        [left, parent.Children[1], parent.Children[2]]
                                    );

                                    // move the token pointer to the start of the second child (eg in 3 + 4, the 4)
                                    // so if there is another binary operator after that, we can evaluate it next
                                    tokenIndex = nextMatch.Annotations![0].Children![2].Start;
                                }
                                // else some other format for which we don't have an implementation
                                else
                                {
                                    throw new NotImplementedException("Evaluation of this expression format is not implemented.");
                                }

                                result = nextMatch;
                            }
                            else
                            {
                                var parent = previousMatch.Annotations![0];
                                var right = nextMatch.Annotations![0];

                                // unary operator/expression
                                if (parent.Children == null || parent.Children!.Count == 0)
                                {
                                    previousMatch.Annotations![0] = new Annotation(
                                        parent.FunctionId,
                                        new Range(parent.Start, parent.Length + right.Length),
                                        [right]
                                    );

                                    tokenIndex = nextMatch.Annotations![0].End;
                                }
                                // binary operator
                                else if (parent.Children != null && parent.Children!.Count == 3)
                                {
                                    previousMatch.Annotations![0] = new Annotation(
                                        parent.FunctionId,
                                        new Range(parent.Start, (parent.Length + right.Length) - 1),
                                        [parent.Children[0], parent.Children[1], right]
                                    );

                                    tokenIndex = nextMatch.Annotations![0].Children![2].Start;
                                }
                                // else some other format for which we don't have an implementation
                                else
                                {
                                    throw new NotImplementedException("Evaluation of this expression format is not implemented.");
                                }

                                result = previousMatch;
                            }

                            // xxx should be the parent ? xxx test
                            previousMatch = nextMatch;
                        }
                        else
                        {
                            break;
                        }
                    }                    
                }

                return BuildFunctionRuleResult(result.Annotations[0].Range, result.Annotations);
            }

            return ParseResult.Failure;
        }

        private ParseResult FindMatch(T[] input, int start)
        {
            foreach (var option in RuleOptions)
            {   
                var result = option.Parse(input, start);
                if (result.FoundMatch)
                {
                    return result;
                }
            }
            return ParseResult.Failure;
        }

        public void ReplaceSubRule(RuleBase<T> subRule, RuleBase<T> replacement)
        {
            Contract.RequiresNotNull(replacement, $"{nameof(MatchOneOfFunction<T>)} cannot have null as its options.");

            var index = Array.IndexOf(RuleOptions, subRule);
            
            Contract.Requires(index >= 0);

            RuleOptions[index] = replacement;
        }
    }
}
