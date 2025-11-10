// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.properties
{
    public static class TypeExtensions
    {
        public static bool IsDictionary(this Type t) =>

            t.IsGenericType
                && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

        public static bool IsObjectClass(this Type t) =>
            t.IsClass && t != typeof(string);

        public static bool IsStruct(this Type t) =>
            t.IsValueType && !t.IsEnum && !t.IsPrimitive;

    }
}
