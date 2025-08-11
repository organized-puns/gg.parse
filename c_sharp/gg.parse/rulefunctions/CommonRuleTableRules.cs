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
        

            public static MatchFunctionCount<T> ZeroOrOne<T>(this RuleGraph<T> graph, string name, AnnotationProduct product, RuleBase<T> function)
                where T : IComparable<T> =>
               graph.TryFindRule(name, out MatchFunctionCount<T>? existingRule)
                    ? existingRule!
                    : graph.RegisterRule(new MatchFunctionCount<T>(name, function, product, 0, 1));

        public static MatchSingleData<T> Single<T>(this RuleGraph<T> graph, string name, AnnotationProduct product, T tokenId)
            where T : IComparable<T> =>
            graph.TryFindRule(name, out MatchSingleData<T>? existingRule)
                 ? existingRule!
                 : graph.RegisterRule(new MatchSingleData<T>(name, tokenId, product));


        public static RuleBase<T> OneOrMore<T>(this RuleGraph<T> graph, string name, AnnotationProduct product, RuleBase<T> function)
            where T : IComparable<T> =>
            
            graph.TryFindRule(name, out MatchFunctionCount<T>? existingRule)
                ? existingRule!
                : graph.RegisterRule(new MatchFunctionCount<T>(name, function, product, 1, 0));

        public static RuleBase<T> OneOrMore<T>(this RuleGraph<T> graph, RuleBase<T> function)
            where T : IComparable<T> =>
            
            graph.OneOrMore($"{TokenNames.OneOrMore}({function.Name})", AnnotationProduct.None, function);

        public static MatchAnyData<T> Any<T>(this RuleGraph<T> graph)
            where T : IComparable<T> =>
        
            graph.Any($"{AnnotationProduct.None.GetPrefix()}{TokenNames.AnyCharacter}(1,1)", AnnotationProduct.None, 1, 1);
        

        public static MatchAnyData<T> Any<T>(this RuleGraph<T> graph, string name, AnnotationProduct product, int min, int max)
            where T : IComparable<T> =>

            graph.TryFindRule(name, out MatchAnyData<T>? existingRule)
                 ? existingRule!
                 : graph.RegisterRule(new MatchAnyData<T>(name, product, min, max));

        public static MatchNotFunction<T> Not<T>(this RuleGraph<T> graph, RuleBase<T> rule)
            where T : IComparable<T> =>
        
            graph.Not($"{AnnotationProduct.None.GetPrefix()}{TokenNames.Not}({rule.Name})", AnnotationProduct.None, rule);
        

        public static MatchNotFunction<T> Not<T>(this RuleGraph<T> graph, string name, AnnotationProduct product, RuleBase<T> rule)
            where T : IComparable<T> =>

            graph.TryFindRule(name, out MatchNotFunction<T>? existingRule)
                 ? existingRule!
                 : graph.RegisterRule(new MatchNotFunction<T>(name, product, rule));
    }
}
