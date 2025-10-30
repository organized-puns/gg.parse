// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.util;

using Range = gg.parse.util.Range;

namespace gg.parse.rules
{
    public class MatchEvaluation<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        // we need to wrangle the resulting annotations so wrap the annotation we get from
        // parsing into a mutable structure we can manipulate
        private class MutableAnnotation 
        {
            public Annotation Annotation { get; set; }

            public Range AnnotationRange { get; set; } 

            public MutableAnnotation? Parent { get; set; }

            public int Precedence => Annotation.Rule.Precedence;

            public IRule Rule => Annotation.Rule;

            public MutableAnnotation? Left { get; set; }

            public MutableAnnotation? Right { get; set; }

            public Annotation? Operation { get; set; }

            public int TokenStart => AnnotationRange.Start;

            public MutableAnnotation(Annotation annotation)
            {
                Annotation = annotation;
                AnnotationRange = annotation.Range;

                if (Annotation.Children != null)
                {
                    Left = Annotation.Children.Count >= 1
                        ? new MutableAnnotation(Annotation.Children[0]) 
                        : null;

                    Operation = Annotation.Children.Count >=  2 
                        ? Annotation.Children[1] 
                        : null;


                    Right = Annotation.Children.Count >= 3
                        ? new MutableAnnotation(Annotation.Children[2])
                        : null;
                }
            }

            public Range UpdateRanges()
            {
                if (Left != null)
                {
                    if (Right == null)
                    {
                        AnnotationRange = Left.UpdateRanges();
                    }
                    else
                    {
                        Left.UpdateRanges();
                        Right.UpdateRanges();
                        AnnotationRange = new Range(Left.TokenStart, Right.Annotation.End - Left.TokenStart);
                    }
                }

                return AnnotationRange;
            }

            public override string ToString()
            {
                return "mutable:" + Annotation.ToString();
            }

        }

        private RuleBase<T>[] _options;
        
        public RuleBase<T>[] RuleOptions => _options;

        public int Count => _options == null ? 0 : _options.Length;

        public RuleBase<T>? this[int index] => _options[index];
        
        public IEnumerable<RuleBase<T>> Rules => RuleOptions;

        public MatchEvaluation(string name, params RuleBase<T>[] options)
            : base(name, AnnotationPruning.None)
        {
            Assertions.RequiresNotNull(options);
            Assertions.Requires(options!.Any(v => v != null));

            _options = options;
        }

        public MatchEvaluation(string name, AnnotationPruning output, int precedence, params RuleBase<T>[] options)
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
                var parent = new MutableAnnotation(parentResult.Annotations![0]);

                // any more input left
                if (parent.Right == null)
                {                     
                    // unary operator / value, nothing more to evaluate
                    return BuildResult(parent.AnnotationRange, [parent.Annotation]);
                }

                var root = parent;
                var tokensRead = parentResult.MatchLength;

                // move the token pointer to the end of the last child annotation, so we can try to match another 
                // binary. The assumption here is that the root of this annotation expresses the operator
                // the first child is the left-hand side of the operator, the second child is the operator and the third
                // child is the right-hand side of the operator.
                // so take the third child token position and try to find another match
                var tokenIndex = parent.Right.TokenStart;
                
                ParseResult nextMatchResult;

                // have we run out of tokens ?
                while (tokenIndex < input.Length
                    // is there another match ?
                    && (nextMatchResult = FindMatch(input, tokenIndex))) 
                {
                    var nextMatch = new MutableAnnotation(nextMatchResult.Annotations![0]);

                    // need to take in account that we've encountered an error in the remainder
                    if (nextMatch.Rule is LogRule<T> logRule)
                    {
                        if (logRule.Level == LogLevel.Error)
                        {
                            return BuildResult(new Range(start, tokenIndex - start), [nextMatch.Annotation]);
                        }
                        // else this is unexpected but we should assume the user knows what they are doing
                        // (until there's reason to believe otherwise)
                    }

                    tokensRead += nextMatchResult.MatchLength - 1;

                    // move up the right hand side of the tree until we reach the root of the tree
                    // or until we find a node with a lower precedence
                    while (parent != null && parent.Precedence >= nextMatch.Precedence)
                    {
                        parent = parent.Parent;

                        if (parent != null)
                        {
                            // remember the node with the higher precedence, if we find a node with a lower precedence
                            // we will use this node as the left side of the new match

                            // note: this erases the left side of the next match - which is correct
                            // since the left side was the end of the previous match which had a higher precedence
                            // (and therefore owns that)

                            nextMatch.Left = parent.Right;
                        }
                    }

                    if (parent != null)
                    {
                        // reached a node with a lower precedence, so the new match becomes the right child of that node
                        //      parent
                        //          \
                        //          nextMatch   
                        //           /    \
                        //               current value
                        nextMatch.Parent = parent;

                        // in the previous while loop we captured the node with the higher precedence
                        // this will become our left
                        nextMatch.Left!.Parent = nextMatch;
                        parent.Right = nextMatch;
                    }
                    else
                    {
                        // reached the root, the root becomes the left side of the new match
                        // and the new match becomes the new root
                        root.Parent = nextMatch;
                        nextMatch.Left = root;
                        root = nextMatch;
                    }
                    
                    tokenIndex = nextMatch.Right!.TokenStart;
                    parent = nextMatch;
                }

                root.UpdateRanges();

                // root can span more tokens than comes from the result of the range union called inside
                // updateRanges.
                // eg when tokens are dropped at the end of a match, eg: group = ~group_start, expression, ~group_end;
                // while this will yield correct results for the internal nodes of the evaluation,
                // the root may return less tokens read as a result.
                // So set the range of the root according to the tokens read
                root.AnnotationRange = new Range(start, tokensRead);

                return BuildResult(root.AnnotationRange, [ComposeAnnotationTree(root, null)]);
            }

            return ParseResult.Failure;
        }

        private static Annotation ComposeAnnotationTree(MutableAnnotation node, MutableAnnotation? parent)
        {
            List<Annotation>? children = null;

            if (node.Left != null)
            {
                children = [ComposeAnnotationTree(node.Left, node)];

                if (node.Operation != null)
                {
                    children.Add(node.Operation);
                }

                if (node.Right != null)
                {
                    children.Add(ComposeAnnotationTree(node.Right, node));
                }
            }

            return new Annotation(
                node.Rule,
                node.AnnotationRange,
                children == null ? null : [.. children],
                parent?.Annotation);
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

        public IRuleComposition<T> CloneWithComposition(IEnumerable<RuleBase<T>> composition) =>
            new MatchEvaluation<T>(Name, Prune, Precedence, [.. composition]);

        public void MutateComposition(IEnumerable<RuleBase<T>> composition)
        {
            Assertions.RequiresNotNull(composition);

            _options = [.. composition];
        }
    }
}
