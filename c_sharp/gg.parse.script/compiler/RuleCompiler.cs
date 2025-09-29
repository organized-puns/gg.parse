
using gg.parse.rules;

namespace gg.parse.script.compiler
{
    public delegate IRule CompileFunction(
        RuleCompiler compiler,
        RuleDeclaration declaration,
        CompileSession context);

    public class RuleCompiler
    {
        public Dictionary<int, (CompileFunction function, string? name)> Functions { get; private set; } = [];

        public (int functionId, IRule.Output product)[]? ProductLookup { get; set; }

        public RuleCompiler WithAnnotationProductMapping((int functionId, IRule.Output product)[] productMapp)
        {
            ProductLookup = productMapp;
            return this;
        }

        public RuleCompiler RegisterFunction(int parseFunctionId, CompileFunction function, string? name = null)
        {
            Assertions.Requires(function != null);

            Functions.Add(parseFunctionId, (function!, name ?? $"function_id:{parseFunctionId}"));
            return this;
        }

        public RuleCompiler RegisterFunction(IRule rule, CompileFunction function)
        {
            Assertions.Requires(rule != null);
            Assertions.Requires(function != null);

            Functions.Add(rule!.Id, (function!, rule.Name ?? $"function_id:{rule.Id}"));
            return this;
        }

        public (CompileFunction function, string? name) FindCompilationFunction(int parseFunctionId)
        {
            if (Functions.TryGetValue(parseFunctionId, out var compilationFunction))
            {
                return compilationFunction;
            }

            throw new RuleReferenceException($"Cannot find a compilation function referred to by rule id {parseFunctionId}", parseFunctionId);
        }

        public RuleGraph<T> Compile<T>(CompileSession context) where T : IComparable<T>
        {
            return Compile(context, new RuleGraph<T>());
        }

        public RuleGraph<T> Compile<T>(CompileSession session, RuleGraph<T> resultGraph) where T : IComparable<T>
        {
            foreach (var node in session.AstNodes)
            {
                var declaration = GetRuleDeclaration(session, node.Children, 0);
                var ruleBody = declaration.RuleBodyAnnotation;

                // user provided an empty body - which is results in a warning but is allowed
                if (ruleBody == null)
                {
                    var compiledRule = resultGraph.RegisterRuleAndSubRules(new NopRule<T>(declaration.Name));

                    // First compiled rule will be assigned to the root. Seems the most intuitive
                    // xxx replace with name root or smth
                    resultGraph.Root ??= compiledRule;
                }
                else
                {
                    var (compilationFunction, _) = FindCompilationFunction(ruleBody.Rule.Id);

                    if (resultGraph.FindRule(declaration.Name) == null)
                    {
                        var compiledRule = (RuleBase<T>) compilationFunction(this, declaration, session);

                        resultGraph.RegisterRuleAndSubRules(compiledRule);

                        // First compiled rule will be assigned to the root. Seems the most intuitive
                        // xxx replace with name root or smth
                        resultGraph.Root ??= compiledRule;
                    }
                    else
                    {
                        // xxx add to the compilation errors, don't throw
                        throw new InvalidOperationException($"Trying to register a rule with the same name ({declaration.Name}).");
                    }
                }
            }

            // no root defined, this can happen when the input is empty or only contains include statements
            if (resultGraph.Root == null)
            {
                // xxx needs to be a warning
                throw new ArgumentException("Input text contains no root function. Make sure the main input always contains at least one rule.");
            }

            ResolveReferences(resultGraph);

            return resultGraph;
        }

        public bool TryGetProduct(int functionId, out IRule.Output product)
        {
            product = IRule.Output.Self;

            if (ProductLookup != null)
            {
                for (var i = 0; i < ProductLookup.Length; i++)
                {
                    if (functionId == ProductLookup[i].functionId)
                    {
                        product = ProductLookup[i].product;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Captures the rule header properties and body node
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ruleNodes">Nodes that make up the product, rulename, precendence and rulebody</param>
        /// <param name="index"></param>
        /// <returns></returns>
        private RuleDeclaration GetRuleDeclaration(CompileSession context, List<Annotation> ruleNodes, int index)
        {
            var idx = index;

            // annotation product is optional, (will default to Annotation)
            if (TryGetProduct(ruleNodes[idx].Rule.Id, out var product))
            {
                idx++;
            }

            // xxx assuming this is a string literal, need to validate
            var name = context.GetText(ruleNodes[idx].Range);
            idx++;

            // precedence is optional (will default to 0)
            // can exceed range if the rule is empty (ie rule = ;) so test for that
            var precedence = 0;
            
            if (ruleNodes.Count > idx
                && int.TryParse(context.GetText(ruleNodes[idx].Range), out precedence))
            {
                idx++;
            }
            
            Annotation? ruleBody = null;

            if (ruleNodes.Count > idx)
            {
                ruleBody = ruleNodes[idx];
            }

            return new(product, name, precedence, ruleBody);
        }

        /// <summary>
        /// Try find the rule referred to by name in the graph. If the name is not found an exception is thrown.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="RuleReferenceException"></exception>
        private static RuleBase<T> FindRule<T>(RuleGraph<T> graph, string name) where T : IComparable<T>
        {
            var referredRule = graph.FindRule(name);

            if (referredRule == null)
            {
                throw new RuleReferenceException($"Cannot find rule reffered to by name: {name}.", name);
            }

            return referredRule;
        }

        /// <summary>
        /// In all RuleReference rules in the graph find and set the actual rule they refer to.
        /// </summary>
        /// <param name="graph"></param>
        private static void ResolveReferences<T>(RuleGraph<T> graph) where T : IComparable<T>
        {
            foreach (var rule in graph)
            {
                if (rule is IRuleComposition<T> composition)
                {
                    foreach (var referenceRule in composition.Rules.Where(r => r is RuleReference<T>).Cast<RuleReference<T>>())
                    {
                        var referredRule = RuleCompiler.FindRule<T>(graph, referenceRule.Reference);

                        // note: we don't replace the rule we just set the reference. This allows
                        // these subrules to have their own annotation production. If we replace these 
                        // any production modifiers will affect the original rule
                        referenceRule.Rule = referredRule!;

                        // if the reference rule is part of a composition (sequence/option/oneormore/...)
                        // then use the referred rule's name / production to show up in the result/ast tree
                        // rather than this reference rule's name/production
                        referenceRule.DeferResultToReference = true;
                    }
                }
                else if (rule is RuleReference<T> reference)
                {
                    reference.Rule = RuleCompiler.FindRule<T>(graph, reference.Reference);
                }
            }
        }
    }
}
