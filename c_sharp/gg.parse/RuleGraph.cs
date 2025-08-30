using System.Collections;
using System.Data;

using gg.core.util;
using gg.parse.rulefunctions.rulefunctions;

namespace gg.parse
{
    public class RuleGraph<T> : IEnumerable<RuleBase<T>> where T : IComparable<T>
    {
        private int _nextRuleId = 0;

        private readonly Dictionary<string, RuleBase<T>> _nameRuleLookup = [];
        private readonly Dictionary<int, RuleBase<T>> _idRuleLookup = [];

        public RuleBase<T>? Root { get; set; }

        public IEnumerable<string> FunctionNames => _nameRuleLookup.Keys;

        public RuleGraph<T> Merge(RuleGraph<T> other)
        {
            Contract.RequiresNotNull(other);
            Contract.Requires(other != this);

            foreach (var rule in other)
            {
                if (!_nameRuleLookup.ContainsKey(rule.Name))
                {
                    // xxx needs to be a clone
                    rule.Id = -1;
                    RegisterRule(rule);
                }
            }

            return this;
        }

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
                    foreach (var subRule in ruleFunction.SubRules)
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

        // xxx todo return errors of unresolved replacements
        public void ResolveReferences()
        {
            foreach (var rule in this)
            {
                if (rule is IRuleComposition<T> composition)
                {
                    foreach (var referenceRule in composition.SubRules.Where(r => r is RuleReference<T>).Cast<RuleReference<T>>())
                    {
                        var replacement = FindRule(referenceRule.Reference);

                        Contract.Requires(replacement != null, $"Cannot find reference {referenceRule.Reference}.");

                        referenceRule.Rule = replacement!;
                        referenceRule.IsPartOfComposition = true;
                    }
                }
                if (rule is RuleReference<T> reference)
                {
                    var replacement = FindRule(reference.Reference);

                    Contract.Requires(replacement != null, $"Cannot find reference {reference.Reference}.");

                    reference.Rule = replacement;
                    // do not overwrite this property, if this rule is lower than its composition in the table
                    // this will be reset
                    //reference.IsPartOfComposition = false;
                }
            }
        }
    }
}
