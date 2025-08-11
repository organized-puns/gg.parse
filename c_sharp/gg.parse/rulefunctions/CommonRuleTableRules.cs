namespace gg.parse.rulefunctions
{
    public static class CommonRuleTableRules
    {
        public static MatchDataSequence<char> Literal(this RuleTable<char> table, string literal) =>
            Literal(table, literal.ToCharArray());

        public static MatchDataSequence<T> Literal<T>(this RuleTable<T> table, T[] sequence) 
            where T : IComparable<T> =>
        
            Literal(table,
                $"{AnnotationProduct.None.GetPrefix()}{TokenNames.Literal}({string.Concat(sequence)})", 
                AnnotationProduct.None, 
                sequence
            );
       

        public static MatchDataSequence<T> Literal<T>(this RuleTable<T> table, string name, AnnotationProduct product, T[] sequence)
            where T : IComparable<T> =>
        
            table.TryFindRule(name, out MatchDataSequence<T>? existingRule)
                ? existingRule!
                : table.RegisterRule(new MatchDataSequence<T>(name, sequence, product));

        public static RuleBase<T> Sequence<T>(this RuleTable<T> table, params T[] data)
            where T : IComparable<T> =>

            table.Sequence($"{AnnotationProduct.None.GetPrefix()}{TokenNames.DataSequence}({string.Join(", ", data)})", 
                        AnnotationProduct.None, 
                        data);
        

        public static RuleBase<T> Sequence<T>(this RuleTable<T> table, string ruleName, AnnotationProduct product, params T[] data)
            where T : IComparable<T> =>

            table.TryFindRule(ruleName, out MatchDataSequence<T>? existingRule)
                ? existingRule!
                : table.RegisterRule(new MatchDataSequence<T>(ruleName, data, product));


        public static MatchFunctionSequence<T> Sequence<T>(this RuleTable<T> table, params RuleBase<T>[] functions)
            where T : IComparable<T> =>
            
            table.Sequence($"{AnnotationProduct.None.GetPrefix()}{TokenNames.FunctionSequence}({string.Join(",", functions.Select(f => f.Name))})", 
                AnnotationProduct.None, 
                functions);

        public static MatchFunctionSequence<T> Sequence<T>(this RuleTable<T> table, string ruleName, AnnotationProduct product, params RuleBase<T>[] functions)
            where T : IComparable<T> =>
            
            table.TryFindRule(ruleName, out MatchFunctionSequence<T>? existingRule)
                ? existingRule!
                : table.RegisterRule(new MatchFunctionSequence<T>(ruleName, product, functions));
    }
}
