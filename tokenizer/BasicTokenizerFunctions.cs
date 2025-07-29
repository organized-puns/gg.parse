namespace gg.parse.tokenizer
{
    public static class BasicTokenizerFunctions
    {
        public static class TokenNames
        {
            public const string Digit = "Digit";
            public const string Sign = "Sign";
            public const string DigitString = "DigitString";
            public const string String = "String";
            public const string Float = "Float";
            public const string Integer = "Integer";
            public const string Boolean = "Boolean";
            public const string Whitespace = "Whitespace";
        }

        public static TokenFunction CreateOneOrMoreFunction(TokenFunction function, string name = "OneOrMore") =>
            new MatchCountFunction(name, -1, function, 1, 0);

        public static TokenFunction AddOneOrMore(this Tokenizer tokenizer, TokenFunction function, string name = "OneOrMore") =>
            tokenizer.AddFunction(CreateOneOrMoreFunction(function, name));

        public static Tokenizer OneOrMore(this Tokenizer tokenizer, TokenFunction function, string name = "OneOrMore")
        {
            AddOneOrMore(tokenizer, function, name);
            return tokenizer;
        }


        public static TokenFunction CreateZeroOrMoreFunction(TokenFunction function, string name = "ZeroOrMore") =>
            new MatchCountFunction(name + $"({function.Name})", -1, function, 0, 0);

        public static TokenFunction AddZeroOrMore(this Tokenizer tokenizer, TokenFunction function, string name = "ZeroOrMore") =>
            tokenizer.AddFunction(CreateOneOrMoreFunction(function, name));

        public static Tokenizer ZeroOrMore(this Tokenizer tokenizer, TokenFunction function, string name = "ZeroOrMore")
        {
            AddZeroOrMore(tokenizer, function, name);
            return tokenizer;
        }


        public static TokenFunction CreateZeroOrOneFunction(TokenFunction function, string name = "ZeroOrOne") =>
            new MatchCountFunction(name + $"({function.Name})", -1, function, 0, 1);

        public static TokenFunction AddZeroOrOne(this Tokenizer tokenizer, TokenFunction function, string name = "ZeroOrOne") =>
            tokenizer.AddFunction(CreateZeroOrOneFunction(function, name));

        public static Tokenizer ZeroOrOne(this Tokenizer tokenizer, TokenFunction function, string name = "ZeroOrOne")
        {
            AddZeroOrOne(tokenizer, function, name);
            return tokenizer;
        }


        public static TokenFunction CreateDigitFunction(string? name = null) =>
            new CharacterRangeFunction(name ?? TokenNames.Digit, -1, '0', '9');

        public static TokenFunction AddDigit(this Tokenizer tokenizer, string? name = null) =>
            tokenizer.AddFunction(CreateDigitFunction(name));

        public static Tokenizer Digit(this Tokenizer tokenizer, string? name = null)
        {
            AddDigit(tokenizer, name);
            return tokenizer;
        }

        public static TokenFunction CreateDigitString(string? name = null) =>
            CreateOneOrMoreFunction(CreateDigitFunction(), name ?? TokenNames.DigitString);

        public static TokenFunction AddDigitString(this Tokenizer tokenizer, string? name = null) =>
            tokenizer.AddFunction(CreateDigitString(name));

        public static Tokenizer DigitString(this Tokenizer tokenizer, string? name = null)
        {
            AddDigitString(tokenizer, name);
            return tokenizer;
        }


        public static TokenFunction CreateSignFunction(string? name = null) =>
            new CharacterSetFunction(name ?? TokenNames.Sign, -1, "+-");


        public static TokenFunction AddSign(this Tokenizer tokenizer, string? name = null) =>
            tokenizer.AddFunction(CreateSignFunction(name));

        public static Tokenizer Sign(this Tokenizer tokenizer, string? name = null)
        {
            AddSign(tokenizer);
            return tokenizer;
        }

        public static TokenFunction CreateIntegerFunction(string? name = null) =>
            new MatchSequenceFunction(name ?? TokenNames.Integer, -1,
                [
                    CreateZeroOrOneFunction(CreateSignFunction(), "OptionalSign"),
                    CreateDigitString()
                ]);

        public static TokenFunction AddInteger(this Tokenizer tokenizer, string? name = null) =>
            tokenizer.AddFunction(CreateIntegerFunction(name));

        public static Tokenizer Integer(this Tokenizer tokenizer, string? name = null)
        {
            AddInteger(tokenizer, name);
            return tokenizer;
        }

        public static TokenFunction CreateFloatFunction(string? name = null)
        {
            var digitString = CreateDigitString();

            var exponentPart = new MatchSequenceFunction("ExponentPart", -1,
                [
                    new CharacterSetFunction("ExponentCharacter", -1, "eE"),
                    CreateZeroOrOneFunction(CreateSignFunction(), "OptionalSign"),
                    digitString
                ]);

            return new MatchSequenceFunction(name ?? TokenNames.Float, -1,
                [
                    CreateZeroOrOneFunction(CreateSignFunction(), "OptionalSign"),
                    digitString,
                    new LiteralFunction("DecimalPoint", -1, "."),
                    digitString,
                    CreateZeroOrOneFunction(exponentPart, "OptionalExponentPart")
                ]);
        }

        public static TokenFunction AddFloat(this Tokenizer tokenizer, string? name = null) =>
            tokenizer.AddFunction(CreateFloatFunction(name));

        public static Tokenizer Float(this Tokenizer tokenizer, string? name = null)
        {
            AddFloat(tokenizer, name);
            return tokenizer;
        }

        public static TokenFunction CreateWhitespaceFunction(string? name = null, TokenAction onMatch = TokenAction.IgnoreToken) =>
            new CharacterSetFunction(name ?? TokenNames.Whitespace, -1, " \t\r\n", onMatch);

        public static TokenFunction AddWhitespace(this Tokenizer tokenizer, string? name = null, TokenAction onMatch = TokenAction.IgnoreToken) =>
            tokenizer.AddFunction(CreateWhitespaceFunction(name));

        public static Tokenizer Whitespace(this Tokenizer tokenizer, string? name = null, TokenAction onMatch = TokenAction.IgnoreToken)
        {
            AddWhitespace(tokenizer, name);
            return tokenizer;
        }

        public static TokenFunction CreateStringFunction(string? name = null)
        {
            var quote = new LiteralFunction("Quote", -1, "\"");
            var escapedQuote = new LiteralFunction("EscapedQuote", -1, "\\\"");
            var notQuote = new MatchNotFunction("Not quote", -1, quote);
            var notQuoteElseAny = new MatchSequenceFunction("NotQuote", -1, 
                [
                    notQuote, 
                    new AnyCharacterFunction("Character", -1, 0, 1)
                ]);

            var stringCharacter = new MatchOneOfFunction("StringCharacter", -1, 
                [
                    escapedQuote, 
                    notQuoteElseAny
                ]);
            

            return new MatchSequenceFunction(name ?? TokenNames.String, -1, [quote, CreateZeroOrMoreFunction(stringCharacter), quote]);
        }

        public static TokenFunction AddString(this Tokenizer tokenizer, string? name = null) =>
            tokenizer.AddFunction(CreateStringFunction(name));

        public static Tokenizer String(this Tokenizer tokenizer, string? name = null)
        {
            tokenizer.AddFunction(CreateStringFunction(name));
            return tokenizer;
        }

        public static TokenFunction CreateBooleanFunction(string? name = null) =>
        
            new MatchOneOfFunction(name ?? TokenNames.Boolean, -1, 
                [
                    new LiteralFunction("True", -1, "true"),
                    new LiteralFunction("True", -1, "false"),
                ]);

        public static TokenFunction AddBoolean(this Tokenizer tokenizer, string? name = null) =>
            tokenizer.AddFunction(CreateBooleanFunction(name));

        public static Tokenizer Boolean(this Tokenizer tokenizer, string? name = null)
        {
            AddBoolean(tokenizer, name);
            return tokenizer;
        }

        public static TokenFunction CreateDelimiterFunction(string[] delimiter, string name) =>

            new MatchOneOfFunction(name, -1,
            [
                new LiteralFunction($"{name}Start", -1, delimiter[0]),
                new LiteralFunction($"{name}End", -1, delimiter[1]),
            ]);

        public static TokenFunction AddDelimiter(this Tokenizer tokenizer, string[] delimiter, string name) =>
            tokenizer.AddFunction(CreateDelimiterFunction(delimiter, name));

        public static Tokenizer Delimiter(this Tokenizer tokenizer,string[] delimiter, string name)
        {
            AddDelimiter(tokenizer, delimiter, name);
            return tokenizer;
        }



    }
}
