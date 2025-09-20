namespace gg.parse
{
    public interface IRule 
    {
        string Name { get; init; }

        int Id { get; set; }

        int Precedence { get; init; }

        AnnotationProduct Production { get; init; }
    }
}
