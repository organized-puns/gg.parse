using System.Collections;
using System.Data;

using gg.parse.rules;

namespace gg.parse
{
    public class RuleGraph<T> : IEnumerable<RuleBase<T>> where T : IComparable<T>
    {
        private int _nextRuleId = 0;

        private readonly Dictionary<string, RuleBase<T>> _nameRuleLookup = [];
        private readonly Dictionary<int, RuleBase<T>> _idRuleLookup = [];

        public RuleBase<T>? Root { get; set; }

        public IEnumerable<string> FunctionNames => _nameRuleLookup.Keys;

        /// <summary>
        /// Number of registered rules.
        /// </summary>
        public int Count => _nameRuleLookup.Count;

        public RuleGraph<T> Merge(RuleGraph<T> other)
        {
            Assertions.RequiresNotNull(other);
            Assertions.Requires(other != this);

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

        // xxx todo return errors of unresolved replacements
        public void ResolveReferences()
        {
            foreach (var rule in this)
            {
                if (rule is IRuleComposition<T> composition)
                {
                    foreach (var referenceRule in composition.Rules.Where(r => r is RuleReference<T>).Cast<RuleReference<T>>())
                    {
                        var referredRule = FindRule(referenceRule.Reference);

                        Assertions.Requires(referredRule != null, $"Cannot find reference {referenceRule.Reference}.");

                        // note: we don't replace the rule we just set the reference. This allows
                        // these subrules to have their own annotation production. If we replace these 
                        // any production modifiers will affect the original rule
                        referenceRule.Rule = referredRule!;
                        referenceRule.IsPartOfComposition = true;
                    }
                }
                if (rule is RuleReference<T> reference)
                {
                    var referredRule = FindRule(reference.Reference);

                    Assertions.Requires(referredRule != null, $"Cannot find reference {reference.Reference}.");

                    reference.Rule = referredRule;
                    
                    // do not overwrite this property, if this rule is lower than its composition in the table
                    // this will be reset
                    // reference.IsPartOfComposition = false;
                }
            }
        }

        public TRule FindOrRegister<TRule>(
            string ruleName,
            AnnotationProduct product,
            Func<string, AnnotationProduct, TRule> factoryMethod)
            where TRule : RuleBase<T> =>

            TryFindRule(ruleName, out TRule? existingRule)
                     ? existingRule!
                     : factoryMethod(ruleName, product);

    }
}
