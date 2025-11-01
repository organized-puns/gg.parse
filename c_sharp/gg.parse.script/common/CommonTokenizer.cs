// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.rules;

namespace gg.parse.script.common
{
    public class CommonTokenizer : CommonGraphWrapper<char>
    {
        public RuleBase<char> Boolean() =>
            Boolean(null);

        public RuleBase<char> Boolean(string? name) =>
                FindOrRegister(name, CommonTokenNames.Boolean,
                        (ruleName, pruning) =>
                            RegisterRule(
                                new MatchOneOf<char>(
                                    ruleName,
                                    pruning,
                                    0,
                                    Literal("true"),
                                    Literal("false")
                                )
                            )
                        );

        public MatchDataRange<char> Digit(string? name = null) =>
            FindOrRegister(name, CommonTokenNames.Digit,
                        (ruleName, pruning) => RegisterRule(new MatchDataRange<char>(ruleName, '0', '9', pruning)));

        public MatchCount<char> DigitSequence(string? name = null) =>
            FindOrRegister(name, 
                CommonTokenNames.DigitSequence,
                (ruleName, pruning) => 
                    RegisterRule(new MatchCount<char>(ruleName, pruning, 0, Digit(), min: 1, max: 0)));

        public MatchRuleSequence<char> Float() =>
            Float(null);

        public MatchRuleSequence<char> Float(string? name)
        {
            var optionalSign = ZeroOrOne(Sign());
            var exponentSymbol = InSet(['e', 'E']);
            var exponent = Sequence(exponentSymbol, optionalSign, DigitSequence());
            var digits = DigitSequence();

            return FindOrRegister(name,
                CommonTokenNames.Float,
                (ruleName, pruning) =>
                    RegisterRule(new MatchRuleSequence<char>(
                        ruleName,
                        pruning,
                        precedence: 0,
                        Sequence(
                            optionalSign,
                            digits,
                            OneOf(
                                // captures cases with a floating point like 1.2 or -3.4e5
                                Sequence(
                                    MatchSingle('.'),
                                    digits,
                                    OneOf(exponent, Not(exponentSymbol))
                                ),
                                // captures cases without a floating point eg 3e-5 or -53E+5
                                exponent
                            )
                        )
                    )
                )
            );
        }

        public MatchRuleSequence<char> Identifier(string? name = null) =>
            FindOrRegister(name, 
                CommonTokenNames.Identifier,
                (ruleName, pruning) => RegisterRule(
                    new MatchRuleSequence<char>(
                        ruleName,
                        pruning,
                        0,
                        IdentifierStartingCharacter(), 
                        ZeroOrMore(IdentifierCharacter())
                    )
                )
            );

        public MatchOneOf<char> IdentifierCharacter(string? name = null) =>
            FindOrRegister(name, CommonTokenNames.IdentifierCharacter,
                        (ruleName, pruning) => RegisterRule(
                            new MatchOneOf<char>(ruleName, pruning, 0, IdentifierStartingCharacter(), Digit())));

        public MatchOneOf<char> IdentifierStartingCharacter(string? name = null) =>
            FindOrRegister(name, CommonTokenNames.IdentifierStartingCharacter,
                        (ruleName, pruning) => RegisterRule(
                            new MatchOneOf<char>(ruleName, pruning, 0, LowerCaseLetter(), UpperCaseLetter(), UnderscoreCharacter())));

        public MatchRuleSequence<char> Integer(string? name = null) =>
            FindOrRegister(
                name,
                CommonTokenNames.Integer,
                (ruleName, pruning) => RegisterRule(
                    new MatchRuleSequence<char>(
                        ruleName,
                        pruning,
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
                        (ruleName, pruning) => 
                            RegisterRule(
                                new MatchRuleSequence<char>(
                                    ruleName,
                                    pruning,
                                    precedence: 0,
                                    Literal(keyword.ToCharArray()),
                                    Not("-a not_identifier_char", IdentifierCharacter())
                                )
                            )
            );

        public MatchDataSequence<char> Literal(string sequence) =>
            Literal(null, sequence.ToCharArray());

        public MatchDataSequence<char> Literal(string name, string sequence) =>
            Literal(name, sequence.ToCharArray());

        public MatchDataRange<char> LowerCaseLetter(string? name = null) =>
            FindOrRegister(name, CommonTokenNames.LowerCaseLetter, 
                        (ruleName, pruning) => RegisterRule(new MatchDataRange<char>(ruleName, 'a', 'z', pruning)));

        public MatchRuleSequence<char> MatchString(string? name = null, char delimiter = '"') =>
            FindOrRegister(name, CommonTokenNames.String,
                        (ruleName, pruning) =>
                            RegisterRule(new MatchRuleSequence<char>(
                                ruleName,
                                pruning,
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
                        (ruleName, pruning) =>
                            RegisterRule(
                                new MatchRuleSequence<char>(
                                    ruleName,
                                    pruning,
                                    0,
                                    Literal(startComment),
                                    ZeroOrMore(Sequence(Not(Literal(endComment)), Any())),
                                    Literal(endComment)
                                )
                            )
                        );

        public MatchDataSet<char> Sign(string? name = null) =>
            FindOrRegister(name, CommonTokenNames.Sign,
                        (ruleName, pruning) => RegisterRule(new MatchDataSet<char>(ruleName, pruning, ['+', '-'])));

        public MatchRuleSequence<char> SingleLineComment(string? name = null, string startComment = "//") =>
        FindOrRegister(name, CommonTokenNames.SingleLineComment,
                (ruleName, pruning) =>
                    RegisterRule(
                        new MatchRuleSequence<char>(
                            ruleName,
                            pruning,
                            0,
                            Literal(startComment),
                            ZeroOrMore(Sequence(Not(MatchSingle('\n')), Any()))
                        )
                    )
                );        

        public MatchSingleData<char> UnderscoreCharacter(string? name = null) =>
            FindOrRegister(name, CommonTokenNames.Underscore,
                        (ruleName, pruning) => RegisterRule(new MatchSingleData<char>(ruleName, '_', pruning)));

        public MatchDataRange<char> UpperCaseLetter(string? name = null) =>
            FindOrRegister(name, CommonTokenNames.UpperCaseLetter,
                        (ruleName, pruning) => RegisterRule(new MatchDataRange<char>(ruleName, 'A', 'Z', pruning)));

        public MatchDataSet<char> Whitespace(string? name = null) =>
            FindOrRegister(name, CommonTokenNames.Whitespace,
                        (ruleName, pruning) =>
                            RegisterRule(new MatchDataSet<char>(ruleName, pruning, [' ', '\r', '\n', '\t'])));
    }
}
