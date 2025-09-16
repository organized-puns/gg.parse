
using gg.core.util;
using gg.parse.rulefunctions;

namespace gg.parse.compiler
{
    public class NoCompilationFunctionException : Exception
    {
        public int RuleId { get; init; }

        public NoCompilationFunctionException(int id) 
            : base("No complilation function for the given rule id.")
        {
            RuleId = id;
        }
    }

    public delegate RuleBase<T> CompileFunction<T>(
        RuleCompiler<T> compiler,
        RuleDeclaration declaration, 
        CompileSession<T> context) where T : IComparable<T>;

    public class RuleCompiler<T> where T : IComparable<T>
    {
        public List<int> IgnoredRules { get; private set; } = [];

        public Dictionary<int, (CompileFunction<T> function, string? name)> Functions { get; private set; } = [];

        public (int functionId, AnnotationProduct product)[]? ProductLookup { get; set; }

        public RuleCompiler<T> WithAnnotationProductMapping((int functionId, AnnotationProduct product)[] productMapp)
        {
            ProductLookup = productMapp;
            return this;
        }

        public RuleCompiler<T> RegisterFunction(int parseFunctionId, CompileFunction<T> function, string? name = null)
        {
            Contract.Requires(function != null);

            Functions.Add(parseFunctionId, (function!, name ?? $"function_id:{parseFunctionId}"));
            return this;
        }

        public (CompileFunction<T> function, string? name) FindCompilationFunction(int parseFunctionId)
        {
            if (Functions.TryGetValue(parseFunctionId, out var compilationFunction))
            {
                return compilationFunction;
            }

            throw new NoCompilationFunctionException(parseFunctionId);
        }

        public RuleGraph<T> Compile(CompileSession<T> context)
        {
            return Compile(context, new RuleGraph<T>());
        }

        public RuleGraph<T> Compile(CompileSession<T> session, RuleGraph<T> resultGraph)
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
                    var (compilationFunction, _) = FindCompilationFunction(ruleBody.RuleId);

                    if (resultGraph.FindRule(declaration.Name) == null)
                    {
                        var compiledRule = compilationFunction(this, declaration, session);

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

            resultGraph.ResolveReferences();

            return resultGraph;
        }

        public bool TryGetProduct(int functionId, out AnnotationProduct product)
        {
            product = AnnotationProduct.Annotation;

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
        private RuleDeclaration GetRuleDeclaration(CompileSession<T> context, List<Annotation> ruleNodes, int index)
        {
            var idx = index;

            // annotation product is optional, (will default to Annotation)
            if (TryGetProduct(ruleNodes[idx].RuleId, out var product))
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
    }
}
