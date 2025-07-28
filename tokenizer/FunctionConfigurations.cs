namespace gg.parse.tokenizer
{
    public class LiteralFunction(string name, int id, string literal, TokenAction action = TokenAction.GenerateToken)
    : TokenFunction(name, id, action)
    {
        public string Literal { get; init; } = literal;

        public override Annotation? Parse(string text, int start)
        {
            var range = TokenizerFunctions.Literal(text, start, Literal);
            return range.HasValue
                ? new Annotation(AnnotationCategory.Token, Id, range.Value)
                : null;
        }
    }

    public class AnyCharacterFunction(string name, int id, int min = 0, int max = 1, TokenAction action = TokenAction.GenerateToken)
    : TokenFunction(name, id, action)
    {
        public int Min { get; init; } = min;

        public int Max { get; init; } = max;

        public override Annotation? Parse(string text, int start)
        {
            var range = TokenizerFunctions.AnyCharacter(text, start, Min, Max);
            return range.HasValue
                ? new Annotation(AnnotationCategory.Token, Id, range.Value)
                : null;
        }
    }

    public class CharacterRangeFunction(string name, int id, char min = 'a', char max = 'Z', TokenAction action = TokenAction.GenerateToken)
        : TokenFunction(name, id, action)
    {
        public char Min { get; init; } = min;

        public char Max { get; init; } = max;

        public override Annotation? Parse(string text, int start)
        {
            var range = TokenizerFunctions.InCharacterRange(text, start, Min, Max);
            return range.HasValue
                ? new Annotation(AnnotationCategory.Token, Id, range.Value)
                : null;
        }
    }

    public class CharacterSetFunction(string name, int id, string set, TokenAction action = TokenAction.GenerateToken)
        : TokenFunction(name, id, action)
    {
        public string Set { get; init; } = set;

        public override Annotation? Parse(string text, int start)
        {
            var range = TokenizerFunctions.InCharacterSet(text, start, Set);
            return range.HasValue
                ? new Annotation(AnnotationCategory.Token, Id, range.Value)
                : null;
        }      
    }

    public class MatchCountFunction(string name, int id, TokenFunction function, int min = 0, int max = 0, TokenAction action = TokenAction.GenerateToken)
        : TokenFunction(name, id, action)
    {
        public TokenFunction Function { get; init; } = function;
        
        public int Min { get; init; } = min;

        public int Max { get; init; } = max;

        public override Annotation? Parse(string text, int start)
        {
            var range = TokenizerFunctions.MatchCount(text, start, Function, Min, Max);
            return range.HasValue
                ? new Annotation(AnnotationCategory.Token, Id, range.Value)
                : null;
        }
    }

    public class MatchSequenceFunction(string name, int id, TokenFunction[] sequence, TokenAction action = TokenAction.GenerateToken)
        : TokenFunction(name, id, action)
    {
        public TokenFunction[] Sequence { get; init; } = sequence;

        public override Annotation? Parse(string text, int start)
        {
            var range = TokenizerFunctions.MatchSequence(text, start, Sequence);
            return range.HasValue
                ? new Annotation(AnnotationCategory.Token, Id, range.Value)
                : null;
        }
    }

    public class MatchOneOfFunction(string name, int id, TokenFunction[] options, TokenAction action = TokenAction.GenerateToken)
        : TokenFunction(name, id, action)
    {
        public TokenFunction[] Options { get; init; } = options;

        public override Annotation? Parse(string text, int start)
        {
            var range = TokenizerFunctions.MatchOneOf(text, start, Options);
            return range.HasValue
                ? new Annotation(AnnotationCategory.Token, Id, range.Value)
                : null;
        }
    }

    public class MatchNotFunction(string name, int id, TokenFunction function,  TokenAction action = TokenAction.GenerateToken)
        : TokenFunction(name, id, action)
    {
        public TokenFunction Function { get; init; } = function;
        
        public override Annotation? Parse(string text, int start)
        {
            var range = TokenizerFunctions.MatchNot(text, start, Function);
            return range.HasValue
                ? new Annotation(AnnotationCategory.Token, Id, range.Value)
                : null;
        }
    }

    public class MarkErrorFunction(string name, int id, string message, int skipCharacters = 0, TokenAction action = TokenAction.Error)
        : TokenFunction(name, id, action)
    {
        public string Message { get; init; } = message;

        public int SkipCharacters { get; init; } = skipCharacters;

        public override Annotation? Parse(string text, int start)
        {
            return new Annotation(AnnotationCategory.Error, Id, new Range(start, SkipCharacters));
        }
    }
}
