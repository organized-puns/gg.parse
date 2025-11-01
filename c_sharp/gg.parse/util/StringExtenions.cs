// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;

namespace gg.parse.util
{
    public static class StringExtenions
    {
        public static string Substring(this string str, Annotation annotation) => 
            str.Substring(annotation.Range);

        public static string Substring(this string str, Range range) => 
            str.Substring(range.Start, range.Length);

    }
}
