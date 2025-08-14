namespace gg.parse.ebnf
{
    public class TokenizeException : Exception
    {
        public IEnumerable<Annotation>? Errors { get; init; }

        public TokenizeException(string message)
            : base(message)
        {
        }

        public TokenizeException(string message, IEnumerable<Annotation> errors)
            : base(message)
        {
            Errors = errors;
        }
    }
}

