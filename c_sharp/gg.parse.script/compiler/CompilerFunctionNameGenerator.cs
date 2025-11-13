// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.script.common;
using gg.parse.script.parser;
using gg.parse.util;

namespace gg.parse.script.compiler
{
    public static class CompilerFunctionNameGenerator
    {
        public static readonly string UnnamedRulePrefix = ".";

        public static string GenerateUnnamedRuleName(
            this Annotation annotation,
            ISession session,
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
                        + $"('{session.GetText(annotation[0]!)}, {session.GetText(annotation[1]!)}')";

                case ScriptParser.Names.CharacterSet:
                    Assertions.Requires(annotation.Count >= 1);

                    return $"{UnnamedRulePrefix}{ScriptParser.Names.CharacterSet}"
                        + $"('{session.GetText(annotation[0]!)}')";

                case ScriptParser.Names.Literal:
                    return $"{UnnamedRulePrefix}{ScriptParser.Names.Literal}({session.GetText(annotation)})";

                case ScriptParser.Names.Any:
                    return $"{UnnamedRulePrefix}{ScriptParser.Names.Any}";

                case ScriptParser.Names.ZeroOrMore:
                case ScriptParser.Names.ZeroOrOne:
                case ScriptParser.Names.OneOrMore:
                case ScriptParser.Names.Not:
                case ScriptParser.Names.If:
                    return CreateUnaryName(annotation, session, annotation.Name);

                case ScriptParser.Names.MatchOneOf:
                case ScriptParser.Names.Sequence:
                case ScriptParser.Names.Evaluation:
                    return CreateBinaryName(annotation, session, annotation.Name);
                
                case ScriptParser.Names.Reference:
                    var modifier = AnnotationPruning.None;
                    
                    if (annotation.Count > 1)
                    {
                        annotation.Children![0]!.TryReadPruning(out modifier);
                        //session.Compiler.TryMatchOutputModifier(annotation.Children[0]!.Rule.Id, out modifier);
                    }
                    
                    var modifierString = modifier.ToString().ToLower();

                    return $"{UnnamedRulePrefix}{modifierString}_{ScriptParser.Names.Reference}({GetReferenceName(annotation, session)})";

                case ScriptParser.Names.Log:
                    Assertions.Requires(annotation.Count >= 2);
                    Assertions.RequiresNotNull(annotation[0]);
                    Assertions.RequiresNotNull(annotation[0]![0]);
                    Assertions.RequiresNotNull(annotation[1]);
                    
                    return $"{UnnamedRulePrefix}{ScriptParser.Names.Log}({annotation[0]![0]!.Name}, '{session.GetText(annotation[1]!)}')";

                default:
                    break;
            }

            return annotation == null
                ? "null"
                : $"{UnnamedRulePrefix}{parentName}[{index}]";
        }

        private static string GetReferenceName(Annotation annotation, ISession session)
        {
            var text = session.GetText(annotation);

            return (text.StartsWith(AnnotationPruningToken.All) || text.StartsWith(AnnotationPruningToken.Root))
                ? text[1..]
                : text;
        }

        private static string CreateUnaryName(Annotation annotation, ISession session, string name) =>
                annotation.Children == null || annotation.Children.Count == 0
                        ? $"{UnnamedRulePrefix}{name}()"
                        : $"{UnnamedRulePrefix}{name}("
                            + GenerateUnnamedRuleName(annotation[0]!, session, name, 0)
                            + ")";

        private static string CreateBinaryName(Annotation annotation, ISession session, string name) =>
                $"{UnnamedRulePrefix}{name}("
                    + string.Join(", ", BinaryOperandNames(annotation, session, name))
                    + ")";
        private static IEnumerable<string> BinaryOperandNames(this Annotation annotation, ISession session, string parentName)
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
