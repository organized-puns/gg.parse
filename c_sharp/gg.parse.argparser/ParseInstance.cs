using System.Collections;
using System.Globalization;

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
                if (targetType.GetInterfaces().Any( i =>
                        i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                {
                    return OfDictionary<T>(targetType, annotation, tokenList, text);
                }

                throw new NotImplementedException($"No backing implementation for type {targetType}.");
            }
            else
            {
                return OfBasicValue(targetType, annotation.GetText(text, tokenList));
            }
        }

        public static Array OfArray<T>(Type targetType, Annotation annotation, List<Annotation> tokenList, string text)
        {
            var arrayType = targetType.GetElementType();

            if (annotation[0] == ArgParserNames.Array)
            {
                var result = Array.CreateInstance(arrayType, annotation[0].Count);
                var index = 0;

                foreach (var element in annotation[0])
                {
                    result.SetValue(OfValue<T>(arrayType, element, tokenList, text), index);
                    index++;
                }

                return result;
            }

            throw new ArgumentException($"Request Array<{arrayType}> but provided value is not a valid array.");
        }

        public static IDictionary OfDictionary<T>(Type targetType, Annotation annotation, List<Annotation> tokenList, string text)
        {
            var keyType = targetType.GetGenericArguments()[0];
            var valueType = targetType.GetGenericArguments()[1];

            if (annotation[0] == ArgParserNames.Dictionary)
            {
                var genericType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                var result = (IDictionary)Activator.CreateInstance(genericType);

                foreach (var element in annotation[0])
                {
                    var key = OfValue<T>(keyType, element[0], tokenList, text);
                    var value = OfValue<T>(valueType, element[1], tokenList, text);

                    result.Add(key, value);
                }

                return result;
            }

            throw new ArgumentException($"Request Dictionary<{keyType}, {valueType}> but provided value is not a valid dictionary of those types.");
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
