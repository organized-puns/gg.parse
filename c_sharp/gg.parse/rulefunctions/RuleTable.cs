using gg.core.util;
using System.Collections;

namespace gg.parse.rulefunctions
{
    public class RuleTable<T> : IEnumerable<RuleBase<T>> where T : IComparable<T>
    {
        private int _nextRuleId = 0;

        private readonly Dictionary<string, RuleBase<T>> _nameRuleLookup = [];
        private readonly Dictionary<int, RuleBase<T>> _idRuleLookup = [];

        public RuleBase<T>? Root { get; set; }

        public IEnumerable<string> FunctionNames => _nameRuleLookup.Keys;

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

        public TRule GetOrRegisterRule<TRule>(string name, Func<TRule> ruleFactoryMethod) where TRule : RuleBase<T>
        {
            Contract.RequiresNotNull(ruleFactoryMethod);
            Contract.RequiresNotNullOrEmpty(name);

            if (_nameRuleLookup.TryGetValue(name, out var existingRule))
            {
                Contract.Requires(existingRule.GetType() == typeof(TRule),
                    $"Rule with name '{name}' already exists in the rule table with a different type.");
                return (TRule)existingRule;
            }

            var rule = ruleFactoryMethod();

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

        public MatchDataSequence<T> Literal(T[] sequence)
        {
            var product = AnnotationProduct.None;
            var ruleName = $"{product.GetPrefix()}{TokenNames.Literal}({string.Concat(sequence)})";
            return Literal(ruleName, product, sequence);
        }

        

        public MatchDataSequence<T> Literal(string name, AnnotationProduct product, T[] sequence)
        {
            return TryFindRule(name, out MatchDataSequence<T>? existingRule)
                ? existingRule!
                : RegisterRule(new MatchDataSequence<T>(name, sequence, product));
        }


        public MatchSingleData<T> Single(string name, AnnotationProduct product, T tokenId) =>
            TryFindRule(name, out MatchSingleData<T>? existingRule)
                 ? existingRule!
                 : RegisterRule(new MatchSingleData<T>(name, tokenId, product));


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

        public RuleBase<T> Sequence(params T[] data)
        {
            var product = AnnotationProduct.None;
            var ruleName = $"{product.GetPrefix()}{TokenNames.DataSequence}({string.Join(", ", data)})";
            return Sequence(ruleName, product, data);
        }

        public RuleBase<T> Sequence(string ruleName, AnnotationProduct product, params T[] data) =>
            TryFindRule(ruleName, out MatchDataSequence<T>? existingRule)
                ? existingRule!
                : RegisterRule(new MatchDataSequence<T>(ruleName, data, product));


        public MatchFunctionSequence<T> Sequence(params RuleBase<T>[] functions)
        {
            var product = AnnotationProduct.None;
            var ruleName = $"{product.GetPrefix()}{TokenNames.FunctionSequence}({string.Join(",", functions.Select(f => f.Name))})";

            return Sequence(ruleName, product, functions);
        }

        public MatchFunctionSequence<T> Sequence(string ruleName, AnnotationProduct product, params RuleBase<T>[] functions) =>

            TryFindRule(ruleName, out MatchFunctionSequence<T>? existingRule)
                ? existingRule!
                : RegisterRule(new MatchFunctionSequence<T>(ruleName, product, functions));

        public MatchOneOfFunction<T> OneOf(params RuleBase<T>[] rules)
        {
            var product = AnnotationProduct.None;
            var ruleName = $"{product.GetPrefix()}{TokenNames.OneOf}({string.Join(",", rules.Select(f => f.Name))})";
            return OneOf(ruleName, product, rules);
        }

        public MatchOneOfFunction<T> OneOf(string name, AnnotationProduct product, params RuleBase<T>[] rules) =>
            TryFindRule(name, out MatchOneOfFunction<T>? existingRule)
                 ? existingRule!
                 : RegisterRule(new MatchOneOfFunction<T>(name, product, rules));


        public MatchFunctionCount<T> ZeroOrMore(string name, AnnotationProduct product, RuleBase<T> function) =>
           TryFindRule(name, out MatchFunctionCount<T>? existingRule)
                ? existingRule!
                : RegisterRule(new MatchFunctionCount<T>(name, function, product, 0, 0));

        public MatchFunctionCount<T> ZeroOrMore(RuleBase<T> function)
        {
            var product = AnnotationProduct.None;
            var ruleName = $"{product.GetPrefix()}{TokenNames.ZeroOrMore}({function.Name})";
            return ZeroOrMore(ruleName, product, function);
        }

        public MatchFunctionCount<T> ZeroOrOne(string name, AnnotationProduct product, RuleBase<T> function) =>
           TryFindRule(name, out MatchFunctionCount<T>? existingRule)
                ? existingRule!
                : RegisterRule(new MatchFunctionCount<T>(name, function, product, 0, 1));


        public MatchFunctionCount<T> ZeroOrOne(RuleBase<T> function)
        {
            var product = AnnotationProduct.None;
            var ruleName = $"{product.GetPrefix()}{TokenNames.ZeroOrOne}({function.Name})";
            return ZeroOrOne(ruleName, product, function);
        }

        public RuleBase<T> OneOrMore(string name, AnnotationProduct product, RuleBase<T> function) =>
            TryFindRule(name, out MatchFunctionCount<T>? existingRule)
                ? existingRule!
                : RegisterRule(new MatchFunctionCount<T>(name, function, product, 1, 0));


        public RuleBase<T> OneOrMore(RuleBase<T> function) =>
            OneOrMore($"{TokenNames.OneOrMore}({function.Name})", AnnotationProduct.None, function);


        public MatchAnyData<T> Any()
        {
            var product = AnnotationProduct.None;
            var ruleName = $"{product.GetPrefix()}{TokenNames.AnyCharacter}(1,1)";
            return Any(ruleName, product, 1, 1);
        }

        public MatchAnyData<T> Any(string name, AnnotationProduct product, int min, int max) =>
            TryFindRule(name, out MatchAnyData<T>? existingRule)
                 ? existingRule!
                 : RegisterRule(new MatchAnyData<T>(name, product, min, max));

        public MatchNotFunction<T> Not(RuleBase<T> rule)
        {
            var product = AnnotationProduct.None;
            var ruleName = $"{product.GetPrefix()}{TokenNames.Not}({rule.Name})";
            return Not(ruleName, product, rule);
        }

        public MatchNotFunction<T> Not(string name, AnnotationProduct product, RuleBase<T> rule) =>
            TryFindRule(name, out MatchNotFunction<T>? existingRule)
                 ? existingRule!
                 : RegisterRule(new MatchNotFunction<T>(name, product, rule));


        // xxx todo return errors of unresolved replacements
        public void ResolveReferences()
        {
            foreach (var rule in this)
            {
                if (rule is IRuleComposition<T> composition)
                {
                    foreach (var subRule in composition.SubRules.Where(r => r is RuleReference<T>).Cast<RuleReference<T>>())
                    {
                        var replacement = FindRule(subRule.Reference);

                        Contract.Requires(replacement != null, $"Cannot find reference {subRule.Reference}.");

                        composition.ReplaceSubRule(subRule, replacement!);
                    }
                }
                if (rule is RuleReference<T> reference)
                {
                    var replacement = FindRule(reference.Reference);

                    Contract.Requires(replacement != null, $"Cannot find reference {reference.Reference}.");

                    _nameRuleLookup[rule.Name] = replacement!;
                    _idRuleLookup[rule.Id] = replacement!;

                    if (rule == Root)
                    {
                        Root = replacement;
                    }
                }
            }
        }
    }
}
