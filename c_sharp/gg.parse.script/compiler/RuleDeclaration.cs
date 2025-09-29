namespace gg.parse.script.compiler
{
    // xxx rename to RuleHeader and remove RuleBodyAnnotation
    /// <summary>
    /// Name + Production of a rule 
    /// </summary>
    /// <param name="product"></param>
    /// <param name="name"></param>
    public class RuleDeclaration
    {
        public string Name { get; init; }

        public IRule.Output Product { get; init; }
              
        public int Precedence { get; init; }

        /// <summary>
        /// Annotation which describes the rule's body.
        /// </summary>
        public Annotation? RuleBodyAnnotation { get; init; }

        public RuleDeclaration(IRule.Output product, string name, int precedence, Annotation? annotation)
        {
            Name = name;
            Product = product;
            Precedence = precedence;
            RuleBodyAnnotation = annotation;
        }

        public RuleDeclaration(IRule.Output product, string name, Annotation? annotation)
        {
            Name = name;
            Product = product;
            Precedence = 0;
            RuleBodyAnnotation = annotation;
        }
    }
}
