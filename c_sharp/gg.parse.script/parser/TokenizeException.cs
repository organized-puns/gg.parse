namespace gg.parse.script.parser
{
    public class TokenizeException : Exception
    {
        public List<Annotation>? Errors { get; init; }

        public string? Text { get; init; }

        public TokenizeException(string message)
            : base(message)
        {
        }

        public TokenizeException(string message, IEnumerable<Annotation> errors, string? text)
            : base(message)
        {
            Errors = errors.ToList();
            Text = text;
        }

        public void WriteErrors(Action<string> writeError)
        {
            if (Errors != null && Text != null)
            {
                var errorMessages = Errors.Select(token => $"Token error at: {Text.Substring(token.Start, token.Length)}.");
                foreach (var errorMessage in errorMessages)
                {
                    writeError(errorMessage);
                }
            }
        }
    }
}

