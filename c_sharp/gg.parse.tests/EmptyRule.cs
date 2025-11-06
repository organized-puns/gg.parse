// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;

namespace gg.parse.tests
{
    public class EmptyRule : IRule
    {
        public string Name { get; init; }
        public int Id { get; init; }
        public int Precedence { get; init; }
        public AnnotationPruning Prune { get; init; }

        public EmptyRule(
            string name, 
            int precedence = 0,
            AnnotationPruning pruning = AnnotationPruning.None
        )
        {
            Name = name;
            Precedence = precedence;
            Prune = pruning;
        }

        public EmptyRule(
            int id, 
            string name = "DummyRule", 
            int precedence = 0, 
            AnnotationPruning pruning = AnnotationPruning.None)
        {
            Id = id;
            Name = name;
            Precedence = precedence;
            Prune = pruning;
        }

        public object Clone() => MemberwiseClone();

        ParseResult IRule.Parse(Array input, int start)
        {
            return ParseResult.Success;
        }
    }
}
