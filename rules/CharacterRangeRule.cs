namespace gg.parse.rules
{
    public class CharacterRangeRule : RuleBase
    {
        public char Start { get; set; }
        
        public char End { get; set; }

        public CharacterRangeRule(string name, char start, char end)
            : base(name)
        {
            Start = start;
            End = end;
        }
        
        public override ParseResult Parse(string text, int offset)
        {
            if (text[offset] >= Start && text[offset] <= End)
            {
                return new ParseResult
                {
                    ResultCode = ParseResult.Code.Success,
                    Length = 1,
                    Rule = this,
                    Offset = offset,
                };
            }
            else
            {
                return new ParseResult
                {
                    ResultCode = ParseResult.Code.Failure,
                    Length = 1,
                    Rule = this,
                    Offset = offset,
                };
            }
        }
    }
}
