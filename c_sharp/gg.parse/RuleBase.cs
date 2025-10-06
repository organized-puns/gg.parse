using gg.parse.util;

using Range = gg.parse.util.Range;

namespace gg.parse
{

    public abstract class RuleBase<T>(
        string name, 
        IRule.Output production = IRule.Output.Self, 
        int precedence = 0) 
        : IRule
    {
        public string Name { get; init; } = name;

        public int Id { get; set; } = -1;

        public int Precedence { get; init; } = precedence;

        public IRule.Output Production { get; init; } = production;

        
        public abstract ParseResult Parse(T[] input, int start);

        public override string ToString() => $"{Production} {Name}({Id})";

        public ParseResult BuildDataRuleResult(Range dataRange) 
        {
            return Production switch
            {
                IRule.Output.Self => 
                    new ParseResult(true, dataRange.Length, [new Annotation(this, dataRange)]),

                IRule.Output.Children => 
                    new ParseResult(true, dataRange.Length, [new Annotation(this, dataRange)]),

                IRule.Output.Void => 
                    new ParseResult(true, dataRange.Length),
  
                _ => throw new NotImplementedException($"Production rule {Production} is not implemented"),
            };
        }

        public ParseResult BuildResult(Range dataRange, List<Annotation>? children = null)
        {
            return Production switch
            {
                IRule.Output.Self => new ParseResult(true, dataRange.Length, [ new Annotation(this, dataRange, children) ]),

                IRule.Output.Children => new ParseResult(true, dataRange.Length, children),

                IRule.Output.Void => new ParseResult(true, dataRange.Length),

                _ => throw new NotImplementedException($"Production rule {Production} is not implemented"),
            };
        }
    }
}
