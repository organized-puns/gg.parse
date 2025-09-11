
namespace gg.parse.rulefunctions.rulefunctions
{
    public class MatchFunctionCount<T>(
        string name, 
        RuleBase<T> function, 
        AnnotationProduct production = AnnotationProduct.Annotation, 
        int min = 1, 
        int max = 1, 
        int precedence = 0
    ) : RuleBase<T>(name, production, precedence), IRuleComposition<T> where T : IComparable<T>
    {
        public RuleBase<T> Function { get; private set; } = function;
        
        public int Min { get; } = min;
        
        public int Max { get; } = max;

        public IEnumerable<RuleBase<T>> SubRules => [Function];

        public override ParseResult Parse(T[] input, int start)
        {
            int count = 0;
            int index = start;
            List<Annotation>? children = null;

            while (index < input.Length && (Max <= 0 || count < Max))
            {
                var result = Function.Parse(input, index);
                if (!result.FoundMatch)
                {
                    break;
                }
        
                if (result.MatchedLength == 0 && Max <= 0)
                {
                    throw new InvalidProgramException($"Rule {Name} detected an infinite loop with its subrule {Function.Name}.");
                }

                count++;
                index += result.MatchedLength;

                if (result.Annotations != null && result.Annotations.Count > 0 &&
                    (Production == AnnotationProduct.Annotation || Production == AnnotationProduct.Transitive))
                {
                    children ??= [];
                    children.AddRange(result.Annotations);
                }
            }

            return Min <= 0 || count >= Min
                ? this.BuildFunctionRuleResult(new Range(start, index - start), children)
                : ParseResult.Failure;
        }
    }
}
