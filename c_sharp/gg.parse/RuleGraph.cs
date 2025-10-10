using gg.parse.util;
using System.Collections;

namespace gg.parse
{
    public class RuleGraph<T> : IEnumerable<RuleBase<T>> where T : IComparable<T>
    {
        private int _nextRuleId = 0;

        private readonly Dictionary<string, RuleBase<T>> _nameRuleLookup = [];
        private readonly Dictionary<int, RuleBase<T>> _idRuleLookup = [];

        public RuleBase<T>? Root { get; set; }

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
            Assertions.Requires(!_idRuleLookup.ContainsKey(rule.Id),
                $"Rule with id '{rule.Id}' already exists in the rule table.");

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
            if (FindRule(rule.Name) == null)
            {
                rule = RegisterRule(rule);

                if (rule is IRuleComposition<T> ruleFunction)
                {
                    foreach (var subRule in ruleFunction.Rules)
                    {
                        RegisterRuleAndSubRules(subRule);
                    }
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
