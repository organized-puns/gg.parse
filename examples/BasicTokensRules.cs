using gg.parse.rules;

namespace gg.parse.examples
{
    public static class BasicTokensRules
    {
        public static IRule CreateDigitRule(string name = "Digit")
        {
            return new CharacterRangeRule(name, '0', '9');
        }

        public static IRule CreateSignRule(string name = "Sign")
        {
            return new OneOfRule(name, new LiteralRule("+"), new LiteralRule("-"));
        }

        public static IRule CreateZeroOrOne(IRule rule, string name = "ZeroOrOne")
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule), "Rule cannot be null");
            }

            return new CountRule(name + $"({rule.Name})", rule, 0, 1);
        }

        public static IRule CreateZeroOrMore(IRule rule, string name = "ZeroOrMore")
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule), "Rule cannot be null");
            }

            return new CountRule(name + $"({rule.Name})", rule, 0, 0);
        }


        public static IRule CreateDigitStringRule(string name = "DigitString", IRule? digitRule = null)
        {
            return new CountRule(name, digitRule ?? CreateDigitRule(), 1, 0);
        }

        public static IRule CreateIntegerRule(string name = "Integer", IRule? digitStringRule = null, IRule? signRule = null)
        {
            var digitString = digitStringRule  ?? CreateDigitStringRule();
            var optionalSign = CreateZeroOrOne(signRule ?? CreateSignRule(), "OptionalSign");
            return new SequenceRule(name, optionalSign, digitString);
        }

        /// <summary>
        /// Creates a simple float rule that matches an optional sign followed by a digit string and
        /// an optional decimal point followed by another digit string.
        /// Does not handle scientific notation or other complex float formats.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="digitStringRule"></param>
        /// <param name="signRule"></param>
        /// <returns></returns>
        public static IRule CreateSimpleFloatRule(string name = "SimpleFloat", IRule? digitStringRule = null, IRule? signRule = null)
        {
            var digitString = digitStringRule ?? CreateDigitStringRule();
            var optionalSign = CreateZeroOrOne(signRule ?? CreateSignRule(), "OptionalSign");
            var decimalRule = new SequenceRule("DecimalPoint", new LiteralRule("."), digitString);
            return new SequenceRule(name, optionalSign, digitString, decimalRule);
        }

        public static IRule CreateStringRule(string name = "String")
        {
            var escapedQuote = new LiteralRule("\\\"");
            var notQuote = new NotRule(new LiteralRule("\""));
            var notQuoteElseAny = new SequenceRule("NotQuote", notQuote, new AnyRule());

            var stringCharacter = new OneOfRule("StringCharacter", escapedQuote, notQuoteElseAny);
            var stringContent = CreateZeroOrMore(name: "StringContent", rule: stringCharacter);

            return new SequenceRule(name, new LiteralRule("\""), stringContent, new LiteralRule("\""));
        }

        public static IRule CreateWhitespaceRule(string name = "Whitespace")
        {
            return new CharacterSetRule(name: name, characters: " \t\n\r");
        }
    }
}
