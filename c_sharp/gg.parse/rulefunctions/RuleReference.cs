
namespace gg.parse.rulefunctions
{
    public class RuleReference<T> : RuleBase<T> where T : IComparable<T>
    {
        public string Reference { get; init; }

        public RuleReference(string name, string reference)
            : base(name) 
        {
            Reference = reference;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            throw new NotImplementedException();
        }
    }

}
