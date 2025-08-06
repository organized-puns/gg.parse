namespace gg.parse.rulefunctions
{
    public static class ParseResultFunctions
    {
        public static ParseResult BuildDataRuleResult<T>(this RuleBase<T> rule, Range dataRange) where T : IComparable<T>
        {
            return rule.Production switch
            {
                AnnotationProduct.Annotation => new ParseResult(true, dataRange.Length,
                                        [new Annotation(rule.Id, dataRange)]),
                AnnotationProduct.Transitive => 
                    throw new NotImplementedException("Cannot apply transitive production to a rule which has no children"),
                AnnotationProduct.None => new ParseResult(true, dataRange.Length),
                _ => throw new NotImplementedException($"Production rule {rule.Production} is not implemented"),
            };
        }

        public static ParseResult BuildFunctionRuleResult<T>(this RuleBase<T> rule, Range dataRange, List<Annotation>? children = null) 
            where T : IComparable<T>
        {
            return rule.Production switch
            {
                AnnotationProduct.Annotation => new ParseResult(true, dataRange.Length, 
                                        [new Annotation(rule.Id, dataRange, children)]),
                AnnotationProduct.Transitive => new ParseResult(true, dataRange.Length, children),
                AnnotationProduct.None => new ParseResult(true, dataRange.Length),
                _ => throw new NotImplementedException($"Production rule {rule.Production} is not implemented"),
            };
        }
    }
}
