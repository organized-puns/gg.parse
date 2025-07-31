using gg.parse.basefunctions;

namespace gg.parse.rulefunctions
{
    public static class ParseResultFunctions
    {
        public static ParseResult BuildDataRuleResult<T>(this RuleBase<T> rule, Range dataRange) where T : IComparable<T>
        {
            return rule.Production switch
            {
                AnnotationProduction.Annotation => new ParseResult(true, dataRange.Length,
                                        [new Annotation(AnnotationDataCategory.Data, rule.Id, dataRange)]),
                AnnotationProduction.Transitive => 
                    throw new NotImplementedException("Cannot apply transitive production to a rule which has no children"),
                AnnotationProduction.None => new ParseResult(true, dataRange.Length),
                _ => throw new NotImplementedException($"Production rule {rule.Production} is not implemented"),
            };
        }

        public static ParseResult BuildFunctionRuleResult<T>(this RuleBase<T> rule, Range dataRange, List<Annotation>? children = null) 
            where T : IComparable<T>
        {
            return rule.Production switch
            {
                AnnotationProduction.Annotation => new ParseResult(true, dataRange.Length, 
                                        [new Annotation(AnnotationDataCategory.Data, rule.Id, dataRange, children)]),
                AnnotationProduction.Transitive => new ParseResult(true, dataRange.Length, children),
                AnnotationProduction.None => new ParseResult(true, dataRange.Length),
                _ => throw new NotImplementedException($"Production rule {rule.Production} is not implemented"),
            };
        }
    }
}
