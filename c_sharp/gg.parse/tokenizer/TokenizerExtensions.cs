
namespace gg.parse.tokenizer
{
    public static class TokenizerExtensions
    {
        public static TokenFunction AddAnyCharacter(this Tokenizer tokenizer, string name, int min = 0, int max = 1, TokenAction action = TokenAction.GenerateToken) =>
            tokenizer.AddFunction(new AnyCharacterFunction(name, tokenizer.NextFunctionId, min, max, action));

        public static Tokenizer Any(this Tokenizer tokenizer, string name, int min = 0, int max = 1, TokenAction action = TokenAction.GenerateToken)
        {
            AddAnyCharacter(tokenizer, name, min, max, action);
            return tokenizer;
        }

        public static TokenFunction AddLiteral(this Tokenizer tokenizer, string name, string literal, TokenAction action = TokenAction.GenerateToken) =>
            tokenizer.AddFunction(new LiteralFunction(name, tokenizer.NextFunctionId, literal, action));

        public static Tokenizer Literal(this Tokenizer tokenizer, string name, string literal, TokenAction action = TokenAction.GenerateToken)
        {
            AddLiteral(tokenizer, name, literal, action);
            return tokenizer;
        }

        public static TokenFunction AddCharacterRange(this Tokenizer tokenizer, string name, char min = 'a', char max = 'Z', TokenAction action = TokenAction.GenerateToken) =>
            tokenizer.AddFunction(new AnyCharacterFunction(name, tokenizer.NextFunctionId, min, max, action));

        public static Tokenizer CharacterRange(this Tokenizer tokenizer, string name, char min = 'a', char max = 'Z', TokenAction action = TokenAction.GenerateToken)
        {
            AddCharacterRange(tokenizer, name, min, max, action);
            return tokenizer;
        }

        public static TokenFunction AddCharacterSet(this Tokenizer tokenizer, string name, string set, TokenAction action = TokenAction.GenerateToken) =>
            tokenizer.AddFunction(new CharacterSetFunction(name, tokenizer.NextFunctionId, set, action));

        public static Tokenizer CharacterSet(this Tokenizer tokenizer, string name, string set, TokenAction action = TokenAction.GenerateToken)
        {
            AddCharacterSet(tokenizer, name, set, action);
            return tokenizer;
        }

        public static TokenFunction AddMatchCount(this Tokenizer tokenizer, string name, TokenFunction function, int min = 0, int max = 0, TokenAction action = TokenAction.GenerateToken) =>
            tokenizer.AddFunction(new MatchCountFunction(name, tokenizer.NextFunctionId, function, min, max, action));

        public static Tokenizer MatchCount(this Tokenizer tokenizer, string name, TokenFunction function, int min = 0, int max = 0, TokenAction action = TokenAction.GenerateToken)
        {
            AddMatchCount(tokenizer, name, function, min, max, action);
            return tokenizer;
        }

        public static TokenFunction AddMatchSequence(this Tokenizer tokenizer, string name, TokenFunction[] sequence, TokenAction action = TokenAction.GenerateToken) =>
            tokenizer.AddFunction(new MatchSequenceFunction(name, tokenizer.NextFunctionId, sequence, action));

        public static Tokenizer Sequence(this Tokenizer tokenizer, string name, TokenFunction[] sequence, TokenAction action = TokenAction.GenerateToken)
        {
            AddMatchSequence(tokenizer, name, sequence, action);
            return tokenizer;
        }

        public static TokenFunction AddMatchOneOf(this Tokenizer tokenizer, string name, TokenFunction[] options, TokenAction action = TokenAction.GenerateToken) =>
            tokenizer.AddFunction(new MatchOneOfFunction(name, tokenizer.NextFunctionId, options, action));

        public static Tokenizer OneOf(this Tokenizer tokenizer, string name, TokenFunction[] options, TokenAction action = TokenAction.GenerateToken)
        {
            AddMatchOneOf(tokenizer, name, options, action);
            return tokenizer;
        }

        public static TokenFunction AddMatchNot(this Tokenizer tokenizer, string name, TokenFunction function, TokenAction action = TokenAction.GenerateToken) =>
            tokenizer.AddFunction(new MatchNotFunction(name, tokenizer.NextFunctionId, function, action));

        public static Tokenizer Not(this Tokenizer tokenizer, string name, TokenFunction function, TokenAction action = TokenAction.GenerateToken)
        {
            AddMatchNot(tokenizer, name, function, action);
            return tokenizer;
        }
    }
}
