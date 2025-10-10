using gg.parse.util;

using Range = gg.parse.util.Range;

namespace gg.parse
{

    public abstract class RuleBase<T>(
        string name, 
        RuleOutput output = RuleOutput.Self, 
        int precedence = 0) 
        : IRule
    {
        public string Name { get; init; } = name;

        public int Id { get; set; } = -1;

        public int Precedence { get; init; } = precedence;

        public RuleOutput Output { get; init; } = output;

        
        public abstract ParseResult Parse(T[] input, int start);

        public override string ToString() => $"{Name}(id:{Id},out:{Output},pre:{Precedence})";

        public ParseResult BuildDataRuleResult(Range dataRange) 
        {
            return Output switch
            {
                RuleOutput.Self => 
                    new ParseResult(true, dataRange.Length, [new Annotation(this, dataRange)]),

                RuleOutput.Children => 
                    new ParseResult(true, dataRange.Length, [new Annotation(this, dataRange)]),

                RuleOutput.Void => 
                    new ParseResult(true, dataRange.Length),
  
                _ => throw new NotImplementedException($"No implementation to build a data rule result for enum value {Output}."),
            };
        }

        public ParseResult BuildResult(Range dataRange, List<Annotation>? children = null)
        {
            return Output switch
            {
                RuleOutput.Self => new ParseResult(true, dataRange.Length, [ new Annotation(this, dataRange, children) ]),

                RuleOutput.Children => new ParseResult(true, dataRange.Length, children),

                RuleOutput.Void => new ParseResult(true, dataRange.Length),

                _ => throw new NotImplementedException($"No implementation to build a rule result for enum value {Output}."),
            };
        }
    }
}
