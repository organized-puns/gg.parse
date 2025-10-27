// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse
{
    public enum AnnotationPruning
    {
        /// <summary>
        /// Prunes nothing and returns an annotation for the matched item.
        /// </summary>
        None,

        /// <summary>
        /// Prunes the root and returns the annotations produced by any child rules.
        /// </summary>
        Root,

        /// <summary>
        /// Prunes the children and returns only the root.
        /// </summary>
        Children,

        /// <summary>
        /// Prunes the root and the children.
        /// </summary>
        All
    }

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

        /// <summary>
        /// Map pruning to a human readable string.
        /// </summary>
        /// <param name="pruning"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static string ToString(this AnnotationPruning pruning) =>

            pruning switch
            {
                AnnotationPruning.None => "none",
                AnnotationPruning.Root => "root",
                AnnotationPruning.All => "all",
                AnnotationPruning.Children => "children",
                _ => throw new NotImplementedException($"{pruning} has no backing value."),
            };
    }
}
