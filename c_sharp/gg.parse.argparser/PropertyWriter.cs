// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace gg.parse.argparser
{
    public static class PropertyWriter
    {
        public static readonly string MetaInfoKey = "__meta_information";

        private static StringBuilder Indent(this StringBuilder builder, in PropertiesConfig context) =>
            Indent(builder, context.IndentCount, context.Indent);

        private static StringBuilder Indent(this StringBuilder builder, int indentLength, string indent)
        {
            for (var i = 0; i < indentLength; i++)
            {
                builder.Append(indent);
            }

            return builder;
        }

        public static StringBuilder AppendValue(this StringBuilder builder, object? value, in PropertiesConfig context)
        {
            if (value == null)
            {
                builder.Indent(in context).Append("null");
            }
            else
            {
                var targetType = value.GetType();

                if (targetType.IsArray)
                {
                    AppendListCompatbileEnumerable(builder, (IEnumerable) value, in context);
                }
                else if (targetType.IsGenericType)
                {
                    var interfaces = targetType.GetInterfaces();

                    if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                    {
                        AppendDictionary(builder, (IDictionary)value, in context);
                    }
                    else if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
                    {
                        AppendListCompatbileEnumerable(builder, (IEnumerable) value, in context);
                    }
                    else if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>)))
                    {
                        AppendListCompatbileEnumerable(builder, (IEnumerable) value, in context);
                    }
                    else
                    { 
                        throw new NotImplementedException($"No backing implementation for type {targetType}.");
                    }
                }
                else if (targetType != typeof(string) && targetType.IsClass)
                {
                    AppendClassOrStruct(builder, value, context);
                }
                else if (targetType.IsValueType && !targetType.IsEnum && !targetType.IsPrimitive)
                {
                    AppendClassOrStruct(builder, value, context);
                }
                else
                {
                    AppendBasicValue(builder, value);
                }
            }

            return builder;
        }

        public static void AppendListCompatbileEnumerable(this StringBuilder builder, IEnumerable enumeration, in PropertiesConfig context)
        {
            if (enumeration == null)
            {
                builder.Indent(in context).Append("null");
            }
            else
            {
                builder.Append('[');

                foreach (var value in enumeration)
                {
                    AppendValue(builder, value, in context);                 
                    builder.Append(", ");
                }

                // remove the last ,
                if (builder.Length > 1)
                {
                    builder.Remove(builder.Length - 2, 2);
                }

                builder.Append(']');
            }
        }

        public static void AppendDictionary(this StringBuilder builder, IDictionary dictionary, in PropertiesConfig context)
        {
            if (dictionary == null)
            {
                builder.Indent(in context).Append("null");
            }
            else
            {
                builder.Append("{\n");

                foreach (DictionaryEntry kv in dictionary)
                {
                    if (context.Format == PropertiesFormat.Default || kv.Key is string)
                    {
                        builder.Indent(context + 1).AppendValue(kv.Key, context + 1);
                    }
                    else
                    {
                        builder.Indent(context + 1).AppendValue($"\"{kv.Key}\"", context + 1);
                    }

                    builder.Append(": ");
                    builder.AppendValue(kv.Value, in context);

                    builder.Append(",\n");
                }

                // remove the last comma
                builder.Remove(builder.Length - 2, 2);

                builder.Append('\n').Indent(in context).Append('}');
            }
        }

        public static void AppendClassOrStruct(this StringBuilder builder, object value, in PropertiesConfig config)
        {
            if (value == null)
            {
                builder.Indent(in config).Append("null");
            }
            else
            {
                var properties = value
                    .GetType()
                    .GetProperties(
                        BindingFlags.Public | 
                        BindingFlags.Instance | 
                        BindingFlags.GetProperty | 
                        BindingFlags.SetProperty
                    );
                builder.Append("{\n");

                if (config.AddMetaInformation)
                {
                    var key = config.Format == PropertiesFormat.Default
                            ? MetaInfoKey
                            : $"\"{MetaInfoKey}\"";

                    builder.Indent(config + 1).Append($"{key}: \"{{type: {value.GetType().AssemblyQualifiedName}}}\"");

                    if (properties.Length > 0)
                    {
                        builder.Append(",\n");
                    }
                }

                foreach (var property in properties)
                {
                    if (config.Format == PropertiesFormat.Default)
                    { 
                        builder.Indent(config + 1).Append($"{property.Name}: ");
                    }
                    else if (config.Format == PropertiesFormat.Json)
                    {
                        builder.Indent(config + 1).Append($"\"{property.Name}\": ");
                    }

                    AppendValue(builder, property.GetValue(value), config + 1);

                    builder.Append(",\n");
                }

                // remove the last comma
                builder.Remove(builder.Length - 2, 2);

                builder.Append('\n').Indent(in config).Append('}');
            }
        }

        public static void AppendBasicValue(this StringBuilder builder, object? value)
        {
            if (value == null)
            {
                builder.Append("null");
            }
            else if (value is string str)
            {
                builder.Append('"' + Regex.Escape(str) + '"');
            }
            else
            {
                var result = value.ToString();
                builder.Append(result ?? "null");
            }
        }
    }
}
