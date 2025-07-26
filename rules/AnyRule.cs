namespace gg.parse.rules
{
    public class AnyRule : RuleBase
    {
        public static readonly string DefaultName = "AnyRule";

        public int MinLength { get; set; } = 0;

        public int MaxLength { get; set; } = 1;

        public AnyRule(string name, int minLength, int maxLength)
            : base(name)
        {
            MinLength = minLength;
            MaxLength = maxLength;
        }

        public AnyRule(string name)
            : this(name, 0, 1)
        {
        }

        public AnyRule()
            : this(DefaultName, 0, 1)
        {
        }

        public override ParseResult Parse(string text, int offset)
        {
            var charactersLeft = text.Length - offset;

            if (charactersLeft < MinLength)
            {
                return new ParseResult
                {
                    Offset = offset,
                    Length = charactersLeft,
                    ResultCode = ParseResult.Code.Failure,
                    Rule = this
                };
            }

            if (MaxLength <= 0)
            {
                return new ParseResult
                {
                    Offset = offset,
                    Length = charactersLeft,
                    ResultCode = ParseResult.Code.Success,
                    Rule = this
                };
            }

            return new ParseResult
            {
                Offset = offset,
                Length = Math.Min(charactersLeft, MaxLength),
                ResultCode = ParseResult.Code.Success,
                Rule = this
            };
        }
    }
    
}
