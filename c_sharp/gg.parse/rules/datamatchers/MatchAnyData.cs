namespace gg.parse.rulefunctions.datafunctions
{
    public class MatchAnyData<T>(
        string name, 
        AnnotationProduct production = AnnotationProduct.Annotation, 
        int precedence = 0
     ) : RuleBase<T>(name, production, precedence) where T : IComparable<T>
    {
        public override ParseResult Parse(T[] input, int start)
        {
            if (start < input.Length)
            {
                return this.BuildDataRuleResult(new Range(start, 1));
            }

            return ParseResult.Failure;
        }
    }
}
