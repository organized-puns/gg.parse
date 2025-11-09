// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using gg.parse.properties;
using gg.parse.util;

namespace gg.parse.argparser
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
                builder.Append(PropertyFileTokens.Null);
            }
            else
            {
                AppendProperties(builder, value, config);
            }

            return builder;
        }

        public static StringBuilder AppendValue(this StringBuilder builder, object? value, in PropertiesConfig context)
        {
            if (value == null)
            {
                builder.Append(PropertyFileTokens.Null);
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

        public static StringBuilder AppendListCompatbileEnumerable(this StringBuilder builder, IEnumerable enumeration, in PropertiesConfig context)
        {
            if (enumeration == null)
            {
                builder.Append(PropertyFileTokens.Null);
            }
            else
            {
                builder.Append(PropertyFileTokens.ArrayStart[0]);

                foreach (var value in enumeration)
                {
                    AppendValue(builder, value, in context);                 
                    builder.Append($"{PropertyFileTokens.ItemSeparator} ");
                }

                // remove the last ,
                if (builder.Length > 1)
                {
                    builder.Remove(builder.Length - 2, 2);
                }

                builder.Append(PropertyFileTokens.ArrayEnd[0]);
            }

            return builder;
        }

        public static StringBuilder AppendDictionary(this StringBuilder builder, IDictionary dictionary, in PropertiesConfig context)
        {
            if (dictionary == null)
            {
                builder.Append(PropertyFileTokens.Null);
            }
            else
            {
                builder.Append($"{PropertyFileTokens.ScopeStart}\n");

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

                    builder.Append($"{PropertyFileTokens.KvSeparatorColon} ");
                    builder.AppendValue(kv.Value, in context);

                    builder.Append($"{PropertyFileTokens.ItemSeparator}\n");
                }

                // remove the last comma
                builder.Remove(builder.Length - 2, 2);

                builder.Append('\n').Indent(in context).Append(PropertyFileTokens.ScopeEnd[0]);
            }

            return builder;
        }

        public static StringBuilder AppendClassOrStruct(this StringBuilder builder, object value, in PropertiesConfig config)
        {
            if (value == null)
            {
                builder.Append(PropertyFileTokens.Null);
            }
            else
            {
                builder.Append($"{PropertyFileTokens.ScopeStart}\n");

                AppendProperties(builder, value, config + 1);

                builder.Append('\n').Indent(in config).Append(PropertyFileTokens.ScopeEnd[0]);
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
                builder.Append($"{PropertyFileTokens.ItemSeparator}\n");
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
                    builder.Indent(config).Append($"{property.Name}{PropertyFileTokens.KvSeparatorColon} ");
                }
                else if (config.Format == PropertiesFormat.Json)
                {
                    builder.Indent(config).Append($"\"{property.Name}\"{PropertyFileTokens.KvSeparatorColon} ");
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
                builder.Append(PropertyFileTokens.Null);
            }
            else if (value is string str)
            {
                builder.Append('"' + Regex.Escape(str) + '"');
            }
            else if (value is bool b)
            {
                // c# boolean is compatible with json but not vice versa, so use this explicit
                // approach since we want to support both
                builder.Append(b ? PropertyFileTokens.BoolTrue : PropertyFileTokens.BoolFalse);
            }
            else
            {
                var result = value.ToString();
                builder.Append(result ?? PropertyFileTokens.Null);
            }

            return builder;
        }
    }
}
