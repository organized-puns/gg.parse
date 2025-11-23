// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;

namespace gg.parse.properties
{
    public static class AnnotationExtensions
    {
        public static string KeyToPropertyName(this Annotation node, string text) =>
            // can be a string in case of a json format
            node == PropertiesNames.String
                ? text[1..^1]
                // else it's an identifier which has no quotes
                : text;
    }
}
