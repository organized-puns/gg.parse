namespace gg.parse
{
    public class Annotation : IComparable
    {
        
        public Range Range { get; set; }
        
        public int Start => Range.Start;

        public int End => Range.Start + Range.Length;

        public int Length => Range.Length;

        /// <summary>
        /// Function which produced this annotation.
        /// </summary>
        public int RuleId { get; init; }

        public List<Annotation>? Children { get; init; }

        public Annotation? Parent { get; set; }

        public Annotation? this[int index] => 
            Children == null 
            ? null 
            : Children![index];


#if DEBUG
        // Defaults to the annotation's name. Generally will be overwritten by a rule
        // with the rule name during parsing and constructing of the parse results.
        public string DebugName { get; set; } = nameof(Annotation);
#endif

        public Annotation(int functionId, Range range, List<Annotation>? children = null, Annotation? parent = null)
        {
            RuleId = functionId;
            Range = range;
            Children = children;
            Parent = parent;

            if (children != null)
            {
                children.ForEach(c => c.Parent = this);
            }
        }

        public int CompareTo(object? obj)
        {
            if (obj is Annotation other)
            {
                return RuleId.CompareTo(other.Range.Start);
            }

            return 0;
        }

        public override string ToString() =>
        
#if DEBUG
            $"{DebugName}({RuleId}, {Range})";
#else
            $"Annotation(Id: {FunctionId}, Range: {Range})";
#endif

        /// <summary>
        /// Checks if this annotation matches the predicate, if so adds it to the target. Then
        /// does the same for all its children (if any)
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="target"></param>
        /// <returns>target</returns>
        public List<Annotation> Collect(Func<Annotation, bool> predicate, List<Annotation>? target = null)
        {
            target ??= [];

            if (predicate(this))
            {
                target.Add(this);
            }

            if (Children != null && Children.Count > 0)
            {
                foreach (var child in Children)
                {
                    child.Collect(predicate, target);
                }
            }

            return target;
        }
    }
}
