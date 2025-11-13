// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using System.Collections.Immutable;

namespace gg.parse.tests
{
    public static class TestAnnotation
    {
        public static Annotation NewAnnotation(string name, int rangeStart, int rangeLength, params Annotation[] children) =>
            new(new EmptyRule(name), new util.Range(rangeStart, rangeLength), [.. children]);

        public static Annotation NewAnnotation(string name, int rangeStart, int rangeLength) =>
            new (new EmptyRule(name), new util.Range(rangeStart, rangeLength));

        public static Annotation NewAnnotation(int rangeStart, int rangeLength) =>
            NewAnnotation("empty_rule", rangeStart, rangeLength);
    }

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
