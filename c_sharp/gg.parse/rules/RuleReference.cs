namespace gg.parse.rules
{
    public class RuleReference<T> : RuleBase<T> where T : IComparable<T>
    {
        public string Reference { get; init; }

        public RuleBase<T>? Rule { get; set; }

        /// <summary>
        /// Should be set during resolve
        /// </summary>
        public bool IsPartOfComposition { get; set; } = false;

        public RuleReference(string name, string reference, AnnotationProduct product = AnnotationProduct.Annotation, int precedence = 0)
            : base(name, product, precedence) 
        {
            Reference = reference;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            var result = Rule.Parse(input, start);

            if (result.FoundMatch)
            {
                // parse behaviour depends on whether this reference is part of a composition (eg sequence)
                // or as 'rename' eg renamed_rule = ~old_name;
                if (IsPartOfComposition)
                {
                    // this rule is part of a sequence/option/oneormore/..., it's assumed this is only to change 
                    // the rule production so pass back the result based on this' product
                    switch (Production)
                    {
                        case AnnotationProduct.Annotation:

                            return result;

                        case AnnotationProduct.Transitive:

                            return new ParseResult(true, result.MatchedLength, result.Annotations);

                        case AnnotationProduct.None:
                        default:

                            return new ParseResult(true, result.MatchedLength);
                    }
                }
                else
                {
                    // this rule is a named rule we assume the user wants this rule to show up in the result
                    // rather than the referred rule (for whatever the motivations are of the user).
                    // eg foo = 'bar'; bar = foo; => bar will show up in the result, not foo.
                    switch (Production)
                    {
                        case AnnotationProduct.Annotation:

                            return new ParseResult(true, result.MatchedLength,
                                               [new Annotation(this, new Range(start, result.MatchedLength), result.Annotations)]);

                        case AnnotationProduct.Transitive:

                            return result;

                        case AnnotationProduct.None:
                        default:

                            return new ParseResult(true, result.MatchedLength);
                    }
                }
            }

            return result;
        }

        public override string ToString()
        {
            return Rule == null
                ? base.ToString()
                : $"ref_to:{Rule.ToString()}({base.ToString()})";
        }
    }
}
