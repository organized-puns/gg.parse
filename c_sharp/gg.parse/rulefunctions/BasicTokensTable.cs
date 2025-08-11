
using static gg.parse.rulefunctions.CommonRuleTableRules;

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

            return OneOrMore(ruleName, product, digitRule); 
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
                : this.Sequence(ruleName, product,
                    ZeroOrOne(Sign(product: AnnotationProduct.None)),
                    DigitSequence(product: AnnotationProduct.None)));            
        }

        public MatchFunctionSequence<char> Keyword(string name, AnnotationProduct product, string keyword )
        {
            var ruleName = name ?? $"{product.GetPrefix()}{name}({keyword})";
            return TryFindRule(ruleName, out MatchFunctionSequence<char>? existingRule)
                ? existingRule!
                : RegisterRule(new MatchFunctionSequence<char>(ruleName, product,
                                    CommonRuleTableRules.Literal(this, keyword), Whitespace()));
        }


        public RuleBase<char> Float(
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            // sign?, digitSequence, '.', digitSequence, (('e' | 'E'), sign?, digitSequence)?
            var digitSequence = DigitSequence(product: AnnotationProduct.None);
            var sign = ZeroOrOne(Sign(product: AnnotationProduct.None));
            var exponentPart = this.Sequence(InSet(['e', 'E']), sign, digitSequence);

            return this.Sequence(name ?? TokenNames.Float, product,
                sign,
                digitSequence,
                CommonRuleTableRules.Literal(this, "."),
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
                                CommonRuleTableRules.Literal(this, "true"),
                                CommonRuleTableRules.Literal(this, "false"));
        }

        public RuleBase<char> String(
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation, char delimiter = '"')
        {
            // '"', ('\\"' or (not '"', any) )*, '"'
            // '"', ('\\"' | (!'"', _) )*, '"'
            var delimiterRule = InSet($"{TokenNames.NoProductPrefix}StringDelimiter({delimiter})", AnnotationProduct.None, delimiter);
            var escapedQuote = this.Sequence($"{TokenNames.NoProductPrefix}Escaped({delimiter})", AnnotationProduct.None, '\\', delimiter);
            var notQuoteThenAny = this.Sequence($"{TokenNames.NoProductPrefix}IsStringCharacter({delimiter})", AnnotationProduct.None, Not(delimiterRule), Any());
            var stringCharacters = ZeroOrMore($"{TokenNames.NoProductPrefix}StringCharacter({delimiter})", AnnotationProduct.None, OneOf(escapedQuote, notQuoteThenAny));

            var ruleName = name ?? $"{product.GetPrefix()}{TokenNames.String}({delimiter})";

            return this.Sequence(ruleName, product, delimiterRule, stringCharacters, delimiterRule);
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

            return this.Sequence(ruleName, product, firstCharacter, nextCharacterString);
        }

        public RuleBase<char> EndOfLine(string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            var ruleName = name ?? $"{TokenNames.EndOfLine}";

            return OneOf(ruleName, product,
                    CommonRuleTableRules.Literal(this, "CRLF", AnnotationProduct.None, "\r\n".ToCharArray()),
                    CommonRuleTableRules.Literal(this, "LF", AnnotationProduct.None, "\n".ToCharArray()));
        }

        public RuleBase<char> SingleLineComment(
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation, string startComment = "//")
        {
            var ruleName = name ?? $"{TokenNames.SingleLineComment}";
            var commentCharacter = this.Sequence(Not(EndOfLine()), Any());

            return this.Sequence(ruleName, product, CommonRuleTableRules.Literal(this, startComment), ZeroOrMore(commentCharacter));
        }

        public RuleBase<char> MultiLineComment(
            string? name = null, AnnotationProduct product = AnnotationProduct.Annotation, string startComment = "/*", string endComment = "*/")
        {
            var ruleName = name ?? $"{TokenNames.MultiLineComment}";
            var commentCharacter =  this.Sequence(Not(CommonRuleTableRules.Literal(this, endComment)), Any());

            return this.Sequence(ruleName, product, CommonRuleTableRules.Literal(this, startComment), 
                ZeroOrMore(commentCharacter), CommonRuleTableRules.Literal(this, endComment));
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
        

        public RuleBase<char> InSet(params char[] set)
        {
            AnnotationProduct product = AnnotationProduct.None;
            var ruleName = $"{product.GetPrefix()}{TokenNames.Set}({string.Join(", ", set)})";
            return InSet(ruleName, product, set);
        }

        public RuleBase<char> InSet(string ruleName, AnnotationProduct product, params char[] set) =>
       
            TryFindRule(ruleName, out MatchDataSet<char>? existingRule)
                ? existingRule!
                : RegisterRule(new MatchDataSet<char>(ruleName, product, set));
     
    }
}
