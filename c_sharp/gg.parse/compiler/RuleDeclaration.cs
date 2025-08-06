using gg.parse.rulefunctions;

namespace gg.parse.compiler
{
    public class RuleDeclaration(AnnotationProduct product, string name )
    {
        public AnnotationProduct Product { get; init; } = product;

        public string Name { get; init; } = name;
    }
}
