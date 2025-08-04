namespace gg.parse.rulefunctions
{
    public class BasicTokensTable : RuleTable<char>
    {
        public RuleBase<char> Digit(string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            // {'0'..'9'}
            var ruleName = name ?? $"{product.GetPrefix()}{TokenNames.Digit}";

            if (TryFindRule(ruleName, out MatchDataRange<char>? existingRule))
            {
                return existingRule!;
            }

            return RegisterRule(
                new MatchDataRange<char>(ruleName, '0', '9', product));
        }

        public RuleBase<char> DigitSequence(string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            // {'0'..'9'}+
            var ruleName = name ?? $"{product.GetPrefix()}{TokenNames.DigitSequence}";

            if (TryFindRule(ruleName, out MatchFunctionCount<char>? existingRule))
            {
                return existingRule!;
            }

            var digitRule = Digit(null, AnnotationProduct.None);

            return OneOrMore(digitRule, ruleName, product);
        }

        public RuleBase<char> Sign(string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            // {'+', '-'}
            var ruleName = name ?? $"{product.GetPrefix()}{TokenNames.Sign}";

            return TryFindRule(ruleName, out MatchDataSet<char>? existingRule)
                ? existingRule!
                : RegisterRule(new MatchDataSet<char>(ruleName, product, ['+', '-']));
        }

        public RuleBase<char> Integer(string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            // ('+' | '-')?, {'0'..'9'}+
            var ruleName = name ?? $"{product.GetPrefix()}{TokenNames.Integer}";

            return (TryFindRule(ruleName, out MatchFunctionSequence<char>? existingRule)
                ? existingRule!
                : Sequence(ruleName, product,
                    ZeroOrOne(Sign(product: AnnotationProduct.None)),
                    DigitSequence(product: AnnotationProduct.None)));            
        }

        public RuleBase<char> Literal(string literal, string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            var ruleName = name ?? $"{product.GetPrefix()}{TokenNames.Literal}({literal})";
            return TryFindRule(ruleName, out MatchDataSequence<char>? existingRule)
                ? existingRule!
                : RegisterRule(new MatchDataSequence<char>(ruleName, literal.ToCharArray(), product));
        }

        public RuleBase<char> Float(
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            // sign?, digitSequence, '.', digitSequence, (('e' | 'E'), sign?, digitSequence)?
            var digitSequence = DigitSequence(product: AnnotationProduct.None);
            var sign = ZeroOrOne(Sign(product: AnnotationProduct.None));
            var exponentPart = Sequence(InSet(['e', 'E']), sign, digitSequence);

            return Sequence(name ?? TokenNames.Float, product,
                sign,
                digitSequence,
                Literal(".", product: AnnotationProduct.None),
                digitSequence,
                ZeroOrOne(exponentPart)
            );
        }

        public RuleBase<char> Boolean(
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            // 'true' | 'false'
            var ruleName = name ?? $"{product.GetPrefix()}{TokenNames.Boolean}";

            return TryFindRule(ruleName, out MatchOneOfFunction<char>? existingRule)
                ? existingRule!
                : OneOf(ruleName, product,
                                Literal("true", product: AnnotationProduct.None),
                                Literal("false", product: AnnotationProduct.None));
        }

        public RuleBase<char> String(
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation, char delimiter = '"')
        {
            // '"', ('\\"' or (not '"', any) )*, '"'
            // '"', ('\\"' | (!'"', _) )*, '"'
            var delimiterRule = InSet($"{TokenNames.NoProductPrefix}StringDelimiter({delimiter})", AnnotationProduct.None, delimiter);
            var escapedQuote = Sequence($"{TokenNames.NoProductPrefix}Escaped({delimiter})", AnnotationProduct.None, '\\', delimiter);
            var notQuoteThenAny = Sequence($"{TokenNames.NoProductPrefix}IsStringCharacter({delimiter})", AnnotationProduct.None, Not(delimiterRule), Any());
            var stringCharacters = ZeroOrMore($"{TokenNames.NoProductPrefix}StringCharacter({delimiter})", AnnotationProduct.None, OneOf(escapedQuote, notQuoteThenAny));

            var ruleName = name ?? $"{product.GetPrefix()}{TokenNames.String}({delimiter})";

            return Sequence(ruleName, product, delimiterRule, stringCharacters, delimiterRule);
        }

        public MatchDataRange<char> LowerCaseLetter(string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            var ruleName = name ?? $"{product.GetPrefix()}{TokenNames.LowerCaseLetter}";
            return TryFindRule(ruleName, out MatchDataRange<char>? existingRule)
                     ? existingRule!
                     : RegisterRule(new MatchDataRange<char>(ruleName, 'a', 'z', product));
        }

        public MatchDataRange<char> UpperCaseLetter(string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            var ruleName = name ?? $"{product.GetPrefix()}{TokenNames.UpperCaseLetter}";
            return TryFindRule(ruleName, out MatchDataRange<char>? existingRule)
                     ? existingRule!
                     : RegisterRule(new MatchDataRange<char>(ruleName, 'A', 'Z', product));
        }

        public RuleBase<char> Identifier(
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            var underscore = TryFindRule("#Underscore", out MatchSingleData<char> existingRule)
                        ? existingRule!
                        : RegisterRule(new MatchSingleData<char>("#Underscore", '_', AnnotationProduct.None));
            var firstCharacter = OneOf("#FirstIdentifierCharacter", AnnotationProduct.None, LowerCaseLetter(), UpperCaseLetter(), underscore);
            var nextCharacter = OneOf("#NextIdentifierCharacter", AnnotationProduct.None, firstCharacter, Digit());
            var nextCharacterString = ZeroOrMore("#NextIdentifierCharacterString", AnnotationProduct.None, nextCharacter);

            var ruleName = name ?? $"{product.GetPrefix()}{TokenNames.Identifier}";

            return Sequence(ruleName, product, firstCharacter, nextCharacterString);
        }

        public RuleBase<char> EndOfLine(string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            var ruleName = name ?? $"{TokenNames.EndOfLine}";

            return OneOf(ruleName, product,
                    Literal("\r\n", "CRLF", AnnotationProduct.None),
                    Literal("\n", "LF", AnnotationProduct.None));
        }

        public RuleBase<char> SingleLineComment(
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation, string startComment = "//")
        {
            var ruleName = name ?? $"{TokenNames.SingleLineComment}";
            var commentCharacter = Sequence(Not(EndOfLine()), Any());

            return Sequence(ruleName, product, Literal(startComment), ZeroOrMore(commentCharacter));
        }

        public RuleBase<char> MultiLineComment(
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation, string startComment = "/*", string endComment = "*/")
        {
            var ruleName = name ?? $"{TokenNames.MultiLineComment}";
            var commentCharacter = Sequence(Not(Literal(endComment)), Any());

            return Sequence(ruleName, product, Literal(startComment), ZeroOrMore(commentCharacter), Literal(endComment));
        }


        public RuleBase<char> Whitespace()
            => Whitespace($"{AnnotationProduct.None.GetPrefix()}{TokenNames.Whitespace}", AnnotationProduct.None);

        public RuleBase<char> Whitespace(string name, AnnotationProduct product) =>
            // {' ', '\r', '\n', '\t' }
            TryFindRule(name, out MatchDataSet<char>? existingRule)
                     ? existingRule!
                     : RegisterRule(new MatchDataSet<char>(name, product, [' ', '\r', '\n', '\t']));
         

        public MarkError<char> Error(string name, AnnotationProduct product, string description, RuleBase<char>? testFunction, int maxLength) =>
            TryFindRule(name, out MarkError<char>? existingRule)
                     ? existingRule!
                     : RegisterRule(new MarkError<char>(name, product, description, testFunction, maxLength));
        

        public RuleBase<char> Any()
        {
            var product = AnnotationProduct.None;
            var ruleName = $"{product.GetPrefix()}{TokenNames.AnyCharacter}(1,1)";
            return Any(ruleName, product, 1, 1);
        }         

        public RuleBase<char> Any(string name, AnnotationProduct product, int min, int max) =>
            TryFindRule(name, out MatchAnyData<char>? existingRule)
                 ? existingRule!
                 : RegisterRule(new MatchAnyData<char>(name, product, min, max));

        public RuleBase<char> Not(RuleBase<char> rule)
        {
            var product = AnnotationProduct.None;
            var ruleName = $"{product.GetPrefix()}{TokenNames.Not}({rule.Name})";
            return Not(ruleName, product, rule);
        }
         

        public RuleBase<char> Not(string name, AnnotationProduct product, RuleBase<char> rule) =>
            TryFindRule(name, out MatchNotFunction<char>? existingRule)
                 ? existingRule!
                 : RegisterRule(new MatchNotFunction<char>(name, product, rule));



        public RuleBase<char> OneOrMore(RuleBase<char> function, string? name = null, AnnotationProduct action = AnnotationProduct.Annotation)
        {
            var ruleName = name ?? $"{action.GetPrefix()}{TokenNames.OneOrMore}({function.Name})";

            if (TryFindRule(ruleName, out MatchFunctionCount<char>? existingRule))
            {
                return existingRule!;
            }

            return RegisterRule(
                new MatchFunctionCount<char>(ruleName, function, action, 1, 0));
        }


        public RuleBase<char> InSet(params char[] set)
        {
            AnnotationProduct product = AnnotationProduct.None;
            var ruleName = $"{product.GetPrefix()}{TokenNames.Set}({string.Join(", ", set)})";
            return InSet(ruleName, product, set);
        }

        public RuleBase<char> InSet(string ruleName, AnnotationProduct product, params char[] set) =>
       
            TryFindRule(ruleName, out MatchDataSet<char>? existingRule)
                ? existingRule!
                : new MatchDataSet<char>(ruleName, product, set);
        
    }
}
