// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.util;

namespace gg.parse.properties
{
    public class EnumProperty
    {
        public const string EnumSeparator = ".";
        public const string EnumKey = $"enum{EnumSeparator}";


        public static string ToText(object enumValue) =>
            $"{EnumKey}{enumValue.GetType().Name}{EnumSeparator}{enumValue}";

        public static bool IsEnum(string enumValue)
        {
            return !string.IsNullOrEmpty(enumValue) 
                    && enumValue.StartsWith(EnumKey);
        }

        public static T Parse<T>(string enumValue, TypePermissions permissions) =>
            (T) Parse(enumValue, permissions);

        public static object Parse(string enumValue, TypePermissions context)
        {
            Assertions.RequiresNotNull(context);
            Assertions.Requires(IsEnum(enumValue));

            var parts = enumValue.Split(EnumSeparator);

            return parts.Length == 3
                ? Enum.Parse(context.ResolveType(parts[1]), parts[2])
                : throw new PropertiesException($"Enum property must start with '{EnumKey}' and contain three parts separated by '{EnumSeparator}'.");
        }
    }
}
