namespace gg.parse.basefunctions
{
    public abstract class ParseFunctionBase<T>(string name, int id, ProductionEnum action = ProductionEnum.ProduceItem)
        where T : IComparable<T>
    {
        public string Name { get; init; } = name;

        public int Id { get; set; } = id;

        public ProductionEnum ActionOnMatch { get; init; } = action;

        public abstract AnnotationBase? Parse(T[] input, int start);

        public override string ToString()
        {
            return Name;
        }
    }
}
