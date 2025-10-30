// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections;
using System.Collections.Immutable;

using Range = gg.parse.util.Range;

namespace gg.parse
{
    public class Annotation : IEnumerable<Annotation>
    {
        /// <summary>
        /// Allows matching the annotation against a string based on the set rule. Applicable in a lot of cases where
        /// we compare token names or grammar rules.
        /// </summary>
        /// <param name="annotation"></param>
        public static implicit operator string(Annotation annotation) => 
            annotation.Rule == null ? "" : annotation.Rule.Name;

        /// <summary>
        /// Range in the data which this annotation spans
        /// </summary>
        public Range Range { get; init; }
        
        public int Start => Range.Start;

        public int End => Range.Start + Range.Length;

        public int Length => Range.Length;

        /// <summary>
        /// Rule which produced this annotation.
        /// </summary>
        public IRule Rule { get; init; }

        public ImmutableList<Annotation>? Children { get; init; }

        public Annotation? Parent { get; private set; }

        /// <summary>
        /// Shorthand for children.count, checks if children is null and returns 0 
        /// otherwise returns Children.Count
        /// </summary>
        public int Count => Children == null ? 0 : Children.Count;

        /// <summary>
        /// short hand for Rule.Name
        /// </summary>
        public string Name => Rule.Name;


        public Annotation? this[int index] => 
            Children == null 
            ? null 
            : Children![index];

        public Annotation(IRule rule, Range range, ImmutableList<Annotation>? children = null, Annotation? parent = null)
        {
            Rule = rule;
            Range = range;
            Children = children;
            Parent = parent;

            children?.ForEach(c => c.Parent = this);
        }

        public override string ToString() =>
            $"({Rule}, {Range.Start}..{Range.End})";

        public IEnumerator<Annotation> GetEnumerator()
        {
            return Children == null
                ? Enumerable.Empty<Annotation>().GetEnumerator()
                : Children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
