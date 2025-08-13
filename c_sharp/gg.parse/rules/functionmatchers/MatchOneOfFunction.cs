using gg.core.util;
using gg.parse.rulefunctions;

namespace gg.parse.rulefunctions.rulefunctions
{
    public class MatchOneOfFunction<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        private RuleBase<T>[] _options;

        public RuleBase<T>[] RuleOptions 
        {
            get => _options;
            set
            {
                Contract.Requires(value != null);
                Contract.Requires(value!.Any(v => v != null));

                _options = value!;
            }
        }

        public IEnumerable<RuleBase<T>> SubRules => RuleOptions;

        public MatchOneOfFunction(string name, params RuleBase<T>[] options)
            : base(name, AnnotationProduct.Annotation)
        {
            RuleOptions = options;
        }

        public MatchOneOfFunction(string name, AnnotationProduct production, params RuleBase<T>[] options)
            : base(name, production)
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

                    return this.BuildFunctionRuleResult(new(start, result.MatchedLength), children);
                }
            }
            return ParseResult.Failure;
        }

        public void ReplaceSubRule(RuleBase<T> subRule, RuleBase<T> replacement)
        {
            Contract.RequiresNotNull(replacement, $"{nameof(MatchOneOfFunction<T>)} cannot have null as its options.");

            var index = Array.IndexOf(RuleOptions, subRule);
            
            Contract.Requires(index >= 0);

            RuleOptions[index] = replacement;
        }
    }
}
