// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using System.Text.RegularExpressions;

namespace gg.parse.util
{
    public static class StringExtenions
    {
        public static string Substring(this string str, Annotation annotation) => 
            str.Substring(annotation.Range);

        public static string Substring(this string str, Range range) => 
            str.Substring(range.Start, range.Length);

        // can't use regex escape as it will also escape characters
        // like '.'
        public static string SimpleEscape(this string str) =>
            str.Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\t", "\\t")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");

        public static string SimpleUnescape(this string str) =>
            Regex.Unescape(str);

    }
}
