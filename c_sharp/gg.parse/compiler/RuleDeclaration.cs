namespace gg.parse.compiler
{
    /// <summary>
    /// Name + Production of a rule 
    /// </summary>
    /// <param name="product"></param>
    /// <param name="name"></param>
    public class RuleDeclaration
    {
        public string Name { get; init; }

        public AnnotationProduct Product { get; init; }
              
        public int Precedence { get; init; }

        /// <summary>
        /// Annotation which describes the rule's body.
        /// </summary>
        public Annotation? RuleBodyAnnotation { get; init; }

        public RuleDeclaration(AnnotationProduct product, string name, int precedence, Annotation? annotation)
        {
            Name = name;
            Product = product;
            Precedence = precedence;
            RuleBodyAnnotation = annotation;
        }

        public RuleDeclaration(AnnotationProduct product, string name, Annotation? annotation)
        {
            Name = name;
            Product = product;
            Precedence = 0;
            RuleBodyAnnotation = annotation;
        }
    }
}
