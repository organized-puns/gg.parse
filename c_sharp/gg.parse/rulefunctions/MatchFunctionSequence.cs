using gg.parse.basefunctions;

namespace gg.parse.rulefunctions
{
    public class MatchFunctionSequence<T>(string name, AnnotationProduction production = AnnotationProduction.Annotation, params RuleBase<T>[] sequence) 
        : RuleBase<T>(name, production), IRuleComposition<T>
        where T : IComparable<T>
    {
        public RuleBase<T>[] Sequence { get; set; } = sequence;

        public IEnumerable<RuleBase<T>> SubRules => Sequence;

        public override ParseResult Parse(T[] input, int start)
        {
            var index = start;
            var children = new List<Annotation>();

            foreach (var function in Sequence)
            {
                var result = function.Parse(input, index);
                
                if (!result.IsSuccess)
                {
                    return ParseResult.Failure;
                }

                if (Production == AnnotationProduction.Annotation || Production == AnnotationProduction.Transitive)
                {
                    if (result.Annotations != null && result.Annotations.Count > 0)
                    {
                        children.AddRange(result.Annotations);
                    }
                }

                index += result.MatchedLength;
            }

            return this.BuildFunctionRuleResult(new Range(start, index - start), children);
        }
    }
}
