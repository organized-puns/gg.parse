namespace gg.parse.rules
{
    public class RuleReference<T> : RuleBase<T> where T : IComparable<T>
    {
        public string Reference { get; init; }

        public RuleBase<T>? Rule { get; set; }

        /// <summary>
        /// If set to true then the result of this rule will be based on the referenced rule's production.
        /// ie this rule will never show up in the result/ast tree.
        /// If false (this is the default) this rule will show up in the ast tree if its annotation production
        /// is set to 'Annotation'.
        /// </summary>
        public bool DeferResultToReference { get; set; } = false;

        public RuleReference(string name, string reference, IRule.Output product = IRule.Output.Self, int precedence = 0)
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
                // in which case we take in account any production modifiers applied to this rule, but
                // otherwise pass the results of the refereed rule
                if (DeferResultToReference)
                {
                    // this rule is part of a sequence/option/oneormore/..., it's assumed this is only to change 
                    // the rule production so pass back the result based on this' product
                    return Production switch
                    {
                        IRule.Output.Self => result,
                        IRule.Output.Children => new ParseResult(true, result.MatchedLength, result.Annotations),
                        _ => new ParseResult(true, result.MatchedLength),
                    };
                }
                else
                {
                    // this rule is a named rule we _assume+ the user wants this rule to show up in the result/asttree
                    // rather than the referred rule (for whatever the motivations are of the user).
                    // eg let's say the user states foo = 'bar'; bar = foo; in this case the rule 'bar' has its own name
                    // so the results should include 'bar' as the rule name, not 'foo'. Bar may still have any production
                    // modifiers eg "#bar = foo;" in which case foo will show up.
                    return Production switch
                    {
                        IRule.Output.Self => new ParseResult(true, result.MatchedLength,
                                                                       [new Annotation(this, new Range(start, result.MatchedLength), result.Annotations)]),
                        IRule.Output.Children => result,
                        _ => new ParseResult(true, result.MatchedLength),
                    };
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
