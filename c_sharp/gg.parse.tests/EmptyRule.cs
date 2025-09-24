namespace gg.parse.tests
{
    public class EmptyRule : IRule
    {
        public string Name { get; init; }
        public int Id { get; set; }
        public int Precedence { get; init; }
        public AnnotationProduct Production { get; init; }

        public EmptyRule(int id, string name = "DummyRule", int precedence = 0, AnnotationProduct product = AnnotationProduct.Annotation)
        {
            Id = id;
            Name = name;
            Precedence = precedence;
            Production = product;
        }

        public object Clone() => MemberwiseClone();
    }
}
