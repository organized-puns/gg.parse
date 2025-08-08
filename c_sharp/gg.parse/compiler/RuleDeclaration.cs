using gg.parse.rulefunctions;

namespace gg.parse.compiler
{
    /// <summary>
    /// Name + Production of a rule 
    /// </summary>
    /// <param name="product"></param>
    /// <param name="name"></param>
    public class RuleDeclaration(AnnotationProduct product, string name )
    {
        public AnnotationProduct Product { get; init; } = product;

        public string Name { get; init; } = name;
    }
}
