using System;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace gg.parse.argparser
{
    public static class ParseInstance
    {
        public static object OfValue<T>(Type targetType, Annotation annotation, List<Annotation> tokenList, string text)
        {
            if (targetType.IsArray)
            {
                return OfArray<T>(targetType, annotation, tokenList, text);
            }
            else if (targetType.IsGenericType)
            {
                var interfaces = targetType.GetInterfaces();

                if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                {
                    return OfDictionary<T>(targetType, annotation, tokenList, text);
                }
                else if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
                {
                    return OfList<T>(targetType, annotation, tokenList, text);
                }

                throw new NotImplementedException($"No backing implementation for type {targetType}.");
            }
            else if (targetType != typeof(string) && targetType.IsClass)
            {
                return OfObject<T>(targetType, annotation, tokenList, text);
            }
            else
            {
                return OfBasicValue(targetType, annotation.GetText(text, tokenList));
            }
        }

        public static Array OfArray<T>(Type targetType, Annotation annotation, List<Annotation> tokenList, string text)
        {
            var arrayType = targetType.GetElementType();

            if (annotation == ArgParserNames.Array)
            {
                // remove the start and end of the array from the count
                var result = Array.CreateInstance(arrayType, annotation.Count-2);

                // need to skip start and end, so start at 1 and end at -1 
                for (var i = 1; i < annotation.Count - 1; i++)
                {
                    result.SetValue(OfValue<T>(arrayType, annotation[i], tokenList, text), i-1);
                }

                return result;
            }

            throw new ArgumentException($"Request Array<{arrayType}> but provided value is not a valid array.");
        }

        public static IList OfList<T>(Type targetType, Annotation annotation, List<Annotation> tokenList, string text)
        {
            var listType = targetType.GetGenericArguments()[0];

            if (annotation == ArgParserNames.Array)
            {
                var genericType = typeof(List<>).MakeGenericType(listType);
                var result = (IList)Activator.CreateInstance(genericType)
                    ?? throw new ArgumentException($"Can't create an instance of list with list type <{listType}>.");
                
                // need to skip start and end, so start at 1 and end at -1 
                for (var i = 1; i < annotation.Count - 1; i++)
                {
                    result.Add(OfValue<T>(listType, annotation[i], tokenList, text));
                }

                return result;
            }

            throw new ArgumentException($"Request Array<{listType}> but provided value is not a valid array.");
        }

        public static IDictionary OfDictionary<T>(Type targetType, Annotation annotation, List<Annotation> tokenList, string text)
        {
            var keyType = targetType.GetGenericArguments()[0];
            var valueType = targetType.GetGenericArguments()[1];

            if (annotation == ArgParserNames.Dictionary)
            {
                var genericType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                var result = (IDictionary) Activator.CreateInstance(genericType) 
                    ?? throw new ArgumentException($"Can't create an instance of dictionary with key/value type <{keyType}, {valueType}>.");

                // need to skip scope start and end, so start at 1 and end at -1
                for (var i = 1; i < annotation.Count - 1; i++)
                {
                    var key = OfValue<T>(keyType, annotation[i][0], tokenList, text);
                    var value = OfValue<T>(valueType, annotation[i][1], tokenList, text);

                    result.Add(key, value);
                }

                return result;
            }

            throw new ArgumentException($"Request Dictionary<{keyType}, {valueType}> but provided value is not a valid dictionary of those types.");
        }

        public static object OfObject<T>(Type targetType, Annotation annotation, List<Annotation> tokenList, string text)
        {
            if (annotation == ArgParserNames.Dictionary)
            {
                var result = Activator.CreateInstance(targetType)
                    ?? throw new ArgumentException($"Can't create an instance of object type <{targetType}>.");

                // need to skip scope start and end, so start at 1 and end at -1
                for (var i = 1; i < annotation.Count - 1; i++)
                {
                    // key needs to be an identifier
                    var key      = annotation[i][0].GetText(text, tokenList);
                    var property = targetType.GetProperty(key);
                    var value = OfValue<T>(property.PropertyType, annotation[i][1], tokenList, text);

                    property.SetValue(result, value);
                }
                

                return result;
            }

            throw new ArgumentException($"Request object of {targetType} but provided value is not a object.");
        }

        public static object OfBasicValue(Type targetType, string text)
        {
            if (targetType == typeof(bool))
            {
                return bool.Parse(text);
            }
            else if (targetType == typeof(string))
            {
                if (text.StartsWith('"') || text.StartsWith("'"))
                {
                    return text.Substring(1, text.Length - 2);
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
