namespace gg.parse.script.compiler
{
    /// <summary>
    /// Name, output and precedence of a rule 
    /// </summary>
    /// <param name="product"></param>
    /// <param name="name"></param>
    public class RuleHeader
    {
        public string Name { get; init; }

        public RuleOutput Product { get; init; }
              
        public int Precedence { get; init; }

        /// <summary>
        /// Number of actual tokens occupied by this header
        /// </summary>
        public int Length { get; init; }

        public RuleHeader(RuleOutput product, string name, int precedence, int length)
        {
            Name = name;
            Product = product;
            Precedence = precedence;
            Length = length;
        }

        public RuleHeader(RuleOutput product, string name)
            : this(product, name, 0, 0) { }
    }
}
