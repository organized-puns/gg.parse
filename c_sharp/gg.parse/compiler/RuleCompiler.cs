using gg.parse.rulefunctions;

namespace gg.parse.compiler
{
    public delegate RuleBase<T> CompileFunction<T>(
        Annotation ruleDefinition,
        RuleDeclaration declaration, 
        CompileContext<T> context) where T : IComparable<T>;

    public class RuleCompiler<T> where T : IComparable<T>
    {

        public RuleTable<T> Compile(CompileContext<T> context)
        {
            return Compile(context, new RuleTable<T>());
        }

        public RuleTable<T> Compile(CompileContext<T> context, RuleTable<T> result)
        {
            foreach (var node in context.AstNodes)
            {
                var (declaration, idx) = GetRuleDeclaration(context, node.Children, 0);
                var ruleDefinition = node.Children[idx];

                if (!context.Functions.ContainsKey(ruleDefinition.FunctionId))
                {
                    var rule = context.Parser.FindRule(ruleDefinition.FunctionId);
                    throw new CompilationException<int>(
                        $"Unable to match rule {rule.Name}({rule.Id}) to a compile function.", 
                        ruleDefinition.Range,
                        rule);
                }

                var compilationFunction = context.Functions[ruleDefinition.FunctionId];

                if (result.FindRule(declaration.Name) == null)
                {
                    var compiledRule = compilationFunction(ruleDefinition, declaration, context);
                    
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

        private static (RuleDeclaration declaration, int readIndex) GetRuleDeclaration(CompileContext<T> context, List<Annotation> ruleNodes, int index)
        {
            var idx = index;

            if (context.TryGetProduct(ruleNodes[idx].FunctionId, out var product))
            {
                idx++;
            }
            
            var name = context.GetText(ruleNodes[idx].Range);
            idx++;

            return (new(product, name), idx);
        }
    }
}
