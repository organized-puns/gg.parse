namespace gg.parse.tokenizer
{
    public enum TokenAction
    {
        GenerateToken,
        IgnoreToken,
        Error
    }


    public abstract class TokenFunction(string name, int id, TokenAction action = TokenAction.GenerateToken)
    {
        public string Name { get; init; } = name;

        public int Id { get; set; } = id;

        public TokenAction ActionOnMatch { get; init; } = action;

        public abstract Annotation? Parse(string text, int start);

        public override string ToString()
        {
            return Name;
        }
    }
}
