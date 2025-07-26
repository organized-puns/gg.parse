namespace gg.parse.rules
{
    public class LiteralRule : RuleBase
    {
        public static readonly string DefaultName = "LiteralRule";

        public string Literal
        {
            get; init;
        }

        public LiteralRule(string name, string literal)
            : base(name)
        {
            Literal = literal;
        }

        public LiteralRule(string literal)
            : this(DefaultName, literal)
        {
        }

        public override ParseResult Parse(string text, int offset)
        {
            for (var i = 0; i < Literal.Length; i++)
            {
                if (offset + i >= text.Length || text[offset + i] != Literal[i])
                {
                    return new ParseResult
                    {
                        Offset = offset,
                        Length = i,
                        ResultCode = ParseResult.Code.Failure,
                        Rule = this
                    };
                }
            }

            return new ParseResult
            {
                Offset = offset,
                Length = Literal.Length,
                ResultCode = ParseResult.Code.Success,
                Rule = this
            };
        }
    }
}
