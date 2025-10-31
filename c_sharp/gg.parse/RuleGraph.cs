// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections;

using gg.parse.util;

namespace gg.parse
{
    /// <summary>
    /// Graph of Rules. Has one rule which is designated as a Root
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RuleGraph<T> : IEnumerable<RuleBase<T>> where T : IComparable<T>
    {
        private readonly Dictionary<string, RuleBase<T>> _registeredRules = [];
        
        /// <summary>
        /// When calling parsing data using the graph the root is used as a starting point.
        /// </summary>
        public RuleBase<T>? Root { get; set; }

        /// <summary>
        /// Returns an enumerable of all registered rules' names.
        /// </summary>
        public IEnumerable<string> RuleNames => _registeredRules.Keys;

        /// <summary>
        /// Number of registered rules.
        /// </summary>
        public int Count => _registeredRules.Count;

        public TRule RegisterRule<TRule>(TRule rule) where TRule : RuleBase<T>
        {
            Assertions.RequiresNotNull(rule);
            Assertions.RequiresNotNullOrEmpty(rule.Name);
            Assertions.Requires(!_registeredRules.ContainsKey(rule.Name), 
                $"Rule with name '{rule.Name}' already exists in the rule table.");

            _registeredRules[rule.Name] = rule;

            return rule;
        }

        public bool TryFindRule<TRule>(string name, out TRule? result) where TRule : RuleBase<T>
        {
            if (_registeredRules.TryGetValue(name, out var rule) && rule is TRule typedRule)
            {
                result = typedRule;
                return true;
            }

            result = default;
            return false;
        }

        public ParseResult Parse(T[] data, int start = 0)
        {
            Assertions.RequiresNotNull(Root, "Root rule must be set before parsing.");
            return Root!.Parse(data, start);
        }

        public RuleBase<T>? FindRule(string name)
        {
            return _registeredRules.TryGetValue(name, out var rule) ? rule : null;
        }

        public TRule FindOrRegisterRuleAndSubRules<TRule>(TRule rule) where TRule : RuleBase<T>
        {
            var existingRule = FindRule(rule.Name);

            if (existingRule == null)
            {
                if (rule is IRuleComposition<T> ruleComposition)
                {
                    var composition = new RuleBase<T>[ruleComposition.Count];

                    // potentially replace the rules making up the composition with registered versions
                    for (var i = 0; i < ruleComposition.Count; i++)
                    {
                        if (ruleComposition[i] != null)
                        {
                            composition[i] = FindOrRegisterRuleAndSubRules(ruleComposition[i]!);
                        }
                        else
                        {
                            // this can only be the case when the rule is a rule reference
                            // we don't want an explicit dependency on RuleReference here so
                            // check by type name
                            var typeName = ruleComposition.GetType().Name;
                            Assertions.Requires(typeName == "RuleReference`1",
                                "Null rule in composition can only be a RuleReference."
                            );
                        }
                    }

                    return RegisterRule((TRule) ruleComposition.CloneWithComposition(composition));
                }
                else
                {
                    return RegisterRule(rule);
                }
            }

            return (TRule) existingRule;
        }

        public IEnumerator<RuleBase<T>> GetEnumerator()
        {
            return _registeredRules.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _registeredRules.Values.GetEnumerator();
        }       

        public TRule FindOrRegister<TRule>(
            string ruleName,
            AnnotationPruning product,
            Func<string, AnnotationPruning, TRule> factoryMethod)
            where TRule : RuleBase<T> =>

            TryFindRule(ruleName, out TRule? existingRule)
                     ? existingRule!
                     : factoryMethod(ruleName, product);

        public void ReplaceRule(RuleBase<T> original, RuleBase<T> replacement)
        {
            Assertions.RequiresNotNull(original);
            Assertions.RequiresNotNullOrEmpty(original.Name);
            Assertions.Requires(_registeredRules.ContainsKey(original.Name));
            Assertions.RequiresNotNull(replacement);
            Assertions.Requires(original != replacement);
            Assertions.Requires(original.Name == replacement.Name);
            
            _registeredRules[original.Name] = replacement;

            _registeredRules.Values
                // find all compositions that reference the original rule
                .Where(r => r is IRuleComposition<T> composition
                    && composition.Count > 0
                    && composition.Rules!.Any(r => r != null && r == original))
                .Cast<IRuleComposition<T>>()
                // recursively replace the composition that contained the original
                .ForEach(composition => 
                    composition.MutateComposition(composition.Rules!.Replace(original, replacement))
                );
        }
    }
}
