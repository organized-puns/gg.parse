using gg.parse.rules;

namespace gg.parse.script.common
{
    /// <summary>
    /// Convenience base class to make the rule declarations shorter / easier to read.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommonGraphWrapper<T> : RuleGraph<T> where T : IComparable<T>
    {
        // -- Utility methods -----------------------------------------------------------------------------------------

        public static (string name, IRule.Output product) CreateRuleNameAndProduct(string? name, string fallback) =>
            string.IsNullOrEmpty(name)
                ? ($"{IRule.Output.Void.GetToken()}{fallback}", IRule.Output.Void)
                : name.SplitNameAndOutput();

        private static string JoinDataArray(T[] array) =>
            array is char[] charArray
                ? new string(charArray)
                : string.Join(", ", array);

        public TRule FindOrRegister<TRule>(
            string? name, string fallback,
            Func<string, IRule.Output, TRule> factoryMethod)
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

        public CallbackRule<T> Callback(
            RuleBase<T> rule,
            RuleCallbackAction<T> callback,
            CallbackRule<T>.CallbackCondition condition = CallbackRule<T>.CallbackCondition.Success) =>
            Callback(null, rule, callback, condition);

        public CallbackRule<T> Callback(
            string? name, 
            RuleBase<T> rule,
            RuleCallbackAction<T> callback,
            CallbackRule<T>.CallbackCondition condition = CallbackRule<T>.CallbackCondition.Success) =>
            FindOrRegister(name, $"{CommonTokenNames.Callback}({rule.Name})",
                        (ruleName, product) => RegisterRule(
                            new CallbackRule<T>(ruleName, rule, callback, condition)));

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

        public MatchCondition<T> IfMatch(RuleBase<T> condition) =>
            IfMatch(null, condition);

        public MatchCondition<T> IfMatch(string? name, RuleBase<T> condition) =>
            FindOrRegister(name, $"{CommonTokenNames.If}({condition.Name})",
                        (ruleName, product) => RegisterRule(
                            new MatchCondition<T>(ruleName, product, 0, condition)));
        
        public MatchDataSequence<T> Literal(T[] sequence) =>
            Literal(null, sequence);

        public MatchDataSequence<T> Literal(string? name, T[] sequence) =>
            FindOrRegister(name, $"{CommonTokenNames.Literal}({JoinDataArray(sequence)})",
                        (ruleName, product) => RegisterRule(
                            new MatchDataSequence<T>(ruleName, sequence, product)));

        public MatchNot<T> Not(string? name, RuleBase<T> rule) =>
            FindOrRegister(name, $"{CommonTokenNames.Literal}({rule.Name})",
                        (ruleName, product) => RegisterRule(
                            new MatchNot<T>(ruleName, product, 0, rule)));

        public MatchNot<T> Not(RuleBase<T> rule) =>
            Not(null, rule);

        public MatchOneOf<T> OneOf(string? name, params RuleBase<T>[] rules) =>
            FindOrRegister(name, $"{CommonTokenNames.OneOf}({string.Join(", ", rules.Select(r => r.Name))})",
                        (ruleName, product) => RegisterRule(
                            new MatchOneOf<T>(ruleName, product, 0, rules)));

        public MatchOneOf<T> OneOf(params RuleBase<T>[] rules) =>
            OneOf(null, rules);


        public MatchCount<T> OneOrMore(string? name, RuleBase<T> rule) =>
            FindOrRegister(name,
                $"{CommonTokenNames.OneOrMore}({rule.Name})",
                (ruleName, product) => RegisterRule(
                    new MatchCount<T>(ruleName, rule, product, 1, 0)
                )
            );

        public MatchCount<T> OneOrMore(RuleBase<T> rule) =>
            ZeroOrMore(null, rule);

        public MatchRuleSequence<T> Sequence(string? name, params RuleBase<T>[] rules) =>
            FindOrRegister(name, $"{CommonTokenNames.FunctionSequence}({string.Join(", ", rules.Select(r => r.Name))})",
                        (ruleName, product) => RegisterRule(
                            new MatchRuleSequence<T>(ruleName, product, 0, rules)));

        public MatchRuleSequence<T> Sequence(params RuleBase<T>[] rules) =>
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
                            new SkipRule<T>(ruleName, product, 0, stopCondition, failOnEoF)));

        public SkipRule<T> Skip(RuleBase<T> stopCondition, bool failOnEoF = true) =>
            Skip(null, stopCondition, failOnEoF);

        public LogRule<T> Warning(string name, string message, RuleBase<T>? condition = null) =>
            FindOrRegister(name, $"{CommonTokenNames.LogError}({name})",
                        (ruleName, product) => RegisterRule(
                            new LogRule<T>(ruleName, product, message, condition, LogLevel.Warning)));

        public MatchCount<T> ZeroOrOne(string? name, RuleBase<T> rule) =>
            FindOrRegister(name,
                $"{CommonTokenNames.ZeroOrOne}({rule.Name})",
                (ruleName, product) => RegisterRule(
                    new MatchCount<T>(ruleName, rule, product, 0, 1)
                )
            );

        public MatchCount<T> ZeroOrOne(RuleBase<T> rule) =>
            ZeroOrOne(null, rule);

        public MatchCount<T> ZeroOrMore(string? name, RuleBase<T> rule) =>
            FindOrRegister(name,
                $"{CommonTokenNames.ZeroOrMore}({rule.Name})",
                (ruleName, product) => RegisterRule(
                    new MatchCount<T>(ruleName, rule, product, 0, 0)
                )
            );

        public MatchCount<T> ZeroOrMore(RuleBase<T> rule) =>
            ZeroOrMore(null, rule);
    }
}
