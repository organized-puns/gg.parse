namespace gg.parse
{
    public interface IRule<T> where T : IComparable<T>
    {
        string Name { get; init; }

        int Id { get; set; }

        int Precedence { get; init; }

        AnnotationProduct Production { get; init; }

        public abstract ParseResult Parse(T[] input, int start);
    }
}
