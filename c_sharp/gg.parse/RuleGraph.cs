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
        private int _nextRuleId = 0;

        private readonly Dictionary<string, RuleBase<T>> _nameRuleLookup = [];
        
        /// <summary>
        /// When calling parsing data using the graph the root is used as a starting point.
        /// </summary>
        public RuleBase<T>? Root { get; set; }

        /// <summary>
        /// Returns an enumerable of all registered rules' names.
        /// </summary>
        public IEnumerable<string> RuleNames => _nameRuleLookup.Keys;

        /// <summary>
        /// Number of registered rules.
        /// </summary>
        public int Count => _nameRuleLookup.Count;


        public TRule RegisterRule<TRule>(TRule rule) where TRule : RuleBase<T>
        {
            Assertions.RequiresNotNull(rule);
            Assertions.RequiresNotNullOrEmpty(rule.Name);
            Assertions.Requires(!_nameRuleLookup.ContainsKey(rule.Name), 
                $"Rule with name '{rule.Name}' already exists in the rule table.");

            _nameRuleLookup[rule.Name] = rule;
            rule.Id = _nextRuleId++;

            return rule;
        }

        public bool TryFindRule<TRule>(string name, out TRule? result) where TRule : RuleBase<T>
        {
            if (_nameRuleLookup.TryGetValue(name, out var rule) && rule is TRule typedRule)
            {
                result = typedRule;
                return true;
            }

            result = default;
            return false;
        }

        public RuleBase<T>? FindRule(string name)
        {
            return _nameRuleLookup.TryGetValue(name, out var rule) ? rule : null;
        }


        public TRule RegisterRuleAndSubRules<TRule>(TRule rule) where TRule : RuleBase<T>
        {
            var existingRule = FindRule(rule.Name);

            if (existingRule == null)
            {
                RegisterRule(rule);

                if (rule is IRuleComposition<T> ruleComposition)
                {
                    for (var i = 0; i < ruleComposition.Count; i++)
                    {
                        ruleComposition[i] = RegisterRuleAndSubRules(ruleComposition[i]);
                    }
                }

                return rule;
            }

            return (TRule) existingRule;
        }

        public IEnumerator<RuleBase<T>> GetEnumerator()
        {
            return _nameRuleLookup.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _nameRuleLookup.Values.GetEnumerator();
        }       

        public TRule FindOrRegister<TRule>(
            string ruleName,
            RuleOutput product,
            Func<string, RuleOutput, TRule> factoryMethod)
            where TRule : RuleBase<T> =>

            TryFindRule(ruleName, out TRule? existingRule)
                     ? existingRule!
                     : factoryMethod(ruleName, product);

    }
}
