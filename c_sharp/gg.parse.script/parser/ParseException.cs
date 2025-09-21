namespace gg.parse.script.parser
{
    public class ParseException : Exception
    {
        public List<Annotation>? Errors { get; init; }

        /// <summary>
        /// Text in which the errors occurred.
        /// </summary>
        public string? Text { get; init; }

        /// <summary>
        /// Tokens which were being parsed when the error occurred.
        /// </summary>
        public List<Annotation>? Tokens { get; init; }

        public ParseException(string message)
            : base(message)
        {
        }

        public ParseException(string message, List<Annotation> errors, string text, List<Annotation> tokens)
            : base(message)
        {
            Errors = errors;
            Text = text;
            Tokens = tokens;
        }

        public void WriteErrors(Action<string> writeError)
        {
            if (Errors != null && Text != null && Tokens != null)
            {
                var errorMessages = Errors.Select(annotation => $"Parse error at: {annotation.GetText(Text, Tokens)}.");
                foreach (var errorMessage in errorMessages)
                {
                    writeError(errorMessage);
                }
            }
        }
    }
}

