


namespace gg.parse.rules
{
    /// <summary>
    /// Rule used in compilation where the rule body is empty. Can be (out) optimized later
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NopRule<T> : RuleBase<T> where T: IComparable<T>
    {

        public NopRule(string name) 
            : base(name, AnnotationProduct.None)
        {
        }

        public override ParseResult Parse(T[] input, int start) => ParseResult.Success;
    }
}
