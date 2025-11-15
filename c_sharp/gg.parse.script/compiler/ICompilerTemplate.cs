// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using System.Collections.Immutable;

namespace gg.parse.script.compiler
{
    public interface ICompilerTemplate<TContext> where TContext: CompileContext
    {
        ICompileOutputCollection Compile(
            Type targetType,
            ImmutableList<Annotation> annotations,
            TContext context,
            ICompileOutputCollection container
        );

        ICompilerTemplate<TContext> RegisterDefaultFunctions();

        T? Compile<T>(Annotation annotation, TContext context);

        object? Compile(Type? targetType, Annotation annotation, TContext context);
    }
}
