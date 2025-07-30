
namespace gg.parse.tokenizer
{
    public static class TokenizerExtensions
    {
        public static TokenFunction AddAnyCharacter(this Tokenizer tokenizer, string name, int min = 0, int max = 1, ProductionEnum action = ProductionEnum.ProduceItem) =>
            tokenizer.AddFunction(new AnyCharacterFunction(name, tokenizer.NextFunctionId, min, max, action));

        public static Tokenizer Any(this Tokenizer tokenizer, string name, int min = 0, int max = 1, ProductionEnum action = ProductionEnum.ProduceItem)
        {
            AddAnyCharacter(tokenizer, name, min, max, action);
            return tokenizer;
        }

        public static TokenFunction AddLiteral(this Tokenizer tokenizer, string name, string literal, ProductionEnum action = ProductionEnum.ProduceItem) =>
            tokenizer.AddFunction(new LiteralFunction(name, tokenizer.NextFunctionId, literal, action));

        public static Tokenizer Literal(this Tokenizer tokenizer, string name, string literal, ProductionEnum action = ProductionEnum.ProduceItem)
        {
            AddLiteral(tokenizer, name, literal, action);
            return tokenizer;
        }

        public static TokenFunction AddCharacterRange(this Tokenizer tokenizer, string name, char min = 'a', char max = 'Z', ProductionEnum action = ProductionEnum.ProduceItem) =>
            tokenizer.AddFunction(new AnyCharacterFunction(name, tokenizer.NextFunctionId, min, max, action));

        public static Tokenizer CharacterRange(this Tokenizer tokenizer, string name, char min = 'a', char max = 'Z', ProductionEnum action = ProductionEnum.ProduceItem)
        {
            AddCharacterRange(tokenizer, name, min, max, action);
            return tokenizer;
        }

        public static TokenFunction AddCharacterSet(this Tokenizer tokenizer, string name, string set, ProductionEnum action = ProductionEnum.ProduceItem) =>
            tokenizer.AddFunction(new CharacterSetFunction(name, tokenizer.NextFunctionId, set, action));

        public static Tokenizer CharacterSet(this Tokenizer tokenizer, string name, string set, ProductionEnum action = ProductionEnum.ProduceItem)
        {
            AddCharacterSet(tokenizer, name, set, action);
            return tokenizer;
        }

        public static TokenFunction AddMatchCount(this Tokenizer tokenizer, string name, TokenFunction function, int min = 0, int max = 0, ProductionEnum action = ProductionEnum.ProduceItem) =>
            tokenizer.AddFunction(new MatchCountFunction(name, tokenizer.NextFunctionId, function, min, max, action));

        public static Tokenizer MatchCount(this Tokenizer tokenizer, string name, TokenFunction function, int min = 0, int max = 0, ProductionEnum action = ProductionEnum.ProduceItem)
        {
            AddMatchCount(tokenizer, name, function, min, max, action);
            return tokenizer;
        }

        public static TokenFunction AddMatchSequence(this Tokenizer tokenizer, string name, TokenFunction[] sequence, ProductionEnum action = ProductionEnum.ProduceItem) =>
            tokenizer.AddFunction(new MatchSequenceFunction(name, tokenizer.NextFunctionId, sequence, action));

        public static Tokenizer Sequence(this Tokenizer tokenizer, string name, TokenFunction[] sequence, ProductionEnum action = ProductionEnum.ProduceItem)
        {
            AddMatchSequence(tokenizer, name, sequence, action);
            return tokenizer;
        }

        public static TokenFunction AddMatchOneOf(this Tokenizer tokenizer, string name, TokenFunction[] options, ProductionEnum action = ProductionEnum.ProduceItem) =>
            tokenizer.AddFunction(new MatchOneOfFunction(name, tokenizer.NextFunctionId, options, action));

        public static Tokenizer OneOf(this Tokenizer tokenizer, string name, TokenFunction[] options, ProductionEnum action = ProductionEnum.ProduceItem)
        {
            AddMatchOneOf(tokenizer, name, options, action);
            return tokenizer;
        }

        public static TokenFunction AddMatchNot(this Tokenizer tokenizer, string name, TokenFunction function, ProductionEnum action = ProductionEnum.ProduceItem) =>
            tokenizer.AddFunction(new MatchNotFunction(name, tokenizer.NextFunctionId, function, action));

        public static Tokenizer Not(this Tokenizer tokenizer, string name, TokenFunction function, ProductionEnum action = ProductionEnum.ProduceItem)
        {
            AddMatchNot(tokenizer, name, function, action);
            return tokenizer;
        }
    }
}
