namespace gg.parse.rules
{
    public abstract class RuleBase : IRule
    {
        public string Name { get; set; }

        public RuleBase(string name) => Name = name;

        public abstract ParseResult Parse(string text, int offset);
    }
}
