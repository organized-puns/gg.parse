// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections;
using System.Collections.Immutable;

namespace gg.parse.core
{
    public readonly struct ParseResult(bool isSuccess, int dataRead, ImmutableList<Annotation>? annotations = null) : IEnumerable<Annotation>
    {
        public static implicit operator bool(ParseResult result) => result.FoundMatch;

        public static readonly ParseResult Success = new(true, 0, null);
        public static readonly ParseResult Unknown = new(true, -1, null);
        public static readonly ParseResult Failure = new(false, 0, null);

        public bool FoundMatch { get; init; } = isSuccess;

        public int MatchLength { get; init; } = dataRead;

        public ImmutableList<Annotation>? Annotations { get; init; } = annotations;

        public Annotation? this [int index] => Annotations?[index];

        public Annotation? this[string name] => Annotations?.FirstOrDefaultDfs(a => a.Rule != null && a.Rule.Name == name);   

        public int Count => Annotations?.Count ?? 0;

        public void Deconstruct(out bool isSuccess, out int matchedLength, out ImmutableList<Annotation>? annotations)
        {
            isSuccess = FoundMatch;
            matchedLength = MatchLength;
            annotations = Annotations;
        }

        public int[] CollectRuleIds() => Annotations == null ? [] : [.. Annotations.Select(a => a.Rule.Id)];

        public override string ToString()
        {
            if (FoundMatch)
            {
                return $"match found, length={MatchLength}, count={(Annotations == null ? 0 : Annotations.Count)}";
            }
            else
            {
                return $"no match, length={MatchLength}";
            }
        }

        public IEnumerator<Annotation> GetEnumerator()
        {
            return Annotations != null
                    ? Annotations.GetEnumerator()
                    : Enumerable.Empty<Annotation>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }       
    }
}
