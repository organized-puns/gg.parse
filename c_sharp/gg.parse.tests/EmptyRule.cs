namespace gg.parse.tests
{
    public class EmptyRule : IRule
    {
        public string Name { get; init; }
        public int Id { get; set; }
        public int Precedence { get; init; }
        public AnnotationPruning Prune { get; init; }

        public EmptyRule(int id, string name = "DummyRule", int precedence = 0, AnnotationPruning product = AnnotationPruning.None)
        {
            Id = id;
            Name = name;
            Precedence = precedence;
            Prune = product;
        }

        public object Clone() => MemberwiseClone();
    }
}
