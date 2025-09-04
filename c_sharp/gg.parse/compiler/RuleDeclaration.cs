namespace gg.parse.compiler
{
    /// <summary>
    /// Name + Production of a rule 
    /// </summary>
    /// <param name="product"></param>
    /// <param name="name"></param>
    public class RuleDeclaration(Annotation annotation, AnnotationProduct product, string name, int precedence = 0 )
    {
        /// <summary>
        /// Annotation which describes this declaration.
        /// </summary>
        public Annotation AssociatedAnnotation { get; init; } = annotation;

        public string Name { get; init; } = name;

        public AnnotationProduct Product { get; init; } = product;
              

        public int Precedence { get; init; } = precedence;
    }
}
