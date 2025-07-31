using gg.core.util;
using System.Collections;

namespace gg.parse.rulefunctions
{
    public class RuleTable<T> : IEnumerable<RuleBase<T>> where T : IComparable<T>
    {
        private int _nextRuleId = 0;

        private readonly Dictionary<string, RuleBase<T>> _nameRuleLookup = new();
        private readonly Dictionary<int, RuleBase<T>> _idRuleLookup = new();


        public TRule RegisterRule<TRule>(TRule rule) where TRule : RuleBase<T>
        {
            Contract.RequiresNotNull(rule);
            Contract.RequiresNotNullOrEmpty(rule.Name);
            Contract.Requires(!_nameRuleLookup.ContainsKey(rule.Name), 
                $"Rule with name '{rule.Name}' already exists in the rule table.");
            Contract.Requires(!_idRuleLookup.ContainsKey(rule.Id),
                $"Rule with id '{rule.Id}' already exists in the rule table.");

            _nameRuleLookup[rule.Name] = rule;
            rule.Id = _nextRuleId++;
            _idRuleLookup[rule.Id] = rule;  

            return rule;
        }

        public TRule GetOrRegisterRule<TRule>(TRule rule) where TRule : RuleBase<T>
        {
            Contract.RequiresNotNull(rule);
            Contract.RequiresNotNullOrEmpty(rule.Name);
            
            if (_nameRuleLookup.TryGetValue(rule.Name, out var existingRule))
            {
                Contract.Requires(existingRule.GetType() == typeof(TRule),
                    $"Rule with name '{rule.Name}' already exists in the rule table with a different type.");   
                return (TRule) existingRule;
            }
            
            Contract.Requires(!_idRuleLookup.ContainsKey(rule.Id),
                $"Rule with id '{rule.Id}' already exists in the rule table but is registered under a different name.");

            _nameRuleLookup[rule.Name] = rule;
            rule.Id = _nextRuleId++;
            _idRuleLookup[rule.Id] = rule;

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

        public RuleBase<T>? FindRule(int id)
        {
            return _idRuleLookup.TryGetValue(id, out var rule) ? rule : null;
        }

        public TRule RegisterRuleAndSubRules<TRule>(TRule rule) where TRule : RuleBase<T>
        {
            rule = GetOrRegisterRule(rule);

            if (rule is IRuleComposition<T> ruleFunction)
            {
                foreach (var subRule in ruleFunction.SubRules)
                {
                    RegisterRuleAndSubRules(subRule);
                }
            }

            return rule;
        }

        public IEnumerator<RuleBase<T>> GetEnumerator()
        {
            return _idRuleLookup.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _idRuleLookup.Values.GetEnumerator();
        }
    }
}
