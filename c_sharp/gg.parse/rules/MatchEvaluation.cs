// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun


using gg.parse.util;
using System.Diagnostics.CodeAnalysis;
using Range = gg.parse.util.Range;

namespace gg.parse.rules
{
    public class MatchEvaluation<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        private RuleBase<T>[] _options;

        [DisallowNull]
        public RuleBase<T>[] RuleOptions 
        {
            get => _options;
            set
            {
                Assertions.RequiresNotNull(value);
                Assertions.Requires(value!.Any(v => v != null));

                _options = value;
            }
        }

        public int Count => _options == null ? 0 : _options.Length;

        public RuleBase<T>? this[int index]
        {
            get => _options[index];
            set
            {
                Assertions.RequiresNotNull(value!);
                Assertions.RequiresNotNull(_options!);
                
                _options[index] = value!;
            }
        }

        public IEnumerable<RuleBase<T>> Rules => RuleOptions;

        public MatchEvaluation(string name, params RuleBase<T>[] options)
            : base(name, RuleOutput.Self)
        {
            Assertions.RequiresNotNull(options);
            Assertions.Requires(options!.Any(v => v != null));

            _options = options;
        }

        public MatchEvaluation(string name, RuleOutput output, int precedence, params RuleBase<T>[] options)
            : base(name, output, precedence)
        {
            Assertions.Requires(options != null);
            Assertions.Requires(options!.Any(v => v != null));

            _options = options!;
        }

        /// <summary>
        /// Try to parse an evaluation expression taking in account operator precedence.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public override ParseResult Parse(T[] input, int start)
        {            
            var parentResult = FindMatch(input, start);

            if (parentResult)
            {               
                var parent = parentResult.Annotations![0];

                // any more input left
                if (parent.Children == null || parent.Children.Count < 3)
                {                     
                    // unary operator / value, nothing more to evaluate
                    return BuildResult(parent.Range, [parent]);
                }

                var root = parent;
                var tokensRead = parentResult.MatchLength;

                // move the token pointer to the end of the last child annotation, so we can try to match another 
                // binary. The assumption here is that the root of this annotation expresses the operator
                // the first child is the left-hand side of the operator, the second child is the operator and the third
                // child is the right-hand side of the operator.
                // so take the third child token position and try to find another match
                var tokenIndex = parent.Children[2].Start;
                
                ParseResult nextMatchResult;

                // have we run out of tokens ?
                while (tokenIndex < input.Length
                    // is there another match ?
                    && (nextMatchResult = FindMatch(input, tokenIndex))) 
                {
                    var nextMatch = nextMatchResult.Annotations![0];

                    // need to take in account that we've encountered an error in the remainder
                    if (nextMatch.Rule is LogRule<T> logRule)
                    {
                        if (logRule.Level == LogLevel.Error)
                        {
                            return BuildResult(new Range(start, tokenIndex - start), [nextMatch]);
                        }
                        // else this is unexpected but we should assume the user knows what they are doing
                        // (until there's reason to believe otherwise)
                    }

                    var nextPrecedence = nextMatch.Rule.Precedence;

                    tokensRead += nextMatchResult.MatchLength - 1;

                    // move up the right hand side of the tree until we reach the root of the tree
                    // or until we find a node with a lower precedence
                    while (parent != null && parent.Rule.Precedence >= nextPrecedence)
                    {
                        parent = parent.Parent;
                        
                        if (parent != null)
                        {
                            // remember the node with the higher precedence, if we find a node with a lower precedence
                            // we will use this node as the left side of the new match

                            // note: this erases the left side of the next match - which is correct
                            // since the left side was the end of the previous match which had a higher precedence
                            // (and therefore owns that)

                            nextMatch.Children![0] = parent.Children![2];
                        }
                    }

                    if (parent != null)
                    {
                        // reached a node with a lower precedence, so the new match becomes the right child of that node
                        nextMatch.Parent = parent;
                        nextMatch.Children![0].Parent = nextMatch;
                        parent.Children![2] = nextMatch;
                    }
                    else
                    {
                        // reached the root, the root becomes the left side of the new match
                        // and the new match becomes the new root
                        root.Parent = nextMatch;
                        nextMatch.Children![0] = root;
                        root = nextMatch;
                    }
                    
                    tokenIndex = nextMatch.Children[2].Start;
                    parent = nextMatch;
                }

                UpdateRanges(root);

                // root can span more tokens than comes from the result of the range union called inside
                // updateRanges.
                // eg when tokens are dropped at the end of a match, eg: group = ~group_start, expression, ~group_end;
                // while this will yield correct results for the internal nodes of the evaluation,
                // the root may return less tokens read as a result.
                // So set the range of the root according to the tokens read
                root.Range = new Range(start, tokensRead);

                return BuildResult(root.Range, [root]);
            }

            return ParseResult.Failure;
        }

        private static Range UpdateRanges(Annotation node)
        {
            return node.Children == null || node.Children.Count == 0
                ? node.Range
                : node.Range = Range.Union(node.Children!.Select(c => UpdateRanges(c)));
        }

        private ParseResult FindMatch(T[] input, int start)
        {
            foreach (var option in RuleOptions)
            {   
                var result = option.Parse(input, start);
                if (result)
                {
                    // validate 
                    // xxx treat this as a user error, not a fatal programming issue
                    // xxx note this can / should be checked when the options are set ?

                    // match should have the form of a root (ast node) with no, or one or more children
                    Assertions.Requires(result.Annotations != null
                                    && result.Annotations.Count > 0
                                    && result.Annotations[0] != null, "No annotations found in result. Evaluation result must have exactly 1 annotation.");
                    Assertions.Requires(result.Annotations!.Count == 1, "Multiple annotations found. Evaluation result must have exactly 1 annotation.");

                    return result;
                }
            }
            return ParseResult.Failure;
        }       
    }
}
