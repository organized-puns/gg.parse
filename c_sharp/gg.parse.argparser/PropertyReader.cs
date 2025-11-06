// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.properties;
using gg.parse.util;
using System;
using System.Collections;
using System.Collections.Immutable;
using System.Globalization;


namespace gg.parse.argparser
{
    /// <summary>
    /// Methods to read values to a target type given a syntax tree, tokens and inputtext
    /// </summary>
    public static class PropertyReader
    {
        public static object? Interpret(
            Annotation annotation,
            ImmutableList<Annotation> tokenList,
            string text
        )
        {
            switch (annotation.Rule.Name)
            {
                case PropertyFileNames.Dictionary:
                {
                    var metaInformationNode = MetaInformation.FindMetaInformation(annotation, tokenList, text);

                    return metaInformationNode == null
                        ? InterpretDictionary(annotation, tokenList, text)
                        : OfObject(metaInformationNode.ResolveObjectType(), annotation, tokenList, text);
                }

                case PropertyFileNames.KvpList:
                {
                    var metaInformationNode = MetaInformation.FindMetaInformation(annotation, tokenList, text);

                    return metaInformationNode == null
                        ? InterpretKvpList(annotation, tokenList, text)
                        : OfKeyValuePairList(metaInformationNode.ResolveObjectType(), annotation, tokenList, text);
                }

                case PropertyFileNames.Array:
                {
                    return InterpretArray(annotation, tokenList, text);
                }

                case PropertyFileNames.Int:
                    return int.Parse(annotation.GetText(text, tokenList));
                case PropertyFileNames.Float:
                    return float.Parse(annotation.GetText(text, tokenList));
                case PropertyFileNames.Boolean:
                    return bool.Parse(annotation.GetText(text, tokenList));
                case PropertyFileNames.String:
                    var str = annotation.GetText(text, tokenList);
                    return str.Substring(1, str.Length - 2);
                case PropertyFileNames.Identifier:
                    return annotation.GetText(text, tokenList);
                case PropertyFileNames.Null:
                    return null;
            }

            throw new ArgumentException($"Cannot process annotation with rule '{annotation.Rule.Name}'.");
        }

        public static object? InterpretArray(
            Annotation annotation,
            ImmutableList<Annotation> tokenList,
            string text
        )
        {
            if (annotation.Children == null || annotation.Children.Count <= 2)
            {
                return null;
            }

            var values = new object?[annotation.Count - 2];
            Type? valueType = null;

            // capture all keys and values and try to guess the type
            // skip the first and last child of this annotation because those should
            // be the delimiters
            for (var i = 1; i < annotation.Count - 1; i++)
            {
                var value = Interpret(annotation[i]!, tokenList, text);

                values[i-1] = value;

                if (value != null)
                {
                    if (valueType == null)
                    {
                        valueType = value.GetType();
                    }
                    else if (valueType != typeof(object) && value.GetType() != valueType)
                    {
                        // multiple types in the dictionary so settle for the object class
                        valueType = typeof(object);
                    }
                }
            }

            if (valueType == null)
            {
                // default to object in case the entire dictionary consists of null values
                valueType = typeof(object);
            }

            var result = Array.CreateInstance(valueType, values.Length);

            Array.Copy(values, result, values.Length);

            return result;
        }


        public static object? InterpretDictionary(
            Annotation annotation,
            ImmutableList<Annotation> tokenList,
            string text
        )
        {
            if (annotation.Children == null || annotation.Children.Count == 0)
            {
                return null;
            }

            var keys = new List<object?>();
            var values = new List<object?>();
            Type? keyType = null;
            Type? valueType = null;

            // capture all keys and values and try to guess the type
            // skip the first and last child of this annotation because those should
            // be the delimiters
            for (var i = 1; i < annotation.Count - 1; i++)
            {
                var key = Interpret(annotation[i]![0]!, tokenList, text);
                var value = Interpret(annotation[i]![1]!, tokenList, text);
                
                keys.Add(key);
                values.Add(value);

                Assertions.RequiresNotNull(key);

                if (keyType == null)
                {
                    keyType = key.GetType();
                }
                else if (keyType != typeof(object) && key.GetType() != keyType)
                {
                    // multiple types in the dictionary so settle for the object class
                    keyType = typeof(object);
                }

                if (value != null)
                {
                    if (valueType == null)
                    {
                        valueType = value.GetType();
                    }
                    else if (valueType != typeof(object) && value.GetType() != valueType)
                    {
                        // multiple types in the dictionary so settle for the object class
                        valueType = typeof(object);
                    }
                }
            }

            if (valueType == null)
            {
                // default to object in case the entire dictionary consists of null values
                valueType = typeof(object);
            }

            // create the resulting dictionary and return the results
            var genericType = typeof(Dictionary<,>).MakeGenericType(keyType!, valueType);
            var result = Activator.CreateInstance(genericType) as IDictionary
                ?? throw new ArgumentException($"Can't create an instance of dictionary with key/value type <{keyType}, {valueType}>.");

            for (var i = 0; i < keys.Count; i++)
            {
                result.Add(keys[i]!, values[i]);
            }

            return result;
        }
                
