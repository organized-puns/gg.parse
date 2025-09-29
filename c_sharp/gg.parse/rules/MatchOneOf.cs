namespace gg.parse.rules
{
    public class MatchOneOf<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        private RuleBase<T>[] _options;

        public RuleBase<T>? this[int index] => _options[index];

        public RuleBase<T>[] RuleOptions 
        {
            get => _options;
            set
            {
                Assertions.Requires(value != null);
                Assertions.Requires(value!.Any(v => v != null));

                _options = value!;
            }
        }

        public IEnumerable<RuleBase<T>> Rules => RuleOptions;

        public MatchOneOf(string name, params RuleBase<T>[] options)
            : base(name, IRule.Output.Self)
        {
            RuleOptions = options;
        }

        public MatchOneOf(
            string name, 
            IRule.Output production, 
            int precedence = 0, 
            params RuleBase<T>[] rules
        ) : base(name, production, precedence)
        {
            RuleOptions = rules;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            foreach (var option in RuleOptions)
            {   
                var result = option.Parse(input, start);
                if (result.FoundMatch)
                {
                    List<Annotation>? children = result.Annotations == null || result.Annotations.Count == 0
                            ? null 
                            : [..result.Annotations!];

                    return BuildResult(new(start, result.MatchLength), children);
                }
            }
            return ParseResult.Failure;
        }
    }
}
