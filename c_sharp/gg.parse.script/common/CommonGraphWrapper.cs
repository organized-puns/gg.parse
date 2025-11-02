// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.rules;
using gg.parse.script.parser;

namespace gg.parse.script.common
{
    /// <summary>
    /// Convenience base class to make the rule declarations shorter / easier to read.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommonGraphWrapper<T> : RuleGraph<T> where T : IComparable<T>
    {
        // -- Utility methods -----------------------------------------------------------------------------------------

        public static (string name, AnnotationPruning pruning) CreateRuleNameAndpruning(string? name, string fallback) =>
            string.IsNullOrEmpty(name)
                ? ($"{AnnotationPruning.All.GetTokenString()}{fallback}", AnnotationPruning.All)
                : name.SplitNameAndPruning();

        private static string JoinDataArray(T[] array) =>
            array is char[] charArray
                ? new string(charArray)
                : string.Join(", ", array);

        public TRule FindOrRegister<TRule>(
            string? name, string fallback,
            Func<string, AnnotationPruning, TRule> factoryMethod)
            where TRule : RuleBase<T>
        {
            var (ruleName, product) = CreateRuleNameAndpruning(name, fallback);
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
                        (ruleName, pruning) => RegisterRule(
                            new LogRule<T>(ruleName, pruning, condition, message, LogLevel.Error)));

        public MatchDataSet<T> InSet(params T[] set) =>
            InSet(null, set);   

        public MatchDataSet<T> InSet(string? name, params T[] set) =>
            FindOrRegister(name, $"{CommonTokenNames.Set}({JoinDataArray(set)})",
                        (ruleName, product) => RegisterRule(
                            new MatchDataSet<T>(ruleName, product, set)));

        public MatchDataRange<T> InRange(T from, T to) =>
            InRange(null, from, to);

        public MatchDataRange<T> InRange(string? name, T from, T to) =>
            FindOrRegister(name, $"{CommonTokenNames.DataRange}({from}..{to})",
                        (ruleName, pruning) => RegisterRule(
                            new MatchDataRange<T>(ruleName, from, to, pruning)));

        public MatchCondition<T> IfMatch(RuleBase<T> condition) =>
            IfMatch(null, condition);

        public MatchCondition<T> IfMatch(string? name, RuleBase<T> condition) =>
            FindOrRegister(name, $"{CommonTokenNames.If}({condition.Name})",
                        (ruleName, pruning) => RegisterRule(
                            new MatchCondition<T>(ruleName, pruning, 0, condition)));
        
        public MatchDataSequence<T> Literal(T[] sequence) =>
            Literal(null, sequence);

        public MatchDataSequence<T> Literal(string? name, T[] sequence) =>
            FindOrRegister(name, $"{CommonTokenNames.Literal}({JoinDataArray(sequence)})",
                        (ruleName, pruning) => RegisterRule(
                            new MatchDataSequence<T>(ruleName, sequence, pruning)));

        public MatchNot<T> Not(string? name, RuleBase<T> rule) =>
            FindOrRegister(name, $"{CommonTokenNames.Literal}({rule.Name})",
                        (ruleName, pruning) => RegisterRule(
                            new MatchNot<T>(ruleName, pruning, 0, rule)));

        public MatchNot<T> Not(RuleBase<T> rule) =>
            Not(null, rule);

        public MatchOneOf<T> OneOf(string? name, params RuleBase<T>[] rules) =>
            FindOrRegister(name, $"{CommonTokenNames.OneOf}({string.Join(", ", rules.Select(r => r.Name))})",
                        (ruleName, pruning) => RegisterRule(
                            new MatchOneOf<T>(ruleName, pruning, 0, rules)));

        public MatchOneOf<T> OneOf(params RuleBase<T>[] rules) =>
            OneOf(null, rules);

        public MatchCount<T> OneOrMore(string? name, RuleBase<T> rule) =>
            FindOrRegister(name,
                $"{CommonTokenNames.OneOrMore}({rule.Name})",
                (ruleName, pruning) => RegisterRule(
                    new MatchCount<T>(ruleName, pruning, 0, rule, 1, 0)
                )
            );

        public MatchCount<T> OneOrMore(RuleBase<T> rule) =>
            ZeroOrMore(null, rule);

        public MatchRuleSequence<T> Sequence(string? name, params RuleBase<T>[] rules) =>
            FindOrRegister(name, $"{CommonTokenNames.FunctionSequence}({string.Join(", ", rules.Select(r => r.Name))})",
                        (ruleName, pruning) => RegisterRule(
                            new MatchRuleSequence<T>(ruleName, pruning, 0, rules)));

        public MatchRuleSequence<T> Sequence(params RuleBase<T>[] rules) =>
            Sequence(null, rules);

        public MatchSingleData<T> MatchSingle(T data) =>
            MatchSingle(null, data);

        public MatchSingleData<T> MatchSingle(string? name, T data) =>
            FindOrRegister(name, $"{CommonTokenNames.SingleData}({data})",
                        (ruleName, pruning) => RegisterRule(new MatchSingleData<T>(ruleName, data, pruning)));

        public SkipRule<T> Skip(string? name, RuleBase<T> stopCondition, bool failOnEoF = true) =>
            FindOrRegister(name, $"{CommonTokenNames.StopAt}({stopCondition}, {failOnEoF})",
                        (ruleName, pruning) => RegisterRule(
                            new SkipRule<T>(ruleName, pruning, 0, stopCondition, failOnEoF)));

        public SkipRule<T> Skip(RuleBase<T> stopCondition, bool failOnEoF = true) =>
            Skip(null, stopCondition, failOnEoF);

        public LogRule<T> Warning(string name, string message, RuleBase<T>? condition = null) =>
            FindOrRegister(name, $"{CommonTokenNames.LogError}({name})",
                        (ruleName, pruning) => RegisterRule(
                            new LogRule<T>(ruleName, pruning, condition, message, LogLevel.Warning)));

        public MatchCount<T> ZeroOrOne(string? name, RuleBase<T> rule) =>
            FindOrRegister(name,
                $"{CommonTokenNames.ZeroOrOne}({rule.Name})",
                (ruleName, pruning) => RegisterRule(
                    new MatchCount<T>(ruleName, pruning, 0, rule, 0, 1)
                )
            );

        public MatchCount<T> ZeroOrOne(RuleBase<T> rule) =>
            ZeroOrOne(null, rule);

        public MatchCount<T> ZeroOrMore(string? name, RuleBase<T> rule) =>
            FindOrRegister(name,
                $"{CommonTokenNames.ZeroOrMore}({rule.Name})",
                (ruleName, pruning) => RegisterRule(
                    new MatchCount<T>(ruleName, pruning, 0, rule, 0, 0)
                )
            );

        public MatchCount<T> ZeroOrMore(RuleBase<T> rule) =>
            ZeroOrMore(null, rule);
    }
}
