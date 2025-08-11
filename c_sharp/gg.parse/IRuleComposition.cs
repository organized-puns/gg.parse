namespace gg.parse
{
    public interface IRuleComposition<T> where T : IComparable<T>
    {
        IEnumerable<RuleBase<T>> SubRules { get; }

        void ReplaceSubRule(RuleBase<T> subRule, RuleBase<T> replacement);
    }
}
