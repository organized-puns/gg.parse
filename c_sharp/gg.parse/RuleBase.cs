// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;
using Range = gg.parse.util.Range;

namespace gg.parse
{
    public abstract class RuleBase<T>(
        string name, 
        AnnotationPruning output = AnnotationPruning.None, 
        int precedence = 0) 
        : IRule
    {
        public string Name { get; init; } = name;

        public int Id { get; init; } = name.GetHashCode();

        public int Precedence { get; init; } = precedence;

        public AnnotationPruning Prune { get; init; } = output;

        
        public abstract ParseResult Parse(T[] input, int start);

        public override string ToString() => Name;


        public ParseResult BuildDataRuleResult(Range dataRange) =>
        
            Prune switch
            {
                AnnotationPruning.All =>
                    new ParseResult(true, dataRange.Length),

                // in case of a data rule, all remaining cases produce the root as data rules should not have
                // children.
                AnnotationPruning.Children =>                
                    new ParseResult(true, dataRange.Length),
                //new ParseResult(true, dataRange.Length, [new Annotation(this, dataRange)]),

                AnnotationPruning.None => 
                    new ParseResult(true, dataRange.Length, [new Annotation(this, dataRange)]),

                // data rules do not have children as they are not composed of other rules
                // so when their children being asked, nothing is left
                AnnotationPruning.Root =>
                    new ParseResult(true, dataRange.Length),
                //new ParseResult(true, dataRange.Length, [new Annotation(this, dataRange)]),               

                _ => throw new NotImplementedException($"No implementation to build a data rule result for enum value {Prune}."),
            };
        

        public ParseResult BuildResult(Range dataRange, ImmutableList<Annotation>? children = null)
        {
            return Prune switch
            {
                AnnotationPruning.All => 
                    new ParseResult(true, dataRange.Length),

                AnnotationPruning.Children => 
                    new ParseResult(true, dataRange.Length, [new Annotation(this, dataRange)]),

                AnnotationPruning.None => 
                    new ParseResult(true, dataRange.Length, [new Annotation(this, dataRange, children) ]),

                AnnotationPruning.Root => 
                    new ParseResult(true, dataRange.Length, children),
                                
                _ => throw new NotImplementedException($"No implementation to build a rule result for enum value {Prune}."),
            };
        }
    }
}
