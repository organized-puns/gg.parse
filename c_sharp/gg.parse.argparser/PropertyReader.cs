// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections;
using System.Collections.Immutable;
using System.Globalization;

using gg.parse.core;
using gg.parse.util;

namespace gg.parse.argparser
{
    /// <summary>
    /// Methods to read values to a target type given a syntax tree, tokens and inputtext
    /// </summary>
    public static class PropertyReader
    {
        public static object? OfValue(Type targetType, Annotation annotation, ImmutableList<Annotation> tokenList, string text)
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
                return OfBasicValue(targetType, annotation, annotation.GetText(text, tokenList));
            }
        }

        public static Array? OfArray(Type targetType, Annotation annotation, ImmutableList<Annotation> tokenList, string text)
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
            else if (annotation == ArgParserNames.Null)
            {
                return null;
            }

                throw arrayType == null
                    ? new ArgumentException($"Request Array but provided target type ({targetType}) is not contain an element type.")
                    : new ArgumentException($"Request Array<{arrayType}> but provided value is not a valid array.");
        }

        public static IList? OfList(Type targetType, Annotation annotation, ImmutableList<Annotation> tokenList, string text)
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
            else if (annotation == ArgParserNames.Null)
            {
                return null;
            }
            
            throw new ArgumentException($"Request Array<{listType}> but provided value is not a valid array.");
        }

        public static object? OfSet(Type targetType, Annotation annotation, ImmutableList<Annotation> tokenList, string text)
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
            else if (annotation == ArgParserNames.Null)
            {
                return null;
            }

            throw new ArgumentException($"Request Set<{setType}> but provided value is not a valid set (must be defined as an array).");
        }

        public static IDictionary? OfDictionary(Type targetType, Annotation annotation, ImmutableList<Annotation> tokenList, string text)
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

                // may need to skip scope start and end, so start at 1 and end at -1
                for (var i = 1; i < annotation.Count - 1; i++)
                {
                    var key = OfValue(keyType, annotation[i]![0]!, tokenList, text);
                    var value = OfValue(valueType, annotation[i]![1]!, tokenList, text);

                    if (key != null)
                    {
                        result.Add(key, value);
                    }
                    else
                    {
                        throw new ArgumentException($"Can't create an instance of dictionary with a null key.");
                    }
                }

                return result;
            }
            else if (annotation == ArgParserNames.Null)
            {
                return null;
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

        public static object? OfObject(
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

                    var key = KeyToPropertyName(annotation[i]![0]!, tokenList, text);

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
                            // ignore values we can't map, should yield a warning xxx
                        }
                    }
                }

                return result;
            }
            else if (annotation == ArgParserNames.Null)
            {
                return null;
            }

            throw new ArgumentException($"Looking for a value of type '{targetType}' but value found is not a object/dictionary but a '{annotation.Rule.Name}'.");
        }

        public static object? OfBasicValue(Type targetType, Annotation annotation, string text)
        {
            if (targetType == typeof(bool))
            {
                return bool.Parse(text);
            }
            else if (targetType == typeof(string))
            {
                if (annotation == ArgParserNames.String)
                {
                    if (text.StartsWith('"') || text.StartsWith('\''))
                    {
#pragma warning disable IDE0057 // Use range operator
                        return text.Substring(1, text.Length - 2);
#pragma warning restore IDE0057 // Use range operator
                    }
                }
                else if (annotation == ArgParserNames.Null)
                {
                    return null;
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
    }
}
