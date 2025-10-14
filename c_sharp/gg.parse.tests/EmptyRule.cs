namespace gg.parse.tests
{
    public class EmptyRule : IRule
    {
        public string Name { get; init; }
        public int Id { get; set; }
        public int Precedence { get; init; }
        public RuleOutput Output { get; init; }

        public EmptyRule(int id, string name = "DummyRule", int precedence = 0, RuleOutput product = RuleOutput.Self)
        {
            Id = id;
            Name = name;
            Precedence = precedence;
            Output = product;
        }

        public object Clone() => MemberwiseClone();
    }
}
