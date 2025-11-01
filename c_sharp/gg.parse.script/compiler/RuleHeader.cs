// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;

namespace gg.parse.script.compiler
{
    /// <summary>
    /// Name, output and precedence of a rule 
    /// </summary>
    /// <param name="product"></param>
    /// <param name="name"></param>
    public class RuleHeader
    {
        public string Name { get; init; }

        public AnnotationPruning Prune { get; init; }
              
        public int Precedence { get; init; }

        /// <summary>
        /// Number of actual tokens occupied by this header
        /// </summary>
        public int Length { get; init; }

        public bool IsTopLevel { get; init; }

        public RuleHeader(AnnotationPruning prune, string name, int precedence, int length, bool isTopLevel = true)
        {
            Name = name;
            Prune = prune;
            Precedence = precedence;
            Length = length;
            IsTopLevel = isTopLevel;
        }

        public RuleHeader(AnnotationPruning product, string name)
            : this(product, name, 0, 0) { }
    }
}
