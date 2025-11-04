// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.util;
using System;
using System.Collections;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;


namespace gg.parse.argparser
{
    public enum PropertiesFormat
    {
        Default,
        Json
    }

    public readonly struct PropertiesConfig
    {
        public bool AddMetaInformation
        {
            get;
            init;
        }

        public PropertiesFormat Format 
        { 
            get; 
            init; 
        }

        public string Indent 
        { 
            get; 
            init; 
        }

        public int IndentCount 
        { 
            get; 
            init; 
        }

        public PropertiesConfig()
        {
            Indent = "    ";
            Format = PropertiesFormat.Default;
            AddMetaInformation = true;
        }

        public PropertiesConfig(PropertiesFormat format = PropertiesFormat.Default, string indent = "    ", bool addMetaInfo = true)
        {
            Indent = indent;
            Format = format;
            AddMetaInformation = addMetaInfo;
        }

        public static PropertiesConfig operator +(PropertiesConfig left, int count)
        {
            return new PropertiesConfig(left.Format, left.Indent, left.AddMetaInformation)
            {
                IndentCount = left.IndentCount + count
            };
        }
    }

    public static class ParseInstance
    {
        public static readonly string MetaInfoKey = "__meta_information";

        public static object OfValue(Type targetType, Annotation annotation, ImmutableList<Annotation> tokenList, string text)
        {
            if (targetType.IsArray)
            {
                return OfArray(targetType, annotation, tokenList, text);
            }
            else if (targetType.IsGenericType)
            {
                var interfaces = targetType.GetInterfaces();

                if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                {
                    return OfDictionary(targetType, annotation, tokenList, text);
                }
                else if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
                {
                    return OfList(targetType, annotation, tokenList, text);
                }
                else if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>)))
                {
                    return OfSet(targetType, annotation, tokenList, text);
                }

