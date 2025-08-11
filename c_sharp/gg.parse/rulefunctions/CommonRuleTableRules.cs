using gg.parse;
using gg.parse.rulefunctions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace gg.parse.rulefunctions
{
    public static class CommonRuleTableRules
    {
        public static MatchDataSequence<char> Literal(this RuleGraph<char> table, string literal) =>
            Literal(table, literal.ToCharArray());

        public static MatchDataSequence<T> Literal<T>(this RuleGraph<T> table, T[] sequence) 
            where T : IComparable<T> =>
        
            Literal(table,
                $"{AnnotationProduct.None.GetPrefix()}{TokenNames.Literal}({string.Concat(sequence)})", 
                AnnotationProduct.None, 
                sequence
            );
       

        public static MatchDataSequence<T> Literal<T>(this RuleGraph<T> table, string name, AnnotationProduct product, T[] sequence)
            where T : IComparable<T> =>
        
            table.TryFindRule(name, out MatchDataSequence<T>? existingRule)
                ? existingRule!
                : table.RegisterRule(new MatchDataSequence<T>(name, sequence, product));

        public static RuleBase<T> Sequence<T>(this RuleGraph<T> table, params T[] data)
            where T : IComparable<T> =>

            table.Sequence($"{AnnotationProduct.None.GetPrefix()}{TokenNames.DataSequence}({string.Join(", ", data)})", 
                        AnnotationProduct.None, 
                        data);
        

        public static RuleBase<T> Sequence<T>(this RuleGraph<T> table, string ruleName, AnnotationProduct product, params T[] data)
            where T : IComparable<T> =>

            table.TryFindRule(ruleName, out MatchDataSequence<T>? existingRule)
                ? existingRule!
                : table.RegisterRule(new MatchDataSequence<T>(ruleName, data, product));


        public static MatchFunctionSequence<T> Sequence<T>(this RuleGraph<T> table, params RuleBase<T>[] functions)
            where T : IComparable<T> =>
            
            table.Sequence($"{AnnotationProduct.None.GetPrefix()}{TokenNames.FunctionSequence}({string.Join(",", functions.Select(f => f.Name))})", 
                AnnotationProduct.None, 
                functions);

        public static MatchFunctionSequence<T> Sequence<T>(this RuleGraph<T> table, string ruleName, AnnotationProduct product, params RuleBase<T>[] functions)
            where T : IComparable<T> =>
            
            table.TryFindRule(ruleName, out MatchFunctionSequence<T>? existingRule)
                ? existingRule!
                : table.RegisterRule(new MatchFunctionSequence<T>(ruleName, product, functions));

        public static MatchOneOfFunction<T> OneOf<T>(this RuleGraph<T> table, params RuleBase<T>[] rules)
             where T : IComparable<T> =>

            table.OneOf($"{AnnotationProduct.None.GetPrefix()}{TokenNames.OneOf}({string.Join(",", rules.Select(f => f.Name))})", 
                AnnotationProduct.None, 
                rules);
        

        public static MatchOneOfFunction<T> OneOf<T>(this RuleGraph<T> table, string name, AnnotationProduct product, params RuleBase<T>[] rules)
             where T : IComparable<T> =>
                table.TryFindRule(name, out MatchOneOfFunction<T>? existingRule)
                     ? existingRule!
                     : table.RegisterRule(new MatchOneOfFunction<T>(name, product, rules));

        public static MatchFunctionCount<T> ZeroOrMore<T>(this RuleGraph<T> table, string name, AnnotationProduct product, RuleBase<T> function)
             where T : IComparable<T> =>
                table.TryFindRule(name, out MatchFunctionCount<T>? existingRule)
                    ? existingRule!
                    : table.RegisterRule(new MatchFunctionCount<T>(name, function, product, 0, 0));

        public static MatchFunctionCount<T> ZeroOrMore<T>(this RuleGraph<T> table, RuleBase<T> function)
             where T : IComparable<T> =>
            table.ZeroOrMore($"{AnnotationProduct.None.GetPrefix()}{TokenNames.ZeroOrMore}({function.Name})", 
                            AnnotationProduct.None, 
                            function);        

            public static MatchFunctionCount<T> ZeroOrOne<T>(this RuleGraph<T> table, RuleBase<T> function)
                where T : IComparable<T> =>
                
                table.ZeroOrOne($"{AnnotationProduct.None.GetPrefix()}{TokenNames.ZeroOrOne}({function.Name})", 
                                AnnotationProduct.None, 
                                function);
        

            public static MatchFunctionCount<T> ZeroOrOne<T>(this RuleGraph<T> table, string name, AnnotationProduct product, RuleBase<T> function)
                where T : IComparable<T> =>
               table.TryFindRule(name, out MatchFunctionCount<T>? existingRule)
                    ? existingRule!
                    : table.RegisterRule(new MatchFunctionCount<T>(name, function, product, 0, 1));


    }
}
