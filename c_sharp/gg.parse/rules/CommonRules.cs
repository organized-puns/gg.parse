using gg.parse.rulefunctions.datafunctions;
using gg.parse.rulefunctions.rulefunctions;
using System.Linq;

namespace gg.parse.rulefunctions
{
    public static class CommonRules
    {
        public static MatchDataSequence<char> Literal(this RuleGraph<char> table, string literal) =>
            Literal(table, literal.ToCharArray());

        public static MatchDataSequence<T> Literal<T>(this RuleGraph<T> table, T[] sequence) 
            where T : IComparable<T> =>
        
            Literal(table,
                $"{AnnotationProduct.None.GetPrefix()}{CommonTokenNames.Literal}({string.Concat(sequence)})", 
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

            table.Sequence($"{AnnotationProduct.None.GetPrefix()}{CommonTokenNames.DataSequence}({string.Join(", ", data)})", 
                        AnnotationProduct.None, 
                        data);

        public static RuleBase<T> Sequence<T>(this RuleGraph<T> table, string ruleName, AnnotationProduct product, params T[] data)
            where T : IComparable<T> =>

            table.TryFindRule(ruleName, out MatchDataSequence<T>? existingRule)
                ? existingRule!
                : table.RegisterRule(new MatchDataSequence<T>(ruleName, data, product));

        public static MatchFunctionSequence<T> Sequence<T>(this RuleGraph<T> table, params RuleBase<T>[] functions)
            where T : IComparable<T> =>
            
            table.Sequence($"{AnnotationProduct.None.GetPrefix()}{CommonTokenNames.FunctionSequence}({string.Join(",", functions.Select(f => f.Name))})", 
                AnnotationProduct.None, 
                functions);

        public static MatchFunctionSequence<T> Sequence<T>(this RuleGraph<T> table, string ruleName, AnnotationProduct product, params RuleBase<T>[] functions)
            where T : IComparable<T> =>
            
            table.TryFindRule(ruleName, out MatchFunctionSequence<T>? existingRule)
                ? existingRule!
                : table.RegisterRule(new MatchFunctionSequence<T>(ruleName, product, 0, functions));

        public static MatchOneOfFunction<T> OneOf<T>(this RuleGraph<T> table, params RuleBase<T>[] rules)
             where T : IComparable<T> =>

            table.OneOf($"{AnnotationProduct.None.GetPrefix()}{CommonTokenNames.OneOf}({string.Join(",", rules.Select(f => f.Name))})", 
                AnnotationProduct.None, 
                rules);

        public static MatchOneOfFunction<T> OneOf<T>(this RuleGraph<T> table, string name, AnnotationProduct product, params RuleBase<T>[] rules)
             where T : IComparable<T> =>
                table.TryFindRule(name, out MatchOneOfFunction<T>? existingRule)
                     ? existingRule!
                     : table.RegisterRule(new MatchOneOfFunction<T>(name, product, 0, rules));

        public static MatchFunctionCount<T> ZeroOrMore<T>(this RuleGraph<T> table, string name, AnnotationProduct product, RuleBase<T> function)
             where T : IComparable<T> =>
                table.TryFindRule(name, out MatchFunctionCount<T>? existingRule)
                    ? existingRule!
                    : table.RegisterRule(new MatchFunctionCount<T>(name, function, product, 0, 0));

        public static MatchFunctionCount<T> ZeroOrMore<T>(this RuleGraph<T> table, RuleBase<T> function)
             where T : IComparable<T> =>
            table.ZeroOrMore($"{AnnotationProduct.None.GetPrefix()}{CommonTokenNames.ZeroOrMore}({function.Name})", 
                            AnnotationProduct.None, 
                            function);        

            public static MatchFunctionCount<T> ZeroOrOne<T>(this RuleGraph<T> table, RuleBase<T> function)
                where T : IComparable<T> =>
                
                table.ZeroOrOne($"{AnnotationProduct.None.GetPrefix()}{CommonTokenNames.ZeroOrOne}({function.Name})", 
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
            
            graph.OneOrMore($"{CommonTokenNames.OneOrMore}({function.Name})", AnnotationProduct.None, function);

        public static MatchAnyData<T> Any<T>(this RuleGraph<T> graph)
            where T : IComparable<T> =>
        
            graph.Any($"{AnnotationProduct.None.GetPrefix()}{CommonTokenNames.AnyCharacter}(1,1)", AnnotationProduct.None);
        
        public static MatchAnyData<T> Any<T>(this RuleGraph<T> graph, string name, AnnotationProduct product)
            where T : IComparable<T> =>

            graph.TryFindRule(name, out MatchAnyData<T>? existingRule)
                 ? existingRule!
                 : graph.RegisterRule(new MatchAnyData<T>(name, product));

        public static MatchNotFunction<T> Not<T>(this RuleGraph<T> graph, RuleBase<T> rule)
            where T : IComparable<T> =>
        
            graph.Not($"{AnnotationProduct.None.GetPrefix()}{CommonTokenNames.Not}({rule.Name})", AnnotationProduct.None, rule);
        
            
        public static MatchNotFunction<T> Not<T>(this RuleGraph<T> graph, string name, AnnotationProduct product, RuleBase<T> rule)
            where T : IComparable<T> =>

            graph.TryFindRule(name, out MatchNotFunction<T>? existingRule)
                 ? existingRule!
                 : graph.RegisterRule(new MatchNotFunction<T>(name, product, rule));

        public static RuleBase<char> Digit(this RuleGraph<char> graph, string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            // {'0'..'9'}
            var ruleName = name ?? $"{product.GetPrefix()}{CommonTokenNames.Digit}";

            if (graph.TryFindRule(ruleName, out MatchDataRange<char>? existingRule))
            {
                return existingRule!;
            }

            return graph.RegisterRule(new MatchDataRange<char>(ruleName, '0', '9', product));
        }

        public static RuleBase<char> DigitSequence(this RuleGraph<char> graph, string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            // {'0'..'9'}+
            var ruleName = name ?? $"{product.GetPrefix()}{CommonTokenNames.DigitSequence}";

            if (graph.TryFindRule(ruleName, out MatchFunctionCount<char>? existingRule))
            {
                return existingRule!;
            }

            return graph.OneOrMore(ruleName, product, graph.Digit(null, AnnotationProduct.None));
        }

        public static RuleBase<char> IdentifierStartingCharacter(this RuleGraph<char> graph,
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            var underscore = 
                graph.TryFindRule("#Underscore", out MatchSingleData<char> existingRule)
                    ? existingRule!
                    : graph.RegisterRule(new MatchSingleData<char>("#Underscore", '_', AnnotationProduct.None));

            var ruleName = name ?? $"{product.GetPrefix()}FirstIdentifierCharacter";

            return graph.OneOf(
                ruleName, 
                product, 
                graph.LowerCaseLetter(), 
                graph.UpperCaseLetter(), 
                underscore
            );
        }

        public static RuleBase<char> IdentifierCharacter(this RuleGraph<char> graph,
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            var ruleName = name ?? $"{product.GetPrefix()}NextIdentifierCharacter";

            return graph.OneOf(
                ruleName, 
                AnnotationProduct.None,
                graph.IdentifierStartingCharacter(product: AnnotationProduct.None), 
                graph.Digit()
            );
        }

        public static RuleBase<char> Identifier(this RuleGraph<char> graph,
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            var firstCharacter = graph.IdentifierStartingCharacter(product: AnnotationProduct.None);
            var nextCharacter = graph.IdentifierCharacter(product: AnnotationProduct.None);
            var nextCharacterString = graph.ZeroOrMore("#NextIdentifierCharacterString", AnnotationProduct.None, nextCharacter);

            var ruleName = name ?? $"{product.GetPrefix()}{CommonTokenNames.Identifier}";

            return graph.Sequence(ruleName, product, firstCharacter, nextCharacterString);
        }

        public static RuleBase<char> Sign(this RuleGraph<char> graph, string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            // {'+', '-'}
            var ruleName = name ?? $"{product.GetPrefix()}{CommonTokenNames.Sign}";

            return graph.TryFindRule(ruleName, out MatchDataSet<char>? existingRule)
                ? existingRule!
                : graph.RegisterRule(new MatchDataSet<char>(ruleName, product, ['+', '-']));
        }

        public static RuleBase<char> Integer(this RuleGraph<char> graph, string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            // ('+' | '-')?, {'0'..'9'}+
            var ruleName = name ?? $"{product.GetPrefix()}{CommonTokenNames.Integer}";

            return (graph.TryFindRule(ruleName, out MatchFunctionSequence<char>? existingRule)
                ? existingRule!
                : graph.Sequence(ruleName, product,
                    graph.ZeroOrOne(graph.Sign(product: AnnotationProduct.None)),
                    graph.DigitSequence(product: AnnotationProduct.None)));
        }

        public static MatchFunctionSequence<char> Keyword(this RuleGraph<char> graph, string name, AnnotationProduct product, string keyword)
        {
            var ruleName = name ?? $"{product.GetPrefix()}{name}({keyword})";
            return graph.TryFindRule(ruleName, out MatchFunctionSequence<char>? existingRule)
                ? existingRule!
                : graph.RegisterRule(
                    new MatchFunctionSequence<char>(
                        ruleName, 
                        product, 
                        precedence: 0, 
                        graph.Literal(keyword), 
                        graph.Not(graph.IdentifierCharacter(product: AnnotationProduct.None))
                    )
                );
        }

        public static RuleBase<char> Float(this RuleGraph<char> graph,
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            // sign?, digitSequence, '.', digitSequence, (('e' | 'E'), sign?, digitSequence)?
            var digitSequence = graph.DigitSequence(product: AnnotationProduct.None);
            var sign = graph.ZeroOrOne(graph.Sign(product: AnnotationProduct.None));
            var exponentPart = graph.Sequence(graph.InSet(['e', 'E']), sign, digitSequence);

            return graph.Sequence(name ?? CommonTokenNames.Float, product,
                sign,
                digitSequence,
                graph.Literal("."),
                digitSequence,
                graph.ZeroOrOne(exponentPart)
            );
        }

        public static RuleBase<char> Boolean(this RuleGraph<char> graph,
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            // 'true' | 'false'
            var ruleName = name ?? $"{product.GetPrefix()}{CommonTokenNames.Boolean}";

            return graph.TryFindRule(ruleName, out MatchOneOfFunction<char>? existingRule)
                ? existingRule!
                : graph.OneOf(ruleName, product,
                                graph.Literal("true"),
                                graph.Literal("false"));
        }

        public static RuleBase<char> String(this RuleGraph<char> graph,
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation, char delimiter = '"')
        {
            // '"', ('\\"' or (not '"', any) )*, '"'
            // '"', ('\\"' | (!'"', _) )*, '"'
            var delimiterRule = graph.InSet($"{CommonTokenNames.NoProductPrefix}StringDelimiter({delimiter})", AnnotationProduct.None, delimiter);
            var escapedQuote = graph.Sequence($"{CommonTokenNames.NoProductPrefix}Escaped({delimiter})", AnnotationProduct.None, '\\', delimiter);
            
            var notQuoteThenAny = graph.Sequence($"{CommonTokenNames.NoProductPrefix}IsStringCharacter({delimiter})", 
                                                    AnnotationProduct.None, 
                                                    graph.Not(delimiterRule), 
                                                    graph.Any());
            
            var stringCharacters = graph.ZeroOrMore($"{CommonTokenNames.NoProductPrefix}StringCharacter({delimiter})", 
                                                    AnnotationProduct.None, 
                                                    graph.OneOf(escapedQuote, notQuoteThenAny));

            var ruleName = name ?? $"{product.GetPrefix()}{CommonTokenNames.String}({delimiter})";

            return graph.Sequence(ruleName, product, delimiterRule, stringCharacters, delimiterRule);
        }

        public static MatchDataRange<char> LowerCaseLetter(this RuleGraph<char> graph, 
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            var ruleName = name ?? $"{product.GetPrefix()}{CommonTokenNames.LowerCaseLetter}";
            return graph.TryFindRule(ruleName, out MatchDataRange<char>? existingRule)
                     ? existingRule!
                     : graph.RegisterRule(new MatchDataRange<char>(ruleName, 'a', 'z', product));
        }

        public static MatchDataRange<char> UpperCaseLetter(this RuleGraph<char> graph, 
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            var ruleName = name ?? $"{product.GetPrefix()}{CommonTokenNames.UpperCaseLetter}";
            return graph.TryFindRule(ruleName, out MatchDataRange<char>? existingRule)
                     ? existingRule!
                     : graph.RegisterRule(new MatchDataRange<char>(ruleName, 'A', 'Z', product));
        }

        public static RuleBase<char> EndOfLine(this RuleGraph<char> graph, 
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            var ruleName = name ?? $"{CommonTokenNames.EndOfLine}";

            return graph.OneOf(ruleName, product,
                    graph.Literal("CRLF", AnnotationProduct.None, "\r\n".ToCharArray()),
                    graph.Literal("LF", AnnotationProduct.None, "\n".ToCharArray()));
        }

        public static RuleBase<char> SingleLineComment(this RuleGraph<char> graph,
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation, string startComment = "//")
        {
            var ruleName = name ?? $"{CommonTokenNames.SingleLineComment}";
            var commentCharacter = graph.Sequence(graph.Not(graph.EndOfLine()), graph.Any());

            return graph.Sequence(ruleName, product, graph.Literal(startComment), graph.ZeroOrMore(commentCharacter));
        }

        public static RuleBase<char> MultiLineComment(this RuleGraph<char> graph,
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation, string startComment = "/*", string endComment = "*/")
        {
            var ruleName = name ?? $"{CommonTokenNames.MultiLineComment}";
            var commentCharacter = graph.Sequence(graph.Not(graph.Literal(endComment)), graph.Any());

            return graph.Sequence(ruleName, product, graph.Literal(startComment),
                graph.ZeroOrMore(commentCharacter), graph.Literal(endComment));
        }

        public static RuleBase<char> Whitespace(this RuleGraph<char> graph)
            => graph.Whitespace($"{AnnotationProduct.None.GetPrefix()}{CommonTokenNames.Whitespace}", AnnotationProduct.None);

        public static RuleBase<char> Whitespace(this RuleGraph<char> graph, string name, AnnotationProduct product) =>
            // {' ', '\r', '\n', '\t' }
            graph.TryFindRule(name, out MatchDataSet<char>? existingRule)
                     ? existingRule!
                     : graph.RegisterRule(new MatchDataSet<char>(name, product, [' ', '\r', '\n', '\t']));

        public static LogRule<T> LogError<T>(this RuleGraph<T> graph,
            string name, AnnotationProduct product, string description, RuleBase<T>? condition = null)
            where T : IComparable<T> =>
            graph.TryFindRule(name, out LogRule<T>? existingRule)
                     ? existingRule!
                     : graph.RegisterRule(new LogRule<T>(name, product, description, condition, LogLevel.Error));

        public static SkipRule<T> Skip<T>(this RuleGraph<T> graph, RuleBase<T> stopCondition, bool failOnEoF = true)
            where T : IComparable<T>
        {
            return graph.Skip(
                $"{AnnotationProduct.None.GetPrefix()}{CommonTokenNames.Skip}", 
                AnnotationProduct.None, 
                stopCondition, 
                failOnEoF
            );
        }

        public static SkipRule<T> Skip<T>(this RuleGraph<T> graph,
            string name, AnnotationProduct product, RuleBase<T> stopCondition, bool failOnEoF = true)
            where T : IComparable<T> =>
            graph.TryFindRule(name, out SkipRule<T>? existingRule)
                     ? existingRule!
                     : graph.RegisterRule(new SkipRule<T>(name, product, stopCondition, failOnEoF));


        public static RuleBase<char> InSet(this RuleGraph<char> graph, params char[] set)
        {
            AnnotationProduct product = AnnotationProduct.None;
            var ruleName = $"{product.GetPrefix()}{CommonTokenNames.Set}({string.Join(", ", set)})";
            return graph.InSet(ruleName, product, set);
        }

        public static RuleBase<char> InSet(this RuleGraph<char> graph, string ruleName, AnnotationProduct product, params char[] set) =>

            graph.TryFindRule(ruleName, out MatchDataSet<char>? existingRule)
                ? existingRule!
                : graph.RegisterRule(new MatchDataSet<char>(ruleName, product, set));

        public static TryMatchFunction<T> TryMatch<T>(
            this RuleGraph<T> graph, 
            string ruleName, 
            AnnotationProduct product, 
            RuleBase<T> function
        ) where T : IComparable<T> =>

            graph.TryFindRule(ruleName, out TryMatchFunction<T>? existingRule)
                ? existingRule!
                : graph.RegisterRule(new TryMatchFunction<T>(ruleName, product, function));


        public static TryMatchFunction<T> TryMatch<T>(this RuleGraph<T> graph, RuleBase<T> function) where T : IComparable<T> =>

            TryMatch(
                graph,
                $"{AnnotationProduct.None.GetPrefix()}{CommonTokenNames.TryMatchOperator}({string.Join(", ", function)})",
                AnnotationProduct.None,
                function
            );            
    }
}