        public static object? InterpretKvpList(
            Annotation annotation,
            ImmutableList<Annotation> tokenList,
            string text
        )
        {
            if (annotation.Children == null || annotation.Children.Count == 0)
            {
                return null;
            }

            var result = new Dictionary<string, object?>();

            foreach (var kvpNode in annotation)
            {
                var key = KeyToPropertyName(kvpNode[0]!, tokenList, text);
                var value = Interpret(kvpNode[1]!, tokenList, text);

                result[key] = value;
            }

            return result;
        }

        public static T? OfValue<T>(
            Annotation annotation,
            ImmutableList<Annotation> tokenList,
            string text
        )
        {
            if ( annotation == PropertyFileNames.Dictionary)
            {
                return (T?) OfValue(typeof(T), annotation, tokenList, text);
            }
            else if (annotation == PropertyFileNames.KvpList)
            {
                return (T?)OfKeyValuePairList(typeof(T), annotation, tokenList, text);
            }

            throw new ArgumentException($"Expected the annotation to be either '{PropertyFileNames.Dictionary}' or '{PropertyFileNames.KvpList}', but found '{annotation.Rule.Name}'.");
        }

        public static object? OfValue(
            Type targetType, 
            Annotation annotation, 
            ImmutableList<Annotation> tokenList, 
            string text)
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

            if (arrayType != null && annotation == PropertyFileNames.Array)
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
            else if (annotation == PropertyFileNames.Null)
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

            if (annotation == PropertyFileNames.Array)
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
            else if (annotation == PropertyFileNames.Null)
            {
                return null;
            }
            
            throw new ArgumentException($"Request Array<{listType}> but provided value is not a valid array.");
        }

        public static object? OfSet(Type targetType, Annotation annotation, ImmutableList<Annotation> tokenList, string text)
        {
            var setType = targetType.GetGenericArguments()[0];

            if (annotation == PropertyFileNames.Array)
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
            else if (annotation == PropertyFileNames.Null)
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

            if (annotation == PropertyFileNames.Dictionary)
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
            else if (annotation == PropertyFileNames.Null)
            {
                return null;
            }

            throw new ArgumentException($"Request Dictionary<{keyType}, {valueType}> but provided value is not a valid dictionary of those types.");
        }

        private static string KeyToPropertyName(Annotation node, ImmutableList<Annotation> tokenList, string text)
        {
            var keyString = node.GetText(text, tokenList);

            // can be a string in case of a json format
            if (node == PropertyFileNames.String)
            {
                return keyString.Substring(1, keyString.Length - 2);
            }

            return keyString;
        }

        public static object? OfKeyValuePairList(
            Type targetType,
            Annotation annotation,
            ImmutableList<Annotation> tokenList,
            string text)
        {
            var result = Activator.CreateInstance(targetType)
                    ?? throw new ArgumentException($"Can't create an instance of object type <{targetType}>.");

            for (var i = 0; i < annotation.Count; i++)
            {
                var kvp = annotation[i];

                Assertions.RequiresNotNull(kvp);
                Assertions.Requires(kvp == PropertyFileNames.KvpPair);
                Assertions.Requires(kvp.Count >= 2);

                // xxx note we don't care if this fails, but ideally we should issue a warning
                TrySetObjectProperty(result, kvp, tokenList, text);
            }

            return result;
        }

        public static object? OfObject(
            Type targetType,
            Annotation annotation,
            ImmutableList<Annotation> tokenList,
            string text)
        {
            if (annotation == PropertyFileNames.Dictionary)
            {
                var result = Activator.CreateInstance(targetType)
                    ?? throw new ArgumentException($"Can't create an instance of object type <{targetType}>.");

                // need to skip scope start and end, so start at 1 and end at -1
                for (var i = 1; i < annotation.Count - 1; i++)
                {
                    var kvp = annotation[i];

                    Assertions.RequiresNotNull(kvp);
                    Assertions.Requires(kvp == PropertyFileNames.KvpPair);
                    Assertions.Requires(kvp.Count >= 2);

                    // xxx note we don't care if this fails, but ideally we should issue a warning
                    TrySetObjectProperty(result, kvp, tokenList, text);
                }

                return result;
            }
            else if (annotation == PropertyFileNames.Null)
            {
                return null;
            }

            throw new ArgumentException($"Looking for a value of type '{targetType}' but value found is not a object/dictionary but a '{annotation.Rule.Name}'.");
        }

        private static bool TrySetObjectProperty(
            object target,
            Annotation annotation,
            ImmutableList<Annotation> tokenList,
            string text)
        {
            var key = KeyToPropertyName(annotation[0]!, tokenList, text);

            var property = target.GetType().GetProperty(key);

            if (property != null)
            {
                property.SetValue(target, OfValue(property.PropertyType, annotation[1]!, tokenList, text));
                return true;
            }
            else
            {
                var field = target.GetType().GetField(key);

                if (field != null)
                {
                    field.SetValue(target, OfValue(field.FieldType, annotation[1]!, tokenList, text));
                    return true;
                }
            }

            return false;
        }

        public static object? OfBasicValue(Type targetType, Annotation annotation, string text)
        {
            if (targetType == typeof(bool))
            {
                return bool.Parse(text);
            }
            else if (targetType == typeof(string))
            {
                if (annotation == PropertyFileNames.String)
                {
                    if (text.StartsWith('"') || text.StartsWith('\''))
                    {
#pragma warning disable IDE0057 // Use range operator
                        return text.Substring(1, text.Length - 2);
#pragma warning restore IDE0057 // Use range operator
                    }
                }
                else if (annotation == PropertyFileNames.Null)
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
