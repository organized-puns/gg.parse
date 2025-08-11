using gg.parse.rulefunctions;

namespace gg.parse.compiler
{
    public delegate RuleBase<T> CompileFunction<T>(
        RuleCompiler<T> compiler,
        Annotation ruleDefinition,
        RuleDeclaration declaration, 
        CompileContext<T> context) where T : IComparable<T>;

    public class RuleCompiler<T> where T : IComparable<T>
    {
        public Dictionary<int, CompileFunction<T>> Functions { get; private set; } = [];

        public RuleCompiler<T> RegisterFunction(int parseFunctionId, CompileFunction<T> function)
        {
            Functions.Add(parseFunctionId, function);
            return this;
        }

        public RuleTable<T> Compile(CompileContext<T> context)
        {
            return Compile(context, new RuleTable<T>());
        }

        public RuleTable<T> Compile(CompileContext<T> session, RuleTable<T> result)
        {
            foreach (var node in session.AstNodes)
            {
                var (declaration, idx) = GetRuleDeclaration(session, node.Children, 0);
                var ruleDefinition = node.Children[idx];

                if (!Functions.ContainsKey(ruleDefinition.FunctionId))
                {
                    var rule = session.Parser.FindRule(ruleDefinition.FunctionId);
                    throw new CompilationException<int>(
                        $"Unable to match rule {rule.Name}({rule.Id}) to a compile function.", 
                        ruleDefinition.Range,
                        rule);
                }

                var compilationFunction = Functions[ruleDefinition.FunctionId];

                if (result.FindRule(declaration.Name) == null)
                {
                    var compiledRule = compilationFunction(this, ruleDefinition, declaration, session);
                    
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
