namespace gg.parse.rules
{
    public class MatchRuleSequence<T> : RuleBase<T>, IRuleComposition<T>  where T : IComparable<T>
    {
        private RuleBase<T>[] _rules;

        public RuleBase<T>[] SequenceRules
        {
            get => _rules;
            set
            {
                Assertions.Requires(value != null);
                Assertions.Requires(value!.Any( v => v != null));

                _rules = value!;
            }
        }

        public IEnumerable<RuleBase<T>> Rules => SequenceRules;

        public MatchRuleSequence(
            string name, 
            IRule.Output production = IRule.Output.Self, 
            int precedence = 0,
            params RuleBase<T>[] sequence
        ) : base(name, production, precedence) 
        {
            SequenceRules = sequence;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            var index = start;
            List<Annotation>? children = null;

            foreach (var rule in SequenceRules)
            {
                var result = rule.Parse(input, index);
                
                if (!result.FoundMatch)
                {
                    return ParseResult.Failure;
                }

                if (result.Annotations != null && result.Annotations.Count > 0 &&
                   (Production == IRule.Output.Self || Production == IRule.Output.Children))
                {
                    children ??= [];
                    children.AddRange(result.Annotations);
                }

                index += result.MatchLength;
            }

            return BuildResult(new Range(start, index - start), children);
        }
    }
}
