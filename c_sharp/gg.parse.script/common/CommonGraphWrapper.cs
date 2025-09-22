using gg.parse.rules;
using System.Data;
using System.Xml.Linq;

namespace gg.parse.script.common
{
    /// <summary>
    /// Convenience base class to make the rule declarations shorter / easier to read.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommonGraphWrapper<T> : RuleGraph<T> where T : IComparable<T>
    {
        // -- Utility methods -----------------------------------------------------------------------------------------
        
        public static (string name, AnnotationProduct product) CreateRuleNameAndProduct(string? name, string fallback) =>
            string.IsNullOrEmpty(name)
                ? ($"{AnnotationProduct.None.GetPrefix()}{fallback}", AnnotationProduct.None)
                : name.SplitNameAndProduct();

        private static string JoinDataArray(T[] array) =>
            array is char[] charArray
                ? new string(charArray)
                : string.Join(", ", array);

        public TRule FindOrRegister<TRule>(
            string? name, string fallback,
            Func<string, AnnotationProduct, TRule> factoryMethod)
            where TRule : RuleBase<T>
        {
            var (ruleName, product) = CreateRuleNameAndProduct(name, fallback);
            return TryFindRule(ruleName, out TRule? existingRule)
                     ? existingRule!
                     : factoryMethod(ruleName, product);
        }


        // -- Rule short hands ----------------------------------------------------------------------------------------

        public MatchAnyData<T> Any() =>
            Any(null);

        public MatchAnyData<T> Any(string? name) =>
            FindOrRegister(name, $"{CommonTokenNames.AnyCharacter}",
                        (ruleName, product) => RegisterRule(
                            new MatchAnyData<T>(ruleName, product)));
        
        public LogRule<T> error(string ruleName, string message, RuleBase<T>? condition = null)
        {
            ruleName.TryGetProduct(out var product, out var start, out var length);

            return this.LogError(ruleName.Substring(start + length), product, message, condition);
        }
       

        public TryMatchRule<T> ifMatches(RuleBase<T> stopCondition) =>
            this.TryMatch(stopCondition);

        public TryMatchRule<T> ifMatches(string ruleName, RuleBase<T> condition)
        {
            ruleName.TryGetProduct(out var product, out var start, out var length);

            return this.TryMatch(ruleName.Substring(start + length), product, condition);
        }

        public MatchDataSequence<T> Literal(T[] sequence) =>
            Literal(null, sequence);

        public MatchDataSequence<T> Literal(string? name, T[] sequence) =>
            FindOrRegister(name, $"{CommonTokenNames.Literal}({JoinDataArray(sequence)})",
                        (ruleName, product) => RegisterRule(
                            new MatchDataSequence<T>(ruleName, sequence, product)));

        public MatchNotFunction<T> Not(string? name, RuleBase<T> rule) =>
            FindOrRegister(name, $"{CommonTokenNames.Literal}({rule.Name})",
                        (ruleName, product) => RegisterRule(
                            new MatchNotFunction<T>(ruleName, product, rule)));

        public MatchNotFunction<T> Not(RuleBase<T> rule) =>
            Not(null, rule);

        public MatchOneOfFunction<T> OneOf(string ruleName, params RuleBase<T>[] rules)
        {
            ruleName.TryGetProduct(out var product, out var start, out var length);

            return this.OneOf(ruleName.Substring(start + length), product, rules);
        }

        public MatchOneOfFunction<T> oneOf(params RuleBase<T>[] rules) =>
            this.OneOf(rules);

        public MatchFunctionSequence<T> Sequence(string ruleName, params RuleBase<T>[] rules)
        {
            ruleName.TryGetProduct(out var product, out var start, out var length);

            return this.Sequence(ruleName.Substring(start + length), product, rules);
        }

        public MatchFunctionSequence<T> sequence(params RuleBase<T>[] rules) =>
            this.Sequence(rules);

        public MatchSingleData<T> MatchSingle(T data) =>
            MatchSingle(null, data);

        public MatchSingleData<T> MatchSingle(string? name, T data) =>
            FindOrRegister(name, $"{CommonTokenNames.SingleData}({data.ToString()})",
                        (ruleName, product) => RegisterRule(
                            new MatchSingleData<T>(ruleName, data, product)));

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

        /*public MatchFunctionCount<T> zeroOrOne(string ruleName, RuleBase<T> rule)
        {
            ruleName.TryGetProduct(out var product, out var start, out var length);

            return this.ZeroOrOne(ruleName.Substring(start + length), product, rule);
        }*/

        public MatchFunctionCount<T> ZeroOrOne(string? name, RuleBase<T> rule) =>
            FindOrRegister(name,
                $"{CommonTokenNames.ZeroOrOne}({rule.Name})",
                (ruleName, product) => RegisterRule(
                    new MatchFunctionCount<T>(ruleName, rule, product, 0, 1)
                )
            );

        public MatchFunctionCount<T> ZeroOrOne(RuleBase<T> rule) =>
            ZeroOrOne(null, rule);

        public MatchFunctionCount<T> ZeroOrMore(string? name, RuleBase<T> rule) =>
            FindOrRegister(name,
                $"{CommonTokenNames.ZeroOrMore}({rule.Name})",
                (ruleName, product) => RegisterRule(
                    new MatchFunctionCount<T>(ruleName, rule, product, 0, 0)
                )
            );

        public MatchFunctionCount<T> ZeroOrMore(RuleBase<T> rule) =>
            ZeroOrMore(null, rule);
    }
}
