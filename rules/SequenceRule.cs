namespace gg.parse.rules
{
    public class SequenceRule : RuleBase
    {
        private List<IRule> _sequence = [];

        public static readonly string DefaultName = "SequenceRule";

        public SequenceRule() : base(DefaultName) 
        {
        }

        public SequenceRule(string name) : base(name)
        {
        }

        public SequenceRule(string name, params IRule[] sequence) : base(name)
        {
            _sequence = sequence.ToList();
        }

        public void AddRule(IRule rule)
        {
            ArgumentNullException.ThrowIfNull(rule);

            _sequence.Add(rule);
        }

        public override ParseResult Parse(string text, int offset = 0)
        {
            var index = offset;

            foreach (var rule in _sequence)
            {
                var result = rule.Parse(text, index);
                if (result.ResultCode != ParseResult.Code.Success)
                {
                    return new ParseResult
                    {
                        Offset = index,
                        Length = 0,
                        ResultCode = ParseResult.Code.Failure,
                        Rule = this,
                        SubRule = rule
                    };
                }
                index += result.Length;
            }

            return new ParseResult
            {
                Offset = offset,
                Length = index - offset,
                ResultCode = ParseResult.Code.Success,
                Rule = this,
            };
        }
    }
}
