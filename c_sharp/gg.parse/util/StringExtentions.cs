// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Text;
using System.Text.RegularExpressions;

using gg.parse.core;

namespace gg.parse.util
{
    public static class StringExtentions
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

        public static string SplitOnCapitals(
            this string input,
            char splitCharacter = '_',
            bool toLowerCase = false
        )
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var current = new StringBuilder();

            for (var i = 0; i < input.Length; i++)
            {
                var ch = input[i];
                var c = toLowerCase ? char.ToLower(ch) : ch;

                if (char.IsUpper(ch) && current.Length > 0)
                {
                    current.Append(splitCharacter);
                    current.Append(c);
                }
                else
                {
                    current.Append(c);
                }
            }

            return current.ToString();
        }

    }
}
