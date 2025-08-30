namespace gg.parse.rulefunctions.rulefunctions
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
                    // this rule is anonymous, ie part of a sequence/option/oneormore/..., it's assumed this is only to change 
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
                    // this rule is a named rule we assume the user wants this id to show up (or hide)
                    switch (Production)
                    {
                        case AnnotationProduct.Annotation:

                            return new ParseResult(true, result.MatchedLength,
                                               [new Annotation(Id, new Range(start, result.MatchedLength), result.Annotations)]);

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
    }
}
