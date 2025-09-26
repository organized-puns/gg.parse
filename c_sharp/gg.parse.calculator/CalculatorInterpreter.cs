using System.Globalization;

using gg.parse.script;

namespace gg.parse.instances.calculator
{
    public delegate double CalculatorFunction(string text, Annotation node, List<Annotation> tokens);

    public class CalculatorInterpreter
    {
        public static class NodeNames
        {
            public static readonly string Group = "group";

            public static readonly string Number = "number";

            public static readonly string Unary = "unary_operation";

            public static readonly string Plus = "plus";

            public static readonly string Minus = "minus";

            public static readonly string Multiply = "multiplication";

            public static readonly string Division = "division";

            public static readonly string Addition = "addition";

            public static readonly string Subtraction = "subtraction";
        }

        private readonly RuleGraphBuilder _builder; 

        private readonly Dictionary<IRule, CalculatorFunction> _functionLookup;

        public RuleGraphBuilder Builder => _builder;

        public CalculatorInterpreter(string tokenizerSpec, string grammarSpec)
        {
            _builder = new RuleGraphBuilder();            
            _builder.InitializeFromDefinition(tokenizerSpec, grammarSpec);

            _functionLookup = CreateFunctionLookup(_builder.Parser!);
        }

        public double Interpret(string text)
        {
            var (tokensResult, treeResult) = _builder.Parse(text);

            if (tokensResult.FoundMatch && treeResult.FoundMatch)
            {
                return Interpret(text, treeResult!.Annotations![0], tokensResult!.Annotations!);
            }

            throw new ArgumentException("Failed to parse text.");
        }

        public double Interpret(string text, in Annotation node, in List<Annotation> tokens) =>
            _functionLookup.TryGetValue(node.Rule, out var function) 
                ? function(text, node, tokens)
                : throw new NotImplementedException();
        

        private Dictionary<IRule, CalculatorFunction> CreateFunctionLookup(RuleGraph<int> graph) =>       
#nullable disable
            new()
            {                
                { graph.FindRule(NodeNames.Group), (text, node, tokens) => 
                    Interpret(text, node.Children[0], tokens) },

                { graph.FindRule(NodeNames.Number), (text, node, tokens) => 
                    double.Parse(node.GetText(text, tokens), CultureInfo.InvariantCulture) },

                { graph.FindRule(NodeNames.Unary), (text, node, tokens) =>
                    {
                        var sign = node.Children[0].Rule == graph.FindRule(NodeNames.Plus) ? 1.0 : -1.0;
                        return sign * Interpret(text, node.Children[1], tokens);
                    }
                },
                { graph.FindRule(NodeNames.Multiply), (text, node, tokens) =>
                        Interpret(text, node.Children[0], tokens) * Interpret(text, node.Children[2], tokens) },

                { graph.FindRule(NodeNames.Addition), (text, node, tokens) =>
                        Interpret(text, node.Children[0], tokens) + Interpret(text, node.Children[2], tokens) },

                { graph.FindRule(NodeNames.Subtraction), (text, node, tokens) =>
                        Interpret(text, node.Children[0], tokens) - Interpret(text, node.Children[2], tokens) },

                { graph.FindRule(NodeNames.Division), (text, node, tokens) =>
                        Interpret(text, node.Children[0], tokens) / Interpret(text, node.Children[2], tokens) },
            };
#nullable enable       
    }
}
