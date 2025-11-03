// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections;
using gg.parse.util;

namespace gg.parse.core
{
    /// <summary>
    /// Graph of Rules. Has one rule which is designated as a Root
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RuleGraph<T> : IEnumerable<IRule> 
    {
        private readonly Dictionary<string, IRule> _registeredRules = [];
        
        /// <summary>
        /// When calling parsing data using the graph the root is used as a starting point.
        /// </summary>
        public IRule? Root { get; set; }

        /// <summary>
        /// Returns an enumerable of all registered rules' names.
        /// </summary>
        public IEnumerable<string> RuleNames => _registeredRules.Keys;

        /// <summary>
        /// Number of registered rules.
        /// </summary>
        public int Count => _registeredRules.Count;

        /// <summary>
        /// Find a rule by name. Rule must exist in the graph.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IRule this[string name] => _registeredRules[name];

        public TRule RegisterRule<TRule>(TRule rule) where TRule : IRule
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

        public IRule? FindRule(string name)
        {
            return _registeredRules.TryGetValue(name, out var rule) ? rule : null;
        }

        public TRule FindOrRegisterRuleAndSubRules<TRule>(TRule rule) where TRule : IRule
        {
            var existingRule = FindRule(rule.Name);

            if (existingRule == null)
            {
                if (rule is IRuleComposition ruleComposition)
                {
                    return (TRule) FindOrRegisterRuleCompositionAndSubRules(ruleComposition);
                }
                else if (rule is IMetaRule metaRule)
                {
                    return (TRule)FindOrRegisterMetaRuleAndSubject(metaRule);
                }
                else
                {
                    return RegisterRule(rule);
                }
            }

            return (TRule) existingRule;
        }

        private IRuleComposition FindOrRegisterRuleCompositionAndSubRules(IRuleComposition ruleComposition) 
        {
            var composition = new IRule[ruleComposition.Count];
            var isChanged = false;

            // potentially replace the rules making up the composition with registered versions
            for (var i = 0; i < ruleComposition.Count; i++)
            {
                if (ruleComposition[i] != null)
                {
                    var registeredSubrule = FindOrRegisterRuleAndSubRules(ruleComposition[i]!);

                    if (registeredSubrule != ruleComposition[i])
                    {
                        isChanged = true;
                    }   

                    composition[i] = registeredSubrule;
                }
            }

            return RegisterRule(isChanged ? ruleComposition.CloneWithComposition(composition) : ruleComposition);
        }

        private IMetaRule FindOrRegisterMetaRuleAndSubject(IMetaRule metaRule)
        {
            // make the assumptions explicit
            Assertions.Requires(!_registeredRules.ContainsKey(metaRule.Name));

            if (metaRule.Subject != null)
            {
                var registeredSubject = FindOrRegisterRuleAndSubRules(metaRule.Subject);

                // if a different object comes back we need to register a new meta rule
                // clone since this subject was already registered unlike this meta rule 
                if (registeredSubject != metaRule.Subject)
                {
                    return RegisterRule(metaRule.CloneWithSubject(registeredSubject));
                }
            }

            return RegisterRule(metaRule);
        }

        public IEnumerator<IRule> GetEnumerator()
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

        public TRule ReplaceRule<TRule>(IRule original, TRule replacement) where TRule : IRule
        {
            Assertions.RequiresNotNull(original);
            Assertions.RequiresNotNullOrEmpty(original.Name);
            Assertions.Requires(_registeredRules.ContainsKey(original.Name));
            Assertions.RequiresNotNull(replacement);
            Assertions.Requires(!ReferenceEquals(original, replacement));
            Assertions.Requires(original.Name == replacement.Name);

            // in order to replace rules which contains self-references we need to first
            // replace the rule in the registry so the rule itself will be considered
            // when replacing the original
            _registeredRules[original.Name] = replacement;

            // replace all references to the original rule in compositions
            _registeredRules.Values
                // find all compositions that reference the original rule
                .Where(r => r is IRuleComposition composition
                    && composition.Count > 0
                    && composition.Rules!.Any(r => r != null && r == original))
                .Cast<IRuleComposition>()
                // recursively replace the composition that contained the original
                .ForEach(composition => 
                    composition.MutateComposition(composition.Rules!.Replace(original, replacement))
                );

            // replace all references to the original rule in meta rules
            _registeredRules.Values
                // find all meta rukes that reference the original rule
                .Where(r => r is IMetaRule metaRule && metaRule.Subject == original)
                .Cast<IMetaRule>()
                // recursively replace the composition that contained the original
                .ForEach(composition => composition.MutateSubject(replacement));

            if (Root == original)
            {
                Root = replacement;
            }

            return (TRule) replacement;
        }
    }
}
