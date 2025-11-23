// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections;

namespace gg.parse.properties
{
    public static class TypeExtensions
    {
        public static bool IsDictionary(this Type t) =>

            t.IsGenericType
                && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

        public static bool IsList(this Type t) =>

            t.IsGenericType
                && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));

        public static bool IsSet(this Type t) =>

            t.IsGenericType
                && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>));

        public static bool IsClass(this Type type)
        {
            // Exclude primitives, value types, and strings
            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type.IsEnum)
                return false;

            // Exclude nullable types
            if (Nullable.GetUnderlyingType(type) != null)
                return false;

            // Exclude common collection interfaces and types
            if (typeof(IDictionary).IsAssignableFrom(type) ||
                typeof(IList).IsAssignableFrom(type) ||
                typeof(ICollection).IsAssignableFrom(type) ||
                type.IsArray)
                return false;

            // Check if it's a class (reference type) and not an anonymous type
            if (type.IsClass && !type.Name.Contains("AnonymousType"))
                return true;

            return false;
        }

        public static bool IsStruct(this Type t) =>
            t.IsValueType && !t.IsEnum && !t.IsPrimitive;

    }
}