                throw new NotImplementedException($"No backing implementation for type {targetType}.");
            }
            else if (targetType != typeof(string) && targetType.IsClass)
            {
                return OfObject(targetType, annotation, tokenList, text);
            }
            else if (targetType.IsValueType && !targetType.IsEnum && !targetType.IsPrimitive)
            {
                // treat structs as objects
                return OfObject(targetType, annotation, tokenList, text);
            }
            else
            {
                return OfBasicValue(targetType, annotation.GetText(text, tokenList));
            }
        }

        public static Array OfArray(Type targetType, Annotation annotation, ImmutableList<Annotation> tokenList, string text)
        {
            var arrayType = targetType.GetElementType();

            if (arrayType != null && annotation == ArgParserNames.Array)
            {
                // remove the start and end of the array from the count
                var result = Array.CreateInstance(arrayType, annotation.Count - 2);

                // need to skip the array start and end, so start at 1 and end at -1 
                for (var i = 1; i < annotation.Count - 1; i++)
                {
                    result.SetValue(OfValue(arrayType, annotation[i]!, tokenList, text), i - 1);
                }

                return result;
            }

            throw arrayType == null 
                ? new ArgumentException($"Request Array but provided target type ({targetType}) is not contain an element type.")
                : new ArgumentException($"Request Array<{arrayType}> but provided value is not a valid array.");
        }

        public static IList OfList(Type targetType, Annotation annotation, ImmutableList<Annotation> tokenList, string text)
        {
            Assertions.RequiresNotNull(targetType);
            Assertions.RequiresNotNull(annotation);           

            var listType = targetType.GetGenericArguments()[0];

            if (annotation == ArgParserNames.Array)
            {
                var genericType = typeof(List<>).MakeGenericType(listType);
                var result = Activator.CreateInstance(genericType) as IList
                    ?? throw new ArgumentException($"Can't create an instance of list with list type <{listType}>.");
                
                // need to skip start and end, so start at 1 and end at -1 
                for (var i = 1; i < annotation.Count - 1; i++)
                {
                    result.Add(OfValue(listType, annotation[i]!, tokenList, text));
                }

                return result;
            }

            throw new ArgumentException($"Request Array<{listType}> but provided value is not a valid array.");
        }

        public static object OfSet(Type targetType, Annotation annotation, ImmutableList<Annotation> tokenList, string text)
        {
            var setType = targetType.GetGenericArguments()[0];

            if (annotation == ArgParserNames.Array)
            {
                var genericType = typeof(HashSet<>).MakeGenericType(setType);
                var result = Activator.CreateInstance(genericType)
                    ?? throw new ArgumentException($"Can't create an instance of list with list type <{setType}>.");
                var addMethod = genericType.GetMethod("Add");

                // need to skip start and end, so start at 1 and end at -1 
                for (var i = 1; i < annotation.Count - 1; i++)
                {
                    addMethod!.Invoke(result, [OfValue(setType, annotation[i]!, tokenList, text)]);
                }

                return result;
            }

            throw new ArgumentException($"Request Set<{setType}> but provided value is not a valid set (must be defined as an array).");
        }

        public static IDictionary OfDictionary(Type targetType, Annotation annotation, ImmutableList<Annotation> tokenList, string text)
        {
            Assertions.RequiresNotNull(targetType);
            Assertions.RequiresNotNull(annotation);

            var keyType = targetType.GetGenericArguments()[0];
            var valueType = targetType.GetGenericArguments()[1];

            if (annotation == ArgParserNames.Dictionary)
            {
                var genericType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                var result = Activator.CreateInstance(genericType) as IDictionary
                    ?? throw new ArgumentException($"Can't create an instance of dictionary with key/value type <{keyType}, {valueType}>.");

                // need to skip scope start and end, so start at 1 and end at -1
                for (var i = 1; i < annotation.Count - 1; i++)
                {
                    var key = OfValue(keyType, annotation[i]![0]!, tokenList, text);
                    var value = OfValue(valueType, annotation[i]![1]!, tokenList, text);

                    result.Add(key, value);
                }

                return result;
            }

            throw new ArgumentException($"Request Dictionary<{keyType}, {valueType}> but provided value is not a valid dictionary of those types.");
        }

        private static string KeyToPropertyName(Annotation node, ImmutableList<Annotation> tokenList, string text)
        {
            var keyString = node.GetText(text, tokenList);

            // can be a string in case of a json format
            if (node == ArgParserNames.String)
            {
                return keyString.Substring(1, keyString.Length - 2);
            }

            return keyString;
        }

        public static object OfObject(
            Type targetType, 
            Annotation annotation, 
            ImmutableList<Annotation> tokenList, 
            string text)
        {
            if (annotation == ArgParserNames.Dictionary)
            {
                var result = Activator.CreateInstance(targetType)
                    ?? throw new ArgumentException($"Can't create an instance of object type <{targetType}>.");

                // need to skip scope start and end, so start at 1 and end at -1
                for (var i = 1; i < annotation.Count - 1; i++)
                {
                    Assertions.RequiresNotNull(annotation[i]);

                    var key      = KeyToPropertyName(annotation[i]![0]!, tokenList, text);

                    if (key == MetaInfoKey) continue;

                    var property = targetType.GetProperty(key);

                    if (property != null)
                    {
                        var value = OfValue(property.PropertyType, annotation[i]![1]!, tokenList, text);
                        property.SetValue(result, value);
                    }
                    else
                    {
                        var field = targetType.GetField(key);

                        if (field != null)
                        {
                            Assertions.Requires(annotation[i]!.Count >= 2);

                            var value = OfValue(field.FieldType, annotation[i]![1]!, tokenList, text);
                            field.SetValue(result, value);
                        }
                        else
                        {
                            throw new ArgumentException($"No field or property found for {key}.");
                        }
                    }   
                }

                return result;
            }

            throw new ArgumentException($"Looking for a value of type '{targetType}' but value found is not a object/dictionary but a '{annotation.Rule.Name}'.");
        }

        public static object OfBasicValue(Type targetType, string text)
        {
            if (targetType == typeof(bool))
            {
                return bool.Parse(text);
            }
            else if (targetType == typeof(string))
            {
                if (text.StartsWith('"') || text.StartsWith('\''))
                {
#pragma warning disable IDE0057 // Use range operator
                    return text.Substring(1, text.Length - 2);
#pragma warning restore IDE0057 // Use range operator
                }

                return text;
            }
            else if (targetType == typeof(int))
            {
                return int.Parse(text);
            }
            else if (targetType == typeof(double))
            {
                return double.Parse(text, CultureInfo.InvariantCulture);
            }
            else if (targetType == typeof(float))
            {
                return float.Parse(text, CultureInfo.InvariantCulture);
            }
            else
            {
                throw new NotImplementedException($"No backing implementation to parse type {targetType}.");
            }
        }

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
                    AppendArray(builder, (Array)value, in context);
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
                    }
                    else if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>)))
                    {
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

        public static void AppendArray(this StringBuilder builder, Array a, in PropertiesConfig context)
        {
            if (a == null)
            {
                builder.Indent(in context).Append("null");
            }
            else
            {
                builder.Append('[');

                for (var i = 0; i < a.Length; i++)
                {
                    AppendValue(builder, a.GetValue(i), in context);

                    if (i < a.Length - 1)
                    {
                        builder.Append(", ");
                    }
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
