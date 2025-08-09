using gg.parse.rulefunctions;

namespace gg.parse.compiler
{
    public delegate RuleBase<T> CompileFunction<T>(
        Annotation ruleDefinition,
        RuleDeclaration declaration, 
        CompileContext<T> context) where T : IComparable<T>;

    public class RuleCompiler<T> where T : IComparable<T>
    {
        private CompileContext<T>? _context;


        public RuleTable<T> Compile(CompileContext<T> context)
        {
            _context = context;

            foreach (var node in context.AstNodes)
            {
                var (declaration, idx) = GetRuleDeclaration(node.Children, 0);
                var ruleDefinition = node.Children[idx];

                if (!_context.Functions.ContainsKey(ruleDefinition.FunctionId))
                {
                    var rule = context.Parser.FindRule(ruleDefinition.FunctionId);
                    throw new CompilationException<int>(
                        $"Unable to match rule {rule.Name}({rule.Id}) to a compile function.", 
                        ruleDefinition.Range,
                        rule);
                }

                var compilationFunction = _context.Functions[ruleDefinition.FunctionId];
                var compiledRule = compilationFunction(ruleDefinition, declaration, _context);

                // First compiled rule will be assigned to the root. Seems the most intuitive
                if (_context.Output.Root == null)
                {
                    _context.Output.Root = compiledRule;
                }
            }

            _context.Output.ResolveReferences();

            return _context.Output;
        }

        private (RuleDeclaration declaration, int readIndex) GetRuleDeclaration(List<Annotation> ruleNodes, int index)
        {
            var idx = index;

            if (_context.TryGetProduct(ruleNodes[idx].FunctionId, out var product))
            {
                idx++;
            }
            
            var name = _context.GetText(ruleNodes[idx].Range);
            idx++;

            return (new(product, name), idx);
        }
    }
}
