using gg.parse.rules;
using System.Linq;

namespace gg.parse.script.common
{
    public class CommonTokenizer : CommonGraphWrapper<char>
    {
        public RuleBase<char> Boolean() =>
            Boolean(null);

        public RuleBase<char> Boolean(string? name) =>
                FindOrRegister(name, CommonTokenNames.Boolean,
                        (ruleName, product) =>
                            RegisterRule(
                                new MatchOneOf<char>(
                                    ruleName,
                                    product,
                                    0,
                                    Literal("true"),
                                    Literal("false")
                                )
                            )
                        );

        public MatchDataRange<char> Digit(string? name = null) =>
            FindOrRegister(name, CommonTokenNames.Digit,
                        (ruleName, product) => RegisterRule(new MatchDataRange<char>(ruleName, '0', '9', product)));

        public MatchCount<char> DigitSequence(string? name = null) =>
            FindOrRegister(name, 
                CommonTokenNames.DigitSequence,
                (ruleName, product) => 
                    RegisterRule(new MatchCount<char>(ruleName, Digit(), product, min: 1, max: 0)));

        public MatchRuleSequence<char> Float() =>
            Float(null);

        public MatchRuleSequence<char> Float(string? name) =>
            FindOrRegister(name,
                CommonTokenNames.Float,
                (ruleName, product) =>
                    RegisterRule(new MatchRuleSequence<char>(
                        ruleName,
                        product,
                        precedence: 0,
                        ZeroOrOne(Sign()),
                        DigitSequence(),
                        MatchSingle('.'),
                        DigitSequence(),
                        ZeroOrOne(
                            Sequence(InSet(['e', 'E']), ZeroOrOne(Sign()), DigitSequence())
                        )
                    )
                )
            );

        public MatchRuleSequence<char> Identifier(string? name = null) =>
            FindOrRegister(name, 
                CommonTokenNames.Identifier,
                (ruleName, product) => RegisterRule(
                    new MatchRuleSequence<char>(
                        ruleName,
                        product,
                        0,
                        IdentifierStartingCharacter(), 
                        ZeroOrMore(IdentifierCharacter())
                    )
                )
            );

        public MatchOneOf<char> IdentifierCharacter(string? name = null) =>
            FindOrRegister(name, CommonTokenNames.IdentifierCharacter,
                        (ruleName, product) => RegisterRule(
                            new MatchOneOf<char>(ruleName, product, 0, IdentifierStartingCharacter(), Digit())));

        public MatchOneOf<char> IdentifierStartingCharacter(string? name = null) =>
            FindOrRegister(name, CommonTokenNames.IdentifierStartingCharacter,
                        (ruleName, product) => RegisterRule(
                            new MatchOneOf<char>(ruleName, product, 0, LowerCaseLetter(), UpperCaseLetter(), UnderscoreCharacter())));

        public MatchRuleSequence<char> Integer(string? name = null) =>
            FindOrRegister(
                name,
                CommonTokenNames.Integer,
                (ruleName, product) => RegisterRule(
                    new MatchRuleSequence<char>(
                        ruleName,
                        product,
                        0,
                        ZeroOrOne(Sign()),
                        DigitSequence()
                    )
                )
            );

        public MatchRuleSequence<char> Keyword(string keyword) =>
            Keyword(null, keyword);

        public MatchRuleSequence<char> Keyword(string? name, string keyword) =>
            FindOrRegister(name, $"{CommonTokenNames.Keyword}({keyword})",
                        (ruleName, product) => 
                            RegisterRule(
                                new MatchRuleSequence<char>(
                                    ruleName,
                                    product,
                                    precedence: 0,
                                    Literal(keyword.ToCharArray()),
                                    Not(IdentifierCharacter())
                                )
                            )
            );

        public MatchDataSequence<char> Literal(string sequence) =>
            Literal(null, sequence.ToCharArray());

        public MatchDataSequence<char> Literal(string name, string sequence) =>
            Literal(name, sequence.ToCharArray());

        public MatchDataRange<char> LowerCaseLetter(string? name = null) =>
            FindOrRegister(name, CommonTokenNames.LowerCaseLetter, 
                        (ruleName, product) => RegisterRule(new MatchDataRange<char>(ruleName, 'a', 'z', product)));

        public MatchRuleSequence<char> MatchString(string? name = null, char delimiter = '"') =>
            FindOrRegister(name, CommonTokenNames.String,
                        (ruleName, product) =>
                            RegisterRule(new MatchRuleSequence<char>(
                                ruleName,
                                product,
                                precedence: 0,
                                // '"', ('\\"' or (not '"', any) )*, '"'
                                // '"', ('\\"' | (!'"', _) )*, '"'
                                MatchSingle(delimiter),
                                ZeroOrMore(
                                    OneOf(
                                        // escaped character
                                        Sequence(MatchSingle('\\'), Any()),
                                        // escaped escape
                                        //Literal(['\\', '\\']),
                                        // string character that is NOT a delimiter
                                        Sequence(Not(MatchSingle(delimiter)), Any())
                                    )
                                ),
                                MatchSingle(delimiter))
                            )
                        );

        public RuleBase<char> MultiLineComment(string? name = null, string startComment = "/*", string endComment = "*/") =>
            FindOrRegister(name, CommonTokenNames.MultiLineComment,
                        (ruleName, product) =>
                            RegisterRule(
                                new MatchRuleSequence<char>(
                                    ruleName,
                                    product,
                                    0,
                                    Literal(startComment),
                                    ZeroOrMore(Sequence(Not(Literal(endComment)), Any())),
                                    Literal(endComment)
                                )
                            )
                        );

        public MatchDataSet<char> Sign(string? name = null) =>
            FindOrRegister(name, CommonTokenNames.Sign,
                        (ruleName, product) => RegisterRule(new MatchDataSet<char>(ruleName, product, ['+', '-'])));

        public MatchRuleSequence<char> SingleLineComment(string? name = null, string startComment = "//") =>
        FindOrRegister(name, CommonTokenNames.SingleLineComment,
                (ruleName, product) =>
                    RegisterRule(
                        new MatchRuleSequence<char>(
                            ruleName,
                            product,
                            0,
                            Literal(startComment),
                            ZeroOrMore(Sequence(Not(MatchSingle('\n')), Any()))
                        )
                    )
                );        

        public MatchSingleData<char> UnderscoreCharacter(string? name = null) =>
            FindOrRegister(name, CommonTokenNames.Underscore,
                        (ruleName, product) => RegisterRule(new MatchSingleData<char>(ruleName, '_', product)));

        public MatchDataRange<char> UpperCaseLetter(string? name = null) =>
            FindOrRegister(name, CommonTokenNames.UpperCaseLetter,
                        (ruleName, product) => RegisterRule(new MatchDataRange<char>(ruleName, 'A', 'Z', product)));

        public MatchDataSet<char> Whitespace(string? name = null) =>
            FindOrRegister(name, CommonTokenNames.Whitespace,
                        (ruleName, product) =>
                            RegisterRule(new MatchDataSet<char>(ruleName, product, [' ', '\r', '\n', '\t'])));
    }
}
