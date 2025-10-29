// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.util;

using Range = gg.parse.util.Range;

namespace gg.parse.rules
{
    public class RuleReference<T> : RuleBase<T> where T : IComparable<T>
    {
        private RuleBase<T>? _rule;

        public string ReferenceName { get; init; }

        public AnnotationPruning ReferencePrune { get; init; }

        public RuleBase<T>? Rule 
        {
            get => _rule;
           
            set
            {
                Assertions.RequiresNotNull(value!);

                _rule = value;
            }
        }

        /// <summary>
        /// If set to true then the result of this rule will be based on the referenced rule's output.
        /// ie this rule will never show up in the result/ast tree.
        /// If false (this is the default) this rule will show up in the ast tree if its annotation output
        /// is set to 'Annotation'.
        /// </summary>
        public bool IsTopLevel { get; set; } = true;

        public RuleReference(
            string name, 
            string reference, 
            AnnotationPruning prune = AnnotationPruning.None,
            int precedence = 0,
            AnnotationPruning referencePruning = AnnotationPruning.None
        )
        : base(name, prune, precedence) 
        {
            ReferenceName = reference;
            ReferencePrune = referencePruning;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            Assertions.RequiresNotNull(Rule);

            var result = Rule.Parse(input, start);

            if (result.FoundMatch)
            {
                // parse behaviour depends on whether this reference is part of a composition (eg sequence)
                // in which case we take in account any prune modifiers applied to this rule, but
                // otherwise pass the results of the referced rule
                if (IsTopLevel)
                {
                    return GetTopLevelResult(result, start);
                }
                else
                {
                    // this rule is part of a sequence/option/oneormore/..., it's assumed this is only to change 
                    // the rule output so pass back the result based on the reference pruning
                    return ReferencePrune switch
                    {
                        AnnotationPruning.None => result,
                        AnnotationPruning.Children => new ParseResult(
                            true,
                            result.MatchLength,
                            CollectRootAnnotations(result.Annotations)
                        ),
                        AnnotationPruning.Root => new ParseResult(
                            true,
                            result.MatchLength,
                            CollectChildAnnotations(result.Annotations)
                        ),
                        _ => new ParseResult(true, result.MatchLength),
                    };
                }
            }

            return result;
        }


        /// <summary>
        /// Deal with the complications of this rule's pruning and the reference rule's pruning.
        /// Cases:
        ///     Prune               | ReferencePrune        | Result
        ///     --------------------|-----------------------|-----------------------
        ///     All                 | Ignored               | Prune all
        ///     Children            | Ignored               | This
        ///     Root                | All                   | Prune all
        ///     Root                | Children              | Ref Result Root
        ///     Root                | Root                  | Ref Result's children
        ///     Root                | None                  | Ref Result Root + Ref Result children 
        ///     None                | All                   | This
        ///     None                | Children              | This with Ref result
        ///     None                | Root                  | This with Ref result children     
        ///     None                | None                  | This with Ref result + Ref result children    
        /// </summary>
        /// <param name="result"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        private ParseResult GetTopLevelResult(ParseResult result, int start)
        {
            return Prune switch
            {
                AnnotationPruning.All => new ParseResult(true, result.MatchLength),
                AnnotationPruning.Children => 
                    new ParseResult(
                        true,
                        result.MatchLength,
                        [new Annotation(this, new Range(start, result.MatchLength))]
                    ),
                AnnotationPruning.Root => GetTopLevelResultWithoutRoot(result),
                AnnotationPruning.None => GetTopLevelResultWithRoot(result, start),
                _ => throw new NotImplementedException(),
            };
        }

        private ParseResult GetTopLevelResultWithoutRoot(ParseResult result)
        {
            return ReferencePrune switch
            {
                AnnotationPruning.All => new ParseResult(true, result.MatchLength),
                AnnotationPruning.Children => 
                    new ParseResult(
                        true,
                        result.MatchLength,
                        CollectRootAnnotations(result.Annotations)
                    ),
                AnnotationPruning.Root => 
                    new ParseResult(
                        true,
                        result.MatchLength,
                        CollectChildAnnotations(result.Annotations)
                    ),
                AnnotationPruning.None => result,
                _ => throw new NotImplementedException(),
            };
        }

        private ParseResult GetTopLevelResultWithRoot(ParseResult result, int start)
        {
            return ReferencePrune switch
            {
                AnnotationPruning.All => 
                    new ParseResult(
                        true,
                        result.MatchLength,
                        [new Annotation(this, new Range(start, result.MatchLength))]
                    ),
                AnnotationPruning.Children => 
                    new ParseResult(
                        true,
                        result.MatchLength,
                        [new Annotation(this, new Range(start, result.MatchLength), CollectRootAnnotations(result.Annotations))]
                    ),
                AnnotationPruning.Root => 
                    new ParseResult(
                        true,
                        result.MatchLength,
                        [new Annotation(this, new Range(start, result.MatchLength), CollectChildAnnotations(result.Annotations))]
                    ),
                AnnotationPruning.None => 
                    new ParseResult(
                        true,
                        result.MatchLength,
                        [new Annotation(this, new Range(start, result.MatchLength), result.Annotations)]
                    ),
                _ => throw new NotImplementedException(),
            };
        }

        private static List<Annotation>? CollectChildAnnotations(List<Annotation>? annotations)
        {
            if (annotations != null)
            {
                var result = new List<Annotation>();

                annotations.ForEach(a =>
                {
                    if (a != null && a.Children != null && a.Children.Count > 0)
                    {
                        result.AddRange(a.Children);
                    }
                });

                return result.Count > 0 ? result : null;
            }

            return null;
        }

        private static List<Annotation>? CollectRootAnnotations(List<Annotation>? annotations)
        {
            if (annotations != null)
            {
                var result = new List<Annotation>();

                annotations.ForEach(a =>
                {
                    if (a != null)
                    {
                        if (a.Children == null || a.Children.Count == 0)
                        {
                            result.Add(a);
                        }
                        else
                        {
                            result.Add(new Annotation(a.Rule, a.Range));
                        }
                    }
                });

                return result.Count > 0 ? result : null;
            }

            return null;
        }
    }
}
