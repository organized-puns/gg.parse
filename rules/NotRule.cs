
namespace gg.parse.rules
{
    public class NotRule : RuleBase
    {
        public static readonly string DefaultName = "NotRule";

        public IRule Rule { get; set; }

        public NotRule(IRule rule, string? name = null)
            : base(name ?? DefaultName)
        {
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
        }

        public override ParseResult Parse(string text, int offset)
        {
            var result = Rule.Parse(text, offset);
            
            if (result.ResultCode == ParseResult.Code.Failure)
            {
                return new ParseResult
                {
                    ResultCode = ParseResult.Code.Success,
                    Length = result.Length,
                    Rule = this,
                    Offset = offset,
                };
            }
            else
            {
                return new ParseResult
                {
                    ResultCode = ParseResult.Code.Failure,
                    Length = result.Length,
                    Rule = this,
                    Offset = offset,
                };
            }
        }
    }
}
