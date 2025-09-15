


namespace gg.parse.rulefunctions
{
    /// <summary>
    /// Rule which skips data until some EndCondition is met or EoF is encountered. 
    /// Mainly added to optimize / reduce typing for *(try(!endCondition), (eof | .)).
    /// In script: skip_until {condition} (implicit failOnEoF = true) 
    /// skip_until_eof_or {condition} (implicit failOnEoF = false) 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SkipRule<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        /// <summary>
        /// If initialized to true, this rule will fail when encountering eof (succeed otherwise)
        /// </summary>
        public bool FailOnEoF { get; init; }

        /// <summary>
        /// Condition which will cause this rule to skip input
        /// </summary>
        public RuleBase<T> StopCondition
        {
            get;
            init;
        }

        public IEnumerable<RuleBase<T>> Rules => [StopCondition];

        public SkipRule(
            string name,
            AnnotationProduct product,
            RuleBase<T> condition,
            bool failOnEof = true
        ) 
            : base(name, product)
        {
            StopCondition = condition;
            FailOnEoF = failOnEof;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            var idx = start;
            while (idx < input.Length)
            {
                var conditionalResult = StopCondition.Parse(input, idx);

                if (conditionalResult.FoundMatch)
                {
                    return BuildDataRuleResult(new(start, idx - start));
                }

                idx++;
            }

            return FailOnEoF ? ParseResult.Failure : BuildDataRuleResult(new(start, idx - start));
        }
    }
}
