
using gg.parse.basefunctions;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                    ZeroOrOne(Sign(product: AnnotationProduct.None), product: AnnotationProduct.None),
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
            var sign = ZeroOrOne(Sign(product: AnnotationProduct.None), product: AnnotationProduct.None);
            var exponentPart = Sequence(InSet(['e', 'E']), sign, digitSequence);

            return Sequence(name ?? TokenNames.Float, product,
                sign,
                digitSequence,
                Literal(".", product: AnnotationProduct.None),
                digitSequence,
                ZeroOrOne(exponentPart, product: AnnotationProduct.None)
            );
        }


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

        public RuleBase<char> ZeroOrOne(RuleBase<char> function, string? name = null, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            var ruleName = name ?? $"{product.GetPrefix()}{TokenNames.ZeroOrOne}({function.Name})";

            if (TryFindRule(ruleName, out MatchFunctionCount<char>? existingRule))
            {
                return existingRule!;
            }

            return RegisterRule(
                new MatchFunctionCount<char>(ruleName, function, product, 0, 1));
        }

        public RuleBase<char> Sequence(params RuleBase<char>[] functions)
        {
            var product = AnnotationProduct.None;
            var ruleName = $"{product.GetPrefix()}{TokenNames.FunctionSequence}({string.Join(",", functions.Select(f => f.Name))})";

            return Sequence(ruleName, product, functions);
        }

        public RuleBase<char> Sequence(string ruleName, AnnotationProduct product, params RuleBase<char>[] functions) =>
        
            TryFindRule(ruleName, out MatchFunctionSequence<char>? existingRule)
                ? existingRule!
                : RegisterRule(new MatchFunctionSequence<char>(ruleName, product, functions));

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
