using System.Diagnostics.Contracts;

namespace gg.parse.compiler
{
    public delegate RuleBase<T> CompileFunction<T>(
        RuleCompiler<T> compiler,
        RuleDeclaration declaration, 
        CompileSession<T> context) where T : IComparable<T>;

    public class RuleCompiler<T> where T : IComparable<T>
    {
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

        public RuleGraph<T> Compile(CompileSession<T> context)
        {
            return Compile(context, new RuleGraph<T>());
        }

        public RuleGraph<T> Compile(CompileSession<T> session, RuleGraph<T> result)
        {
            foreach (var node in session.AstNodes)
            {
                var declaration = GetRuleDeclaration(session, node.Children, 0);
                var ruleDefinition = declaration.AssociatedAnnotation;

                if (!Functions.ContainsKey(ruleDefinition.FunctionId))
                {
                    throw new CompilationException<int>(
                        $"Unable to match rule {ruleDefinition.FunctionId} to a compile function.", 
                        ruleDefinition.Range);
                }

                var compilationFunction = Functions[ruleDefinition.FunctionId].function;

                if (result.FindRule(declaration.Name) == null)
                {
                    var compiledRule = compilationFunction(this, declaration, session);
                    
                    result.RegisterRuleAndSubRules(compiledRule);
                    
                    // First compiled rule will be assigned to the root. Seems the most intuitive
                    if (result.Root == null)
                    {
                        result.Root = compiledRule;
                    }
                }
            }

            // no root defined, this can happen when the input is empty or only contains include statements
            if (result.Root == null)
            {
                throw new ArgumentException("Input text contains no root function. Make sure the main input always contains at least one rule.");
            }

            result.ResolveReferences();

            return result;
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


        private RuleDeclaration GetRuleDeclaration(CompileSession<T> context, List<Annotation> ruleNodes, int index)
        {
            var idx = index;

            if (TryGetProduct(ruleNodes[idx].FunctionId, out var product))
            {
                idx++;
            }

            var name = context.GetText(ruleNodes[idx].Range);
            idx++;

            return new(ruleNodes[idx], product, name);
        }
    }
}
