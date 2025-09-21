namespace gg.parse.rules
{
    public class MatchOneOfFunction<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        private RuleBase<T>[] _options;

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

        public MatchOneOfFunction(string name, params RuleBase<T>[] options)
            : base(name, AnnotationProduct.Annotation)
        {
            RuleOptions = options;
        }

        public MatchOneOfFunction(string name, AnnotationProduct production, int precedence = 0, params RuleBase<T>[] options)
            : base(name, production, precedence)
        {
            RuleOptions = options;
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

                    return BuildFunctionRuleResult(new(start, result.MatchedLength), children);
                }
            }
            return ParseResult.Failure;
        }
    }
}
