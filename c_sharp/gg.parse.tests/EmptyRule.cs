// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.tests
{
    public class EmptyRule : IRule
    {
        public string Name { get; init; }
        public int Id { get; init; }
        public int Precedence { get; init; }
        public AnnotationPruning Prune { get; init; }

        public EmptyRule(int id, string name = "DummyRule", int precedence = 0, AnnotationPruning product = AnnotationPruning.None)
        {
            Id = id;
            Name = name;
            Precedence = precedence;
            Prune = product;
        }

        public object Clone() => MemberwiseClone();
    }
}
