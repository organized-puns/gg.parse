namespace gg.parse.rules
{
    public class CountRule : RuleBase
    {
        public static readonly string DefaultName = "CountRule";

        public int Min { get; set; } = 0;

        public int Max { get; set; } = 1;

        public IRule Rule { get; set; }

        public CountRule(string name, IRule rule, int min = 0, int max = 1)
            : base(name)
        {
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
            Min = min;
            Max = max;
        }


        public override ParseResult Parse(string text, int offset)
        {
            int count = 0;
            int index = offset;

            while (index < text.Length && (Max <= 0 || count < Max))
            {
                var result = Rule.Parse(text, index);
                if (result.ResultCode != ParseResult.Code.Success)
                {
                    break;
                }
                count++;
                index += result.Length;
            }

            if (Min <= 0 || count >= Min)
            {
                return new ParseResult
                {
                    Offset = index,
                    Length = index - offset,
                    ResultCode = ParseResult.Code.Success,
                    Rule = this,
                    SubRule = Rule
                };
            }

            return new ParseResult
            {
                Offset = offset,
                Length = index - offset,
                ResultCode = ParseResult.Code.Failure,
                Rule = this,
                SubRule = Rule
            };
        }
    }
}
