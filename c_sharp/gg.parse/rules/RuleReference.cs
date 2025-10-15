using gg.parse.util;

using Range = gg.parse.util.Range;

namespace gg.parse.rules
{
    public class RuleReference<T> : RuleBase<T> where T : IComparable<T>
    {
        private RuleBase<T>? _rule;

        public string Reference { get; init; }

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
        public bool DeferResultToReference { get; set; } = false;

        public RuleReference(string name, string reference, RuleOutput product = RuleOutput.Self, int precedence = 0)
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
                // in which case we take in account any  output modifiers applied to this rule, but
                // otherwise pass the results of the referced rule
                if (DeferResultToReference)
                {
                    // this rule is part of a sequence/option/oneormore/..., it's assumed this is only to change 
                    // the rule output so pass back the result based on this' product
                    return Output switch
                    {
                        RuleOutput.Self => result,
                        RuleOutput.Children => new ParseResult(
                            true, 
                            result.MatchLength, 
                            CollectChildAnnotations(result.Annotations)
                        ),
                        _ => new ParseResult(true, result.MatchLength),
                    };
                }
                else
                {
                    // this rule is a named rule we _assume+ the user wants this rule to show up in the result/asttree
                    // rather than the referred rule (for whatever the motivations are of the user).
                    // eg let's say the user states foo = 'bar'; bar = foo; in this case the rule 'bar' has its own name
                    // so the results should include 'bar' as the rule name, not 'foo'. Bar may still have any output
                    // modifiers eg "#bar = foo;" in which case foo will show up.
                    return Output switch
                    {
                        RuleOutput.Self => new ParseResult(true, result.MatchLength,
                                                                       [new Annotation(this, new Range(start, result.MatchLength), result.Annotations)]),
                        RuleOutput.Children => result,
                        _ => new ParseResult(true, result.MatchLength),
                    };
                }
            }

            return result;
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
    }
}
