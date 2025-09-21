using gg.core.util;
using System.Text;
using System.Text.RegularExpressions;

namespace gg.parse.script
{
    public static class ScriptUtils
    {
        public static string AstToString(ScriptParser parser)
        {
            Assertions.RequiresNotNull(parser);
            Assertions.RequiresNotNull(parser!.GrammarSession!);
            Assertions.RequiresNotNull(parser.GrammarSession!.Text!);
            Assertions.RequiresNotNull(parser.GrammarSession!.Tokens!);
            Assertions.RequiresNotNull(parser.GrammarSession!.AstNodes!);

            return AstToString(parser.GrammarSession.Text!, parser.GrammarSession.Tokens!, parser.GrammarSession.AstNodes!);
        }

        public static string AstToString(string text, List<Annotation> tokens, List<Annotation> astNodes, string indentStr = "   ")
        {
            var builder = new StringBuilder();
            var indent = 0;

            if (tokens != null && astNodes.Count > 0)
            {
                foreach (var astNode in astNodes)
                {
                    builder.Append(AstToString(indent, indentStr, astNode, text, tokens));
                }
            }

            return builder.ToString();
        }

        private static string AstToString(int indentCount, string indentStr, Annotation node, string text, List<Annotation> tokens)
        {
            var builder = new StringBuilder();

            var rule = node.Rule;

            for (var i = 0; i < indentCount; i++)
            {
                builder.Append(indentStr);
            }

            var nodeText = Regex.Escape(node.GetText(text, tokens));

            if (nodeText.Length > 20)
            {
                nodeText = $"{nodeText.Substring(0, 17)}...";
            }

            builder.AppendLine($"[{node.Range.Start},{node.Range.End}]{rule.Name}({rule.Id}): {nodeText}");

            if (node.Children != null && node.Children.Count > 0)
            {
                foreach (var child in node.Children)
                {
                    builder.Append(AstToString(indentCount + 1, indentStr, child, text, tokens));
                }
            }

            return builder.ToString();
        }
    }
}
