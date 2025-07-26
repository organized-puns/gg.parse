namespace gg.parse.rules
{
    public class ErrorRule : RuleBase
    {
        public IRule Trigger { get; set; } = null!; 

        public ErrorRule(string name)
            : base(name)
        {
        }

        public override ParseResult Parse(string text, int offset)
        {
            if (Trigger.Parse(text, offset).ResultCode == ParseResult.Code.Failure)
            {
                return new ParseResult
                {
                    ResultCode = ParseResult.Code.Failure,
                    Length = 0,
                    Rule = this,
                    SubRule = Trigger,
                    Offset = offset
                };
            }
            else
            {
                return new ParseResult
                {
                    ResultCode = ParseResult.Code.Success,
                    Length = 0,
                    Rule = this,
                    SubRule = Trigger,
                    Offset = offset
                };
            }
        }
    }
}
