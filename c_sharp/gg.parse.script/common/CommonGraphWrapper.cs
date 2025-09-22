using gg.parse.rules;
using System.Linq;
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
        
        public LogRule<T> Error(string name, string message, RuleBase<T>? condition = null) =>
            FindOrRegister(name, $"{CommonTokenNames.LogError}({name})",
                        (ruleName, product) => RegisterRule(
                            new LogRule<T>(ruleName, product, message, condition, LogLevel.Error)));

        public MatchDataSet<T> InSet(params T[] set) =>
            InSet(null, set);   

        public MatchDataSet<T> InSet(string? name, params T[] set) =>
            FindOrRegister(name, $"{CommonTokenNames.Set}({JoinDataArray(set)})",
                        (ruleName, product) => RegisterRule(
                            new MatchDataSet<T>(ruleName, product, set)));

        public TryMatchRule<T> TryMatch(RuleBase<T> condition) =>
            TryMatch(null, condition);

        public TryMatchRule<T> TryMatch(string? name, RuleBase<T> condition) =>
            FindOrRegister(name, $"{CommonTokenNames.TryMatchOperator}({condition.Name})",
                        (ruleName, product) => RegisterRule(
                            new TryMatchRule<T>(ruleName, product, condition)));
        
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

        public MatchOneOfFunction<T> OneOf(string? name, params RuleBase<T>[] rules) =>
            FindOrRegister(name, $"{CommonTokenNames.OneOf}({string.Join(", ", rules.Select(r => r.Name))})",
                        (ruleName, product) => RegisterRule(
                            new MatchOneOfFunction<T>(ruleName, product, 0, rules)));

        public MatchOneOfFunction<T> OneOf(params RuleBase<T>[] rules) =>
            OneOf(null, rules);

        public MatchFunctionSequence<T> Sequence(string? name, params RuleBase<T>[] rules) =>
            FindOrRegister(name, $"{CommonTokenNames.FunctionSequence}({string.Join(", ", rules.Select(r => r.Name))})",
                        (ruleName, product) => RegisterRule(
                            new MatchFunctionSequence<T>(ruleName, product, 0, rules)));

        public MatchFunctionSequence<T> Sequence(params RuleBase<T>[] rules) =>
            Sequence(null, rules);

        public MatchSingleData<T> MatchSingle(T data) =>
            MatchSingle(null, data);

        public MatchSingleData<T> MatchSingle(string? name, T data) =>
            FindOrRegister(name, $"{CommonTokenNames.SingleData}({data.ToString()})",
                        (ruleName, product) => RegisterRule(
                            new MatchSingleData<T>(ruleName, data, product)));

        public SkipRule<T> Skip(string? name, RuleBase<T> stopCondition, bool failOnEoF = true) =>
            FindOrRegister(name, $"{CommonTokenNames.Skip}({stopCondition.ToString()}, {failOnEoF})",
                        (ruleName, product) => RegisterRule(
                            new SkipRule<T>(ruleName, product, stopCondition, failOnEoF)));

        public SkipRule<T> Skip(RuleBase<T> stopCondition, bool failOnEoF = true) =>
            Skip(null, stopCondition, failOnEoF);

        public LogRule<T> Warning(string name, string message, RuleBase<T>? condition = null) =>
            FindOrRegister(name, $"{CommonTokenNames.LogError}({name})",
                        (ruleName, product) => RegisterRule(
                            new LogRule<T>(ruleName, product, message, condition, LogLevel.Warning)));

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
