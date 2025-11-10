// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;

using gg.parse.properties;
using gg.parse.util;

namespace gg.parse.properties
{
    public static class PropertyWriter
    {
        private static StringBuilder Indent(this StringBuilder builder, in PropertiesConfig config) =>
            builder.Indent(config.IndentCount, config.Indent);


        public static StringBuilder AppendAsKeyValuePairs<T>(this StringBuilder builder, T? value, in PropertiesConfig config)
            where T : class
        {
            if (value == null)
            {
                builder.Append(PropertiesTokens.Null);
            }
            else
            {
                var targetType = value.GetType();

                if (targetType.IsDictionary())
                { 
                    return AppendDictionary(builder, (IDictionary)value, config, false);
                }

                if (targetType.IsClass() || targetType.IsStruct())
                {
                    return AppendProperties(builder, value, config);
                }
                else
                {
                    throw new PropertiesException($"Value of type {targetType} has no backing implementation to decompose it into key/value pairs.");
                }
            }

            return builder;
        }

        public static StringBuilder AppendValue(this StringBuilder builder, object? value, in PropertiesConfig context)
        {
            if (value == null)
            {
                builder.Append(PropertiesTokens.Null);
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
                else if (targetType.IsClass())
                {
                    AppendClassOrStruct(builder, value, context);
                }
                else if (targetType.IsStruct())
                {
                    AppendClassOrStruct(builder, value, context);
                }
                else if (targetType.IsEnum)
                {
                    AppendEnum(builder, value, context);
                }
                else
                {
                    AppendBasicValue(builder, value);
                }
            }

            return builder;
        }

        public static StringBuilder AppendListCompatbileEnumerable(this StringBuilder builder, IEnumerable enumeration, in PropertiesConfig context)
        {
            if (enumeration == null)
            {
                builder.Append(PropertiesTokens.Null);
            }
            else
            {
                builder.Append(PropertiesTokens.ArrayStart[0]);

                foreach (var value in enumeration)
                {
                    AppendValue(builder, value, in context);                 
                    builder.Append($"{PropertiesTokens.ItemSeparator} ");
                }

                // remove the last ,
                if (builder.Length > 1)
                {
                    builder.Remove(builder.Length - 2, 2);
                }

                builder.Append(PropertiesTokens.ArrayEnd[0]);
            }

            return builder;
        }

        public static StringBuilder AppendDictionary(
            this StringBuilder builder, 
            IDictionary dictionary, 
            in PropertiesConfig context,
            bool addDelimiters = true)
        {
            if (dictionary == null)
            {
                builder.Append(PropertiesTokens.Null);
            }
            else
            {
                if (addDelimiters)
                {
                    builder.Append($"{PropertiesTokens.ScopeStart}\n");
                }

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

                    builder.Append($"{PropertiesTokens.KvSeparatorColon} ");
                    builder.AppendValue(kv.Value, in context);

                    builder.Append($"{PropertiesTokens.ItemSeparator}\n");
                }

                // remove the last comma
                builder.Remove(builder.Length - 2, 2);

                if (addDelimiters)
                {
                    builder.Append('\n').Indent(in context).Append(PropertiesTokens.ScopeEnd[0]);
                }
                else
                {
                    builder.Append('\n');
                }
            }

            return builder;
        }

        public static StringBuilder AppendClassOrStruct(this StringBuilder builder, object value, in PropertiesConfig config)
        {
            if (value == null)
            {
                builder.Append(PropertiesTokens.Null);
            }
            else
            {
                builder.Append($"{PropertiesTokens.ScopeStart}\n");

                AppendProperties(builder, value, config + 1);

                builder.Append('\n').Indent(in config).Append(PropertiesTokens.ScopeEnd[0]);
            }

            return builder;
        }

        private static StringBuilder AppendItemSeparator(this StringBuilder builder, in PropertiesConfig config, string defaultSeparator = "\n")
        {
            if (config.Format == PropertiesFormat.Default)
            {
                builder.Append(defaultSeparator);
            }
            else
            {
                builder.Append($"{PropertiesTokens.ItemSeparator}\n");
            }

            return builder;
        }

        private static StringBuilder AppendEnum(this StringBuilder builder, object value, in PropertiesConfig config)
        {
            if (config.Format == PropertiesFormat.Default)
            {
                builder.Append(EnumProperty.ToText(value));
            }
            else
            {
                builder.Append($"{EnumProperty.ToText(value)}\n");
            }

            return builder;
        }

        private static StringBuilder AppendProperties(this StringBuilder builder, object value, in PropertiesConfig config)
        {
            var properties = value
                .GetType()
                .GetProperties(
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.GetProperty |
                    BindingFlags.SetProperty
                );

            if (config.AddMetaInformation)
            {
                MetaInformation.AppendMetaInformation(builder, value, config);

                // don't add separators in the default format
                if (properties.Length > 0)
                {
                    builder.AppendItemSeparator(config);
                }
            }

            foreach (var property in properties)
            {
                if (config.Format == PropertiesFormat.Default)
                {
                    builder.Indent(config).Append($"{property.Name}{PropertiesTokens.KvSeparatorColon} ");
                }
                else if (config.Format == PropertiesFormat.Json)
                {
                    builder.Indent(config).Append($"\"{property.Name}\"{PropertiesTokens.KvSeparatorColon} ");
                }

                AppendValue(builder, property.GetValue(value), config);

                builder.AppendItemSeparator(config);
            }

            if (config.Format == PropertiesFormat.Default)
            {
                // remove the new line
                builder.Remove(builder.Length - 1, 1);
            }
            else
            { 
                // remove the last comma
                builder.Remove(builder.Length - 2, 2);
            }

            return builder;
        }

        public static StringBuilder AppendBasicValue(this StringBuilder builder, object? value)
        {
            if (value == null)
            {
                builder.Append(PropertiesTokens.Null);
            }
            else if (value is string str)
            {
                builder.Append('"' + str.SimpleEscape() + '"');
            }
            else if (value is bool b)
            {
                // c# boolean is compatible with json but not vice versa, so use this explicit
                // approach since we want to support both
                builder.Append(b ? PropertiesTokens.BoolTrue : PropertiesTokens.BoolFalse);
            }
            else if (value is float f)
            {
                builder.Append(f.ToString("0.0######", CultureInfo.InvariantCulture));
            }
            else if (value is double d)
            {
                builder.Append(d.ToString("0.0############", CultureInfo.InvariantCulture));
            }
            else
            {
                var result = value.ToString();
                builder.Append(result ?? PropertiesTokens.Null);
            }

            return builder;
        }
    }
}
