// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;

using gg.parse.core;
using gg.parse.script.common;
using gg.parse.script.parser;
using gg.parse.util;

namespace gg.parse.script.compiler
{
    public static class CompileContextExtensions
    {
        public static string GetDelimitedStringValue(this CompileContext context, Annotation annotation) =>
            context.GetText(annotation)[1 .. ^1];
    }
}
