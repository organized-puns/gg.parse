namespace gg.parse.ebnf
{
    public class EbnfException : Exception
    {
        public EbnfException() { }

        public EbnfException(string message) : base(message) { }

        public EbnfException(string message, Exception inner) : base(message, inner) { }
    }
}
