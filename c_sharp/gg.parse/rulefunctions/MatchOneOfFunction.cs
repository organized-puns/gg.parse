using static System.Runtime.InteropServices.JavaScript.JSType;

namespace gg.parse.rulefunctions
{
    public class MatchOneOfFunction<T> : RuleBase<T>, IRuleComposition<T> where T : IComparable<T>
    {
        public RuleBase<T>[] Options { get; set; }

        public IEnumerable<RuleBase<T>> SubRules => Options;

        public MatchOneOfFunction(string name, params RuleBase<T>[] options)
            : base(name, AnnotationProduct.Annotation)
        {
            Options = options;
        }

        public MatchOneOfFunction(string name, AnnotationProduct production, params RuleBase<T>[] options)
            : base(name, production)
        {
            Options = options;
        }


        public override ParseResult Parse(T[] input, int start)
        {
            foreach (var option in Options)
            {
                var result = option.Parse(input, start);
                if (result.IsSuccess)
                {
                    List<Annotation>? children = (result.Annotations == null || result.Annotations.Count == 0)
                            ? null 
                            : [..result.Annotations!];

                    return this.BuildFunctionRuleResult(new(start, result.MatchedLength), children);
                }
            }
            return ParseResult.Failure;
        }
    }
}
