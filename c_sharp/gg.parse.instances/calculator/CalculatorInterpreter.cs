using gg.parse.ebnf;
using System.Globalization;

namespace gg.parse.instances.calculator
{
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

        public class Ids
        {
            public int Group { get; init; }

            public int Number { get; init; }

            public int Unary { get; init; }

            public int Plus { get; init; }

            public int Minus { get; init; }

            public int Multiply { get; init; }

            public int Divide { get; init; }

            public int Add { get; init; }

            public int Subtract { get; init; }
        }

        private Ids _graphIds;
        private EbnfParser _parser; 


        public CalculatorInterpreter(string tokenizerSpec, string grammarSpec)
        {
            _parser = new EbnfParser(tokenizerSpec, grammarSpec);
            SetIds(_parser.EbnfGrammarParser);
        }

        private CalculatorInterpreter SetIds(RuleGraph<int> graph)
        {
            if (!graph.TryFindRule<RuleBase<int>>(NodeNames.Group, out var groupRule) ||
                !graph.TryFindRule<RuleBase<int>>(NodeNames.Number, out var numberRule) ||
                !graph.TryFindRule<RuleBase<int>>(NodeNames.Unary, out var unaryRule) ||
                !graph.TryFindRule<RuleBase<int>>(NodeNames.Plus, out var plusRule) ||
                !graph.TryFindRule<RuleBase<int>>(NodeNames.Minus, out var minusRule) ||
                !graph.TryFindRule<RuleBase<int>>(NodeNames.Multiply, out var multiplyRule) ||
                !graph.TryFindRule<RuleBase<int>>(NodeNames.Division, out var divisionRule) ||
                !graph.TryFindRule<RuleBase<int>>(NodeNames.Addition, out var additionRule) ||
                !graph.TryFindRule<RuleBase<int>>(NodeNames.Subtraction, out var subtractionRule))
            {
                throw new InvalidOperationException("One or more required rules are missing from the rule graph.");
            }

            _graphIds = new Ids
            {
                Group = groupRule!.Id,
                Number = numberRule!.Id,
                Unary = unaryRule!.Id,
                Plus = plusRule!.Id,
                Minus = minusRule!.Id,
                Multiply = multiplyRule!.Id,
                Divide = divisionRule!.Id,
                Add = additionRule!.Id,
                Subtract = subtractionRule!.Id
            };

            return this;
        }

        public double Interpret(string text)
        {
            if (_parser.TryBuildAstTree(text, out var tokens, out var tree))
            {
                return Interpret(text, tree!.Annotations![0], tokens!.Annotations!);
            }

            throw new ArgumentException("Failed to parse text.");
        }

        public double Interpret(string text, Annotation node, List<Annotation> tokens)
        {
            if (node.FunctionId == _graphIds.Number)
            {
                var valueText = EbnfParser.GetText(text, node, tokens);
                return double.Parse(valueText, CultureInfo.InvariantCulture);
            }
            else if (node.FunctionId == _graphIds.Unary)
            {
                var sign = node.Children[0].FunctionId == _graphIds.Plus ? 1.0 : -1.0;
                return sign * Interpret(text, node.Children[1], tokens);
            }
            else if (node.FunctionId == _graphIds.Multiply)
            {
                return Interpret(text, node.Children[0], tokens) * Interpret(text, node.Children[2], tokens);
            }
            else if (node.FunctionId == _graphIds.Add)
            {
                return Interpret(text, node.Children[0], tokens) + Interpret(text, node.Children[2], tokens);
            }
            else if (node.FunctionId == _graphIds.Subtract)
            {
                return Interpret(text, node.Children[0], tokens) - Interpret(text, node.Children[2], tokens);
            }
            else if (node.FunctionId == _graphIds.Divide)
            {
                return Interpret(text, node.Children[0], tokens) / Interpret(text, node.Children[2], tokens);
            }
            else if (node.FunctionId == _graphIds.Group)
            {
                return Interpret(text, node.Children[0], tokens);
            }

            throw new NotImplementedException();
        }
    }
}
