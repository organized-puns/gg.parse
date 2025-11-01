// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;

namespace gg.parse.script.parser
{
    public static class AnnotationPruningToken
    {
        public const string All = "-a ";

        public const string Children = "-c ";

        public const string None = "";

        public const string Root = "-r ";
        

        public static string GetTokenString(this AnnotationPruning pruning) =>

            pruning switch
            {
                AnnotationPruning.None => None,
                AnnotationPruning.Root => Root,
                AnnotationPruning.All => All,
                AnnotationPruning.Children => Children,
                _ => throw new NotImplementedException(),
            };

        public static AnnotationPruning GetToken(this string pruningString) =>

            pruningString switch
            {
                Root => AnnotationPruning.Root,
                All => AnnotationPruning.All,
                Children => AnnotationPruning.Children,
                None => AnnotationPruning.None,
                _ => throw new NotImplementedException(),
            };


        public static (string token, string name) SplitRuleNameAndPruningTokenText(this string name)
        {
            if (name.StartsWith(All))
            {
                return (All, name[All.Length..]);
            }
            else if (name.StartsWith(Root))
            {
                return (Root, name[Root.Length..]);
            }
            else if (name.StartsWith(Children))
            {
                return (Children, name[Children.Length..]);
            }
            // name must be identifier compliant otherwise to pass
            else if ((name[0] >= 'a' && name[0] <= 'z')
                || (name[0] >= 'A' && name[0] <= 'Z')
                || (name[0] == '_'))
            {
                return (None, name);
            }

            throw new NotImplementedException($"The rule name '{name}' does not start with a valid pruning token.");
        }

        public static (string outputName, AnnotationPruning product) SplitNameAndPruning(this string name)
        {
            var (tokenText, ruleName) = SplitRuleNameAndPruningTokenText(name);
            return (ruleName, GetToken(tokenText));
        }
    }
}
