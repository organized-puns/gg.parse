using gg.parse.rulefunctions;
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

        public RuleCompiler<T> RegisterFunction(int parseFunctionId, CompileFunction<T> function, string? name = null)
        {
            Contract.Requires(function != null);

            Functions.Add(parseFunctionId, (function!, name ?? $"function_id:{parseFunctionId}"));
            return this;
        }

        public RuleTable<T> Compile(CompileSession<T> context)
        {
            return Compile(context, new RuleTable<T>());
        }

        public RuleTable<T> Compile(CompileSession<T> session, RuleTable<T> result)
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

            result.ResolveReferences();

            return result;
        }

        private static RuleDeclaration GetRuleDeclaration(CompileSession<T> context, List<Annotation> ruleNodes, int index)
        {
            var idx = index;

            if (context.TryGetProduct(ruleNodes[idx].FunctionId, out var product))
            {
                idx++;
            }
            
            var name = context.GetText(ruleNodes[idx].Range);
            idx++;

            return new(ruleNodes[idx], product, name);
        }
    }
}
