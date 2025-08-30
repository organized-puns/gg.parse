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

        public MatchEvaluation(string name, AnnotationProduct production, params RuleBase<T>[] options)
            : base(name, production)
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

                Contract.Requires(root.Children != null && root.Children.Count > 0, "No child annotations found in annotation. Evaluation annotation must have at least 1 child annotation.");

                if (root.Children!.Count > 1)
                {
                    // move the token pointer to the end of the last child annotation, so we can try to match another 
                    // binary. The assumption here is that the root of this annotation expresses the operator
                    // the first child is the left-hand side of the operator, the second child is the operator and the third
                    // child is the right-hand side of the operator.
                    // so take the third child token position and try to find another match
                    var tokenIndex = root.Children[2].Start;

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

                            // update the tree baed on precendece, where the higher precendence rule is (should)
                            // be evaluated first.
                            if (previousMatchRule.Precedence >= nextMatchRule.Precedence)
                            {
                                var parent = nextMatch.Annotations![0];
                                var left = previousMatch.Annotations![0];

                                nextMatch.Annotations![0] = new Annotation(
                                    parent.FunctionId,
                                    new Range(left.Start, (left.Length + parent.Length) - 1),
                                    [left, parent.Children[1], parent.Children[2]]
                                );

                                result = nextMatch;
                            }
                            else
                            {
                                var parent = previousMatch.Annotations![0];
                                var right = nextMatch.Annotations![0];

                                previousMatch.Annotations![0] = new Annotation(
                                    parent.FunctionId,
                                    new Range(parent.Start, (parent.Length + right.Length) - 1),
                                    [parent.Children[0], parent.Children[1], right]
                                );

                                result = previousMatch;
                            }

                            tokenIndex = nextMatch.Annotations![0].Children![2].Start;                            
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
