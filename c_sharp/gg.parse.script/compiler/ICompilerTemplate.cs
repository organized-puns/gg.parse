// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using System.Collections.Immutable;

namespace gg.parse.script.compiler
{
    public interface ICompilerTemplate<TContext> where TContext: CompileContext
    {
        ICollection<T> Compile<T>(
            Type targetType,
            ImmutableList<Annotation> annotations,
            TContext context,
            ICollection<T> container
        );

        ICompilerTemplate<TContext> RegisterDefaultFunctions();

        T? Compile<T>(Annotation annotation, TContext context);

        object? Compile(Type? targetType, Annotation annotation, TContext context);
    }
}
