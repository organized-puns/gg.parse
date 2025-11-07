// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections;
using System.Globalization;

using gg.parse.core;
using gg.parse.properties;
using gg.parse.script.compiler;
using gg.parse.util;

namespace gg.parse.argparser
{
    /// <summary>
    /// Class trying to interpret the value of an annotation and create
    /// a c# value from the annotation.
    /// </summary>
    public sealed class PropertyInterpreter : CompilerTemplate<object>
    {
        public PropertyInterpreter()
        {
            Register(PropertyFileNames.Array, CompileArray);
            Register(PropertyFileNames.Boolean, CompileBoolean);
            Register(PropertyFileNames.Dictionary, CompileDictionaryOrObject);
            Register(PropertyFileNames.Float, CompileFloat);
            Register(PropertyFileNames.Identifier, CompileIdentifier);
            Register(PropertyFileNames.Int, CompileInt);
            Register(PropertyFileNames.KvpList, CompileKeyValueListOrObject);
            Register(PropertyFileNames.Null, CompileNull);
            Register(PropertyFileNames.String, CompileString);
        }

        public static object? CompileArray(Annotation annotation, CompileContext<object> context)
        {
            if (annotation.Children != null && annotation.Children.Count > 2)
            {
                Type? commonArrayType = null;
                var tempValues = new object?[annotation.Count - 2];

                // capture all keys and values and try to guess the type
                // skip the first and last child of this annotation because those should
                // be the delimiters
                for (var i = 1; i < annotation.Count - 1; i++)
                {
                    var child = annotation[i];
                    
                    Assertions.RequiresNotNull(child);

                    tempValues[i - 1] = context.Compile(child);
                    commonArrayType = GetCommonValueType(tempValues[i - 1], commonArrayType);
                }

                // default to object in case the entire dictionary consists of null values
                var result = Array.CreateInstance(commonArrayType ?? typeof(object), tempValues.Length);

                Array.Copy(tempValues, result, tempValues.Length);

                return result;
            }

            return null;
        }

        public static object? CompileBoolean(Annotation annotation, CompileContext<object> context) =>
            bool.Parse(context.GetText(annotation));


        public static object? CompileDictionaryOrObject(Annotation annotation, CompileContext<object> context)
        {
            var metaInformationNode = MetaInformation.FindMetaInformation(annotation, context.Tokens, context.Text);

            return metaInformationNode == null
                ? CompileDictionary(annotation, context)
                : PropertyReader.
                        OfObject(
                            metaInformationNode.ResolveObjectType(),
                            annotation,
                            context.Tokens,
                            context.Text
                        );
        }

        public static object? CompileDictionary(Annotation annotation, CompileContext<object> context)
        {
            // children count should be more than 2 as the first and last child
            // are expected to be delimiters
            if (annotation.Children != null && annotation.Children.Count > 2)
            {
                var keyValues = new List<(object? key, object? value)>();
                Type? keyType = null;
                Type? valueType = null;

                // capture all keys and values and try to guess the type
                // skip the first and last child of this annotation because those should
                // be the delimiters
                for (var i = 1; i < annotation.Count - 1; i++)
                {
                    (object? key, object? value) keyValuePair = 
                        (context.Compile(annotation[i]![0]!), context.Compile(annotation[i]![1]!));

                    keyValues.Add(keyValuePair);

                    keyType = GetCommonValueType(keyValuePair.key, keyType);
                    valueType = GetCommonValueType(keyValuePair.value, valueType);
                }

                // create the resulting dictionary and return the results
                var genericType = typeof(Dictionary<,>).MakeGenericType(keyType!, valueType ?? typeof(object));
                var result = 
                    Activator.CreateInstance(genericType) as IDictionary
                    ?? throw new CompilationException($"Can't create an instance of dictionary with key/value type <{keyType}, {valueType}>.");

                for (var i = 0; i < keyValues.Count; i++)
                {
                    result.Add(keyValues[i].key!, keyValues[i].value);
                }

                return result;
            }

            return null;
        }

        public static object? CompileFloat(Annotation annotation, CompileContext<object> context) =>
            float.Parse(context.GetText(annotation), CultureInfo.InvariantCulture);

        public static object? CompileIdentifier(Annotation annotation, CompileContext<object> context) =>
            context.GetText(annotation);

        public static object? CompileInt(Annotation annotation, CompileContext<object> context) =>
            int.Parse(context.GetText(annotation));

        public static object? CompileKeyValueListOrObject(Annotation annotation, CompileContext<object> context)
        {
            var metaInformationNode = MetaInformation.FindMetaInformation(annotation, context.Tokens, context.Text);

            return metaInformationNode == null
                ? CompileKeyValueList(annotation, context)
                : PropertyReader.
                        OfKeyValuePairList(
                            metaInformationNode.ResolveObjectType(),
                            annotation,
                            context.Tokens,
                            context.Text
                        );
        }

        public static object? CompileKeyValueList(Annotation annotation, CompileContext<object> context)
        {
            if (annotation.Children != null && annotation.Children.Count > 0)
            {
                var result = new Dictionary<string, object?>();

                foreach (var kvpNode in annotation)
                {
                    var key = (string) (kvpNode[0]! == PropertyFileNames.String
                        ? CompileString(kvpNode[0]!, context)
                        : CompileIdentifier(kvpNode[0]!, context))!;

                    var value = context.Compile(kvpNode[1]!);

                    result[key] = value;
                }

                return result;
            }

            return null;
        }

        public static object? CompileNull(Annotation annotation, CompileContext<object> context) => 
            null;

        public static object? CompileString(Annotation annotation, CompileContext<object> context) =>
            context.GetText(annotation)[1..^1];
        

        // -- Private methods -----------------------------------------------------------------------------------------

        private static Type? GetCommonValueType(object? value, Type? currentType)
        {
            if (value != null)
            {
                if (currentType == null)
                {
                    return value.GetType();
                }
                else if (currentType != typeof(object) && value.GetType() != currentType)
                {
                    // multiple different types can only be covered by object
                    return typeof(object);
                }
            }

            return currentType;
        }
    }
}
