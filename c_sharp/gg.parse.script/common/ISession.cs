// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;

using gg.parse.core;

namespace gg.parse.script.common
{
    public interface ISession
    {
        ImmutableList<Annotation>? Tokens { get; }

        ImmutableList<Annotation>? SyntaxTree { get; }

        string GetText(Annotation annotation);
    }
}
