using gg.parse.rulefunctions;
using gg.parse.rulefunctions.rulefunctions;

using static gg.parse.rulefunctions.CommonRules;

namespace gg.parse.rules
{
    /// <summary>
    /// Convenience base class to make the rule declarations shorter / easier to read.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommonGraphWrapper<T> : RuleGraph<T> where T : IComparable<T>
    {
        public MatchNotFunction<T> not(string ruleName, RuleBase<T> rule)
        {
            ruleName.TryGetProduct(out var product, out var start, out var length);

            return this.Not(ruleName.Substring(start + length), product, rule);
        }

        public MatchNotFunction<T> not(RuleBase<T> rule) =>
            this.Not(rule);

        public MatchOneOfFunction<T> oneOf(string ruleName, params RuleBase<T>[] rules)
        {
            ruleName.TryGetProduct(out var product, out var start, out var length);

            return this.OneOf(ruleName.Substring(start + length), product, rules);
        }

        public MatchOneOfFunction<T> oneOf(params RuleBase<T>[] rules) =>
            this.OneOf(rules);

        public MatchFunctionSequence<T> sequence(string ruleName, params RuleBase<T>[] rules)
        {
            ruleName.TryGetProduct(out var product, out var start, out var length);

            return this.Sequence(ruleName.Substring(start + length), product, rules);
        }

        public MatchFunctionSequence<T> sequence(params RuleBase<T>[] rules) =>
            this.Sequence(rules);

        public SkipRule<T> skip(string ruleName, RuleBase<T> stopCondition, bool failOnEoF = true)
        {
            ruleName.TryGetProduct(out var product, out var start, out var length);

            return this.Skip(ruleName.Substring(start + length), product, stopCondition, failOnEoF);
        }

        public SkipRule<T> skip(RuleBase<T> stopCondition, bool failOnEoF = true) =>
            this.Skip(stopCondition, failOnEoF);
    }
}
