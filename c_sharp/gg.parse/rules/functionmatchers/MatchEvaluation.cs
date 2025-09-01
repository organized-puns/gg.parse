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
            var parent = FindMatch(input, start);

            if (parent != null)
            {               
                if (parent.Children == null || parent.Children.Count < 3)
                {                     
                    // unary operator / value, nothing more to evaluate
                    return BuildFunctionRuleResult(parent.Range, [parent]);
                }

                var root = parent;

                // move the token pointer to the end of the last child annotation, so we can try to match another 
                // binary. The assumption here is that the root of this annotation expresses the operator
                // the first child is the left-hand side of the operator, the second child is the operator and the third
                // child is the right-hand side of the operator.
                // so take the third child token position and try to find another match
                var tokenIndex = parent.Children[2].Start;
                
                Annotation? nextMatch;

                // have we run out of tokens ?
                while (tokenIndex < input.Length
                    // is there another match ?
                    && (nextMatch = FindMatch(input, tokenIndex)) != null) 
                {
                    var nextPrecedence = FindRule(nextMatch.FunctionId).Precedence;

                    // move up the right hand side of the tree until we reach the root of the tree
                    // or until we find a node with a lower precedence
                    while (parent != null && FindRule(parent.FunctionId).Precedence >= nextPrecedence)
                    {
                        parent = parent.Parent;
                        
                        if (parent != null)
                        {
                            // remember the node with the higher precedence
                            // this erases the left side of the new match 
                            nextMatch.Children![0] = parent.Children![2];
                        }
                    }

                    if (parent != null)
                    {
                        // reached a node with a lower precedence, so the new match becomes the right child of that node
                        nextMatch.Parent = parent;
                        nextMatch.Children![0].Parent = nextMatch;
                        parent.Children[2] = nextMatch;
                    }
                    else
                    {
                        // reached the root, the root becomes the left side of the new match
                        // and the new match becomes the new root
                        root.Parent = nextMatch;
                        nextMatch.Children[0] = root;
                        root = nextMatch;
                    }

                    parent = nextMatch;
                    tokenIndex = parent.Children[2].Start;
                }
                
                return BuildFunctionRuleResult(UpdateRanges(root), [root]);
            }

            return ParseResult.Failure;
        }

        private Range UpdateRanges(Annotation node)
        {
            return (node.Children == null || node.Children.Count == 0)
                ? node.Range
                : node.Range = Range.Union(node.Children!.Select(c => UpdateRanges(c)));
        }


        private Annotation? FindMatch(T[] input, int start)
        {
            foreach (var option in RuleOptions)
            {   
                var result = option.Parse(input, start);
                if (result.FoundMatch)
                {
                    // validate 
                    // xxx treat this as a user error, not a fatal programming issue
                    // xxx note this can / should be checked when the options are set ?

                    // match should have the form of a root (ast node) with no, or one or more children
                    Contract.Requires(result.Annotations != null
                                    && result.Annotations.Count > 0
                                    && result.Annotations[0] != null, "No annotations found in result. Evaluation result must have exactly 1 annotation.");
                    Contract.Requires(result.Annotations!.Count == 1, "Multiple annotations found. Evaluation result must have exactly 1 annotation.");

                    return result.Annotations[0];
                }
            }
            return null;
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
