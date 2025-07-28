namespace gg.parse.tokenizer
{
    public static class BasicTokenizerFunctions
    {
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


        public static TokenFunction CreateDigitFunction(string name = "Digit") =>
            new CharacterRangeFunction(name, -1, '0', '9');

        public static TokenFunction AddDigit(this Tokenizer tokenizer, string name = "Digit") =>
            tokenizer.AddFunction(CreateDigitFunction(name));

        public static Tokenizer Digit(this Tokenizer tokenizer, string name = "Digit")
        {
            AddDigit(tokenizer, name);
            return tokenizer;
        }

        public static TokenFunction CreateDigitString(string name = "DigitString") =>
            CreateOneOrMoreFunction(CreateDigitFunction(), "DigitString");

        public static TokenFunction AddDigitString(this Tokenizer tokenizer, string name = "DigitString") =>
            tokenizer.AddFunction(CreateDigitString(name));

        public static Tokenizer DigitString(this Tokenizer tokenizer, string name = "DigitString")
        {
            AddDigitString(tokenizer, name);
            return tokenizer;
        }


        public static TokenFunction CreateSignFunction(string name = "Sign") =>
            new CharacterSetFunction(name, -1, "+-");


        public static TokenFunction AddSign(this Tokenizer tokenizer, string name = "Sign") =>
            tokenizer.AddFunction(CreateSignFunction(name));

        public static Tokenizer Sign(this Tokenizer tokenizer, string name = "Sign")
        {
            AddSign(tokenizer);
            return tokenizer;
        }

        public static TokenFunction CreateIntegerFunction(string name = "Integer") =>
            new MatchSequenceFunction(name, -1,
                [
                    CreateZeroOrOneFunction(CreateSignFunction(), "OptionalSign"),
                    CreateDigitString()
                ]);

        public static TokenFunction AddInteger(this Tokenizer tokenizer, string name = "Integer") =>
            tokenizer.AddFunction(CreateIntegerFunction(name));

        public static Tokenizer Integer(this Tokenizer tokenizer, string name = "Integer")
        {
            AddInteger(tokenizer, name);
            return tokenizer;
        }

        public static TokenFunction CreateFloatFunction(string name = "Float")
        {
            var digitString = CreateDigitString();

            var exponentPart = new MatchSequenceFunction("ExponentPart", -1,
                [
                    new LiteralFunction("ExponentCharacter", -1, "e"),
                    CreateZeroOrOneFunction(CreateSignFunction(), "OptionalSign"),
                    digitString
                ]);

            return new MatchSequenceFunction(name, -1,
                [
                    CreateZeroOrOneFunction(CreateSignFunction(), "OptionalSign"),
                    digitString,
                    new LiteralFunction("DecimalPoint", -1, "."),
                    digitString,
                    CreateZeroOrOneFunction(exponentPart, "OptionalExponentPart")
                ]);
        }

        public static TokenFunction AddFloat(this Tokenizer tokenizer, string name = "Float") =>
            tokenizer.AddFunction(CreateFloatFunction(name));

        public static Tokenizer Float(this Tokenizer tokenizer, string name = "Integer")
        {
            AddFloat(tokenizer, name);
            return tokenizer;
        }

        public static TokenFunction CreateWhitespaceFunction(string name = "Whitespace") =>
            new CharacterSetFunction(name, -1, " \t\r\n");

        public static TokenFunction AddWhitespace(this Tokenizer tokenizer, string name = "Whitespace") =>
            tokenizer.AddFunction(CreateWhitespaceFunction(name));

        public static Tokenizer Whitespace(this Tokenizer tokenizer, string name = "Whitespace")
        {
            AddWhitespace(tokenizer, name);
            return tokenizer;
        }

        public static TokenFunction CreateStringFunction(string name = "String")
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
            

            return new MatchSequenceFunction(name, -1, [quote, CreateZeroOrMoreFunction(stringCharacter), quote]);
        }

        public static TokenFunction AddString(this Tokenizer tokenizer, string name = "String") =>
            tokenizer.AddFunction(CreateStringFunction(name));

        public static Tokenizer String(this Tokenizer tokenizer, string name = "String")
        {
            tokenizer.AddFunction(CreateStringFunction(name));
            return tokenizer;
        }
    }
}
