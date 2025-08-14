namespace gg.parse.ebnf
{
    public class ParseException : Exception
    {
        public IEnumerable<Annotation>? Errors { get; init; }

        public ParseException(string message)
            : base(message)
        {
        }

        public ParseException(string message, IEnumerable<Annotation> errors)
            : base(message)
        {
            Errors = errors;
        }
    }
}

