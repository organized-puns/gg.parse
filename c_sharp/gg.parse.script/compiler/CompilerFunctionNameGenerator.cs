// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.script.parser;
using gg.parse.util;

namespace gg.parse.script.compiler
{
    public static class CompilerFunctionNameGenerator
    {
        public static readonly string UnnamedRulePrefix = ".";

        public static string GenerateUnnamedRuleName(
            this Annotation annotation,
            CompileSession session,
            string parentName, 
            int index)
        {
            Assertions.RequiresNotNull(annotation);

            switch (annotation)
            {
                case ScriptParser.Names.CharacterRange:
                    Assertions.Requires(annotation.Count >= 2);
                    Assertions.RequiresNotNull(annotation[0]);
                    Assertions.RequiresNotNull(annotation[1]);

                    return $"{UnnamedRulePrefix}{ScriptParser.Names.CharacterRange}"
                        + $"('{session.GetText(annotation[0]!.Range)}, {session.GetText(annotation[1]!.Range)}')";

                case ScriptParser.Names.CharacterSet:
                    Assertions.Requires(annotation.Count >= 1);

                    return $"{UnnamedRulePrefix}{ScriptParser.Names.CharacterSet}"
                        + $"('{session.GetText(annotation[0]!.Range)}')";

                case ScriptParser.Names.Literal:
                    return $"{UnnamedRulePrefix}{ScriptParser.Names.Literal}({session.GetText(annotation.Range)})";

                case ScriptParser.Names.Any:
                    return $"{UnnamedRulePrefix}{ScriptParser.Names.Any}";

                case ScriptParser.Names.ZeroOrMore:
                case ScriptParser.Names.ZeroOrOne:
                case ScriptParser.Names.OneOrMore:
                case ScriptParser.Names.Not:
                case ScriptParser.Names.If:
                    return CreateUnaryName(annotation, session, annotation.Name);

                case ScriptParser.Names.Option:
                case ScriptParser.Names.Sequence:
                case ScriptParser.Names.Evaluation:
                    return CreateBinaryName(annotation, session, annotation.Name);
                
                case ScriptParser.Names.Reference:
                    var modifier = AnnotationPruning.None;
                    
                    if ((annotation.Children != null && annotation.Children.Count > 1))
                    {
                        session.Compiler.TryMatchOutputModifier(annotation.Children[0]!.Rule.Id, out modifier);
                    }
                    
                    var modifierString = modifier.ToString().ToLower();

                    return $"{UnnamedRulePrefix}{modifierString}_{ScriptParser.Names.Reference}({GetReferenceName(annotation, session)})";

                case ScriptParser.Names.Log:
                    Assertions.Requires(annotation.Count >= 2);
                    Assertions.RequiresNotNull(annotation[0]);
                    Assertions.RequiresNotNull(annotation[0]![0]);
                    Assertions.RequiresNotNull(annotation[1]);
                    
                    return $"{UnnamedRulePrefix}{ScriptParser.Names.Log}({annotation[0]![0]!.Name}, '{session.GetText(annotation[1]!.Range)}')";

                default:
                    break;
            }

            return annotation == null
                ? "null"
                : $"{UnnamedRulePrefix}{parentName}[{index}]";
        }

        private static string GetReferenceName(Annotation annotation, CompileSession session)
        {
            var text = session.GetText(annotation.Range);

            return (text.StartsWith(AnnotationPruningToken.All) || text.StartsWith(AnnotationPruningToken.Root))
                ? text[1..]
                : text;
        }

        private static string CreateUnaryName(Annotation annotation, CompileSession session, string name) =>
                annotation.Children == null || annotation.Children.Count == 0
                        ? $"{UnnamedRulePrefix}{name}()"
                        : $"{UnnamedRulePrefix}{name}("
                            + GenerateUnnamedRuleName(annotation[0]!, session, name, 0)
                            + ")";

        private static string CreateBinaryName(Annotation annotation, CompileSession session, string name) =>
                $"{UnnamedRulePrefix}{name}("
                    + string.Join(", ", BinaryOperandNames(annotation, session, name))
                    + ")";
        private static IEnumerable<string> BinaryOperandNames(this Annotation annotation, CompileSession session, string parentName)
        {
            var idx = 0;

            if (annotation != null && annotation.Children != null && annotation.Children.Count > 0)
            {
                return annotation!.Select(child => GenerateUnnamedRuleName(child, session, parentName, idx++));
            }

            return [""];
        }

    }
}
