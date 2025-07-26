using System.Data;

namespace gg.parse.rules
{
    public class OneOfRule : RuleBase
    {
        private List<IRule> _options = [];

        public static readonly string DefaultName = "OneOfRule";

        public OneOfRule() : base(DefaultName) 
        { 
        }

        public OneOfRule(string name) : base(name) 
        {
        }

        public OneOfRule(string name, params IRule[] options) : base(name)
        {
            _options = options.ToList();
        }

        public void AddOption(IRule option)
        {
            ArgumentNullException.ThrowIfNull(option);

            _options.Add(option);
        }

        public override ParseResult Parse(string text, int offset)
        {
            foreach (var option in _options)
            {
                var result = option.Parse(text, offset);
                if (result.ResultCode == ParseResult.Code.Success)
                {
                    return new ParseResult
                    {
                        Offset = offset,
                        Length = result.Length,
                        ResultCode = ParseResult.Code.Success,
                        Rule = this,
                        SubRule = result.Rule
                    };
                }
            }
            return new ParseResult
            {
                Offset = offset,
                Length = 0,
                ResultCode = ParseResult.Code.Failure,
                Rule = this
            };
        }
    }
}
