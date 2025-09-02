using gg.parse.ebnf;
using System.Globalization;

namespace gg.parse.instances.calculator
{
    public class CalculatorInterpreter
    {
        public static class NodeNames
        {
            public static readonly string Group = "group";

            public static readonly string Float = "float";

            public static readonly string Int = "int";

            public static readonly string Multiply = "multiplication";

            public static readonly string Division = "division";

            public static readonly string Addition = "addition";

            public static readonly string Subtraction = "subtraction";
        }

        public class Ids
        {
            public int Group { get; init; }

            public int Float { get; init; }

            public int Int { get; init; }

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
                !graph.TryFindRule<RuleBase<int>>(NodeNames.Float, out var floatRule) ||
                !graph.TryFindRule<RuleBase<int>>(NodeNames.Int, out var intRule) ||
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
                Float = floatRule!.Id,
                Int = intRule!.Id,
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
            if (node.FunctionId == _graphIds.Int)
            {
                var valueText = EbnfParser.GetText(text, node, tokens);
                return int.Parse(valueText, CultureInfo.InvariantCulture);
            }
            else if (node.FunctionId == _graphIds.Float)
            {
                var valueText = EbnfParser.GetText(text, node, tokens);
                return double.Parse(valueText, CultureInfo.InvariantCulture);
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
