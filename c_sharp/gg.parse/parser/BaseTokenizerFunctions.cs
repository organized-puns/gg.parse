
using gg.parse.basefunctions;

namespace gg.parse.parser
{
    public static class BaseTokenizerFunctions
    {
        public static ParseFunctionBase<char> Digit(string? name = null, ProductionEnum action = ProductionEnum.ProduceItem) =>
            // {'0'..'9'}
            new MatchDataRange<char>(name ?? TokenNames.Digit, -1, action, '0', '9');

        public static ParseFunctionBase<char> DigitSequence(string? name = null, ProductionEnum action = ProductionEnum.ProduceItem) =>
            // {'0'..'9'}+
            OneOrMore(Digit(), name, action);

        public static ParseFunctionBase<char> Integer(string? name = null, ProductionEnum action = ProductionEnum.ProduceItem) =>
            new MatchFunctionSequence<char>(name ?? TokenNames.Integer, -1, action,
                [
                    ZeroOrOne(Sign()),
                    DigitSequence()
                ]);

        public static ParseFunctionBase<char> Literal(string literal, string? name = null, ProductionEnum action = ProductionEnum.ProduceItem) =>
            new MatchDataSequence<char>(name ?? $"{TokenNames.Literal}({literal})", -1, action, literal.ToCharArray());

        public static ParseFunctionBase<char> Sign(string? name = null, ProductionEnum action = ProductionEnum.ProduceItem) =>
            // {'+', '-'}
            new MatchDataSet<char>(name ?? TokenNames.Sign, -1, action, ['+', '-']);

        public static ParseFunctionBase<char> Whitespace(string? name = null, ProductionEnum action = ProductionEnum.Ignore) =>
            // {' ', '\r', '\n', '\t' }
            new MatchDataSet<char>(name ?? TokenNames.Whitespace, -1, action, [' ', '\r', '\n', '\t']);


        public static ParseFunctionBase<char> String(
            string? name = null, ProductionEnum action = ProductionEnum.ProduceItem, char delimiter = '"')
        {
            // '"', ('\\"' or (not '"', any) )*, '"'
            // '"', ('\\"' | (!'"', _) )*, '"'
            var quote = InSet(delimiter);
            var escapedQuote = Sequence('\\', delimiter);            
            var notQuoteThenAny = Sequence(Not(quote), Any());
            var stringCharacters = ZeroOrMore(OneOf(escapedQuote, notQuoteThenAny));

            return new MatchFunctionSequence<char>(name ?? TokenNames.String, -1, action,
                [
                    quote,
                    stringCharacters,
                    quote
                ]);
        }

        public static ParseFunctionBase<char> Float(
            string? name = null, ProductionEnum action = ProductionEnum.ProduceItem)
        {
            // sign?, digitSequence, '.', digitSequence, (('e' | 'E'), sign?, digitSequence)?
            var digitSequence = DigitSequence();
            var sign = ZeroOrOne(Sign());
            var exponentPart = Sequence(InSet(['e', 'E']), sign, digitSequence );

            return new MatchFunctionSequence<char>(name ?? TokenNames.Float, -1, action,
                [
                    sign,
                    digitSequence,
                    Literal("."),
                    digitSequence,
                    ZeroOrOne(exponentPart)
                ]);
        }

        public static ParseFunctionBase<char> Boolean(
            string? name = null, ProductionEnum action = ProductionEnum.ProduceItem) =>
            // 'true' | 'false'
            new MatchOneOfFunction<char>(name ?? TokenNames.Boolean, -1, action,
                                    Literal("true"),
                                    Literal("false"));


        public static ParseFunctionBase<char> Any(string? name = null, ProductionEnum action = ProductionEnum.ProduceItem) =>
            new MatchAnyData<char>(name ?? TokenNames.AnyCharacter, -1, action);

        public static ParseFunctionBase<char> Not(ParseFunctionBase<char> function) =>
            new MatchNotFunction<char>($"{TokenNames.Not}({function.Name})", -1, ProductionEnum.ProduceItem, function);

        public static ParseFunctionBase<char> Sequence(params ParseFunctionBase<char>[] functions) =>
            new MatchFunctionSequence<char>($"{TokenNames.FunctionSequence}({string.Join(", ", functions.Select(f => f.Name))})", -1, ProductionEnum.ProduceItem, functions);

        public static ParseFunctionBase<char> Sequence(params char[] characters) =>
            new MatchDataSequence<char>($"{TokenNames.DataSequence}({string.Join(", ", characters)})", -1, ProductionEnum.ProduceItem, characters);

        public static ParseFunctionBase<char> OneOf(params ParseFunctionBase<char>[] functions) =>
            new MatchOneOfFunction<char>($"{TokenNames.OneOf}({string.Join(", ", functions.Select(f=>f.Name))})", -1, ProductionEnum.ProduceItem, functions);

        public static ParseFunctionBase<char> OneOrMore(ParseFunctionBase<char> function, string? name = null, ProductionEnum action = ProductionEnum.ProduceItem) =>
            new MatchFunctionCount<char>(name ?? $"{TokenNames.OneOrMore}({function.Name})", -1, action, function, 1, 0);

        public static ParseFunctionBase<char> InSet(params char[] set) =>
            new MatchDataSet<char>($"{TokenNames.Set}({string.Join(", ", set)})", -1, ProductionEnum.ProduceItem, set);


        public static ParseFunctionBase<char> ZeroOrOne(ParseFunctionBase<char> function, string? name = null, ProductionEnum action = ProductionEnum.ProduceItem) =>
            new MatchFunctionCount<char>(name ?? $"{TokenNames.ZeroOrOne}({function.Name})", -1, action, function, 0, 1);

        public static ParseFunctionBase<char> ZeroOrMore(ParseFunctionBase<char> function, string? name = null, ProductionEnum action = ProductionEnum.ProduceItem) =>
            new MatchFunctionCount<char>(name ?? $"{TokenNames.ZeroOrMore}({function.Name})", -1, action, function, 0, 0);


    }
}
