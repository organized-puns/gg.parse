using gg.parse.rulefunctions;
using gg.parse.rules;
using static gg.parse.script.common.CommonRules;

namespace gg.parse.script.common
{
    /// <summary>
    /// Convenience base class to make the rule declarations shorter / easier to read.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommonGraphWrapper<T> : RuleGraph<T> where T : IComparable<T>
    {
        public MatchAnyData<T> any() =>
            this.Any();

        public MatchAnyData<T> any(string ruleName)
        {
            ruleName.TryGetProduct(out var product, out var start, out var length);
            return this.Any(ruleName, product);
        }

        public MatchFunctionCount<T> between(int min, int max, RuleBase<T> rule)
        {
            var ruleName = $"{AnnotationProduct.None.GetPrefix()}between({min},{max},{rule.Name})";
            return between(ruleName, min, max, rule);   
        }

        public MatchFunctionCount<T> between(string ruleName, int min, int max, RuleBase<T> rule)
        {
            ruleName.TryGetProduct(out var product, out var start, out var length);

            var name = ruleName.Substring(start + length);

            return TryFindRule(name, out MatchFunctionCount<T>? existingRule)
                 ? existingRule!
                 : RegisterRule(new MatchFunctionCount<T>(name, rule, product, min, max));
        }

        public LogRule<T> error(string ruleName, string message, RuleBase<T>? condition = null)
        {
            ruleName.TryGetProduct(out var product, out var start, out var length);

            return this.LogError(ruleName.Substring(start + length), product, message, condition);
        }

        public TryMatchFunction<T> ifMatches(RuleBase<T> stopCondition) =>
            this.TryMatch(stopCondition);

        public TryMatchFunction<T> ifMatches(string ruleName, RuleBase<T> condition)
        {
            ruleName.TryGetProduct(out var product, out var start, out var length);

            return this.TryMatch(ruleName.Substring(start + length), product, condition);
        }

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


        public MatchSingleData<T> token(string ruleName, T tokenId)
        {
            ruleName.TryGetProduct(out var product, out var start, out var length);

            var name = ruleName.Substring(start + length);

            return TryFindRule(name, out MatchSingleData<T>? existingRule)
                 ? existingRule!
                 : RegisterRule(new MatchSingleData<T>(name, tokenId, product));
        }


        public LogRule<T> warning(string ruleName, string message, RuleBase<T>? condition = null)
        {
            ruleName.TryGetProduct(out var product, out var start, out var length);

            var name = ruleName.Substring(start + length);

            return TryFindRule(name, out LogRule<T>? existingRule)
                 ? existingRule!
                 : RegisterRule(new LogRule<T>(name, product, message, condition, LogLevel.Warning));
        }

        public MatchFunctionCount<T> zeroOrOne(string ruleName, RuleBase<T> rule)
        {
            ruleName.TryGetProduct(out var product, out var start, out var length);

            return this.ZeroOrOne(ruleName.Substring(start + length), product, rule);
        }

        public MatchFunctionSequence<T> zeroOrOne(RuleBase<T> rule) =>
            this.Sequence(rule);
    }
}
