// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections;
using System.Globalization;

using gg.parse.core;
using gg.parse.script.compiler;
using gg.parse.util;

namespace gg.parse.properties
{
    /// <summary>
    /// Class trying to interpret the value of an annotation (ignoring the targetType) and create
    /// a c# value from the annotation.
    /// </summary>
    public sealed class AnnotationToPropertyCompiler : CompilerTemplate<string, PropertyContext>
    {
        public ICompilerTemplate<PropertyContext>? TypeBasedCompiler
        {
            get;
            set;
        }

        public AnnotationToPropertyCompiler(ICompilerTemplate<PropertyContext>? typeBasedCompiler = null)
        {
            RegisterDefaultFunctions();
            TypeBasedCompiler = typeBasedCompiler;
        }

        public AnnotationToPropertyCompiler(
            Dictionary<string, CompileFunc<PropertyContext>> properties,
            ICompilerTemplate<PropertyContext>? typeBasedCompiler = null)
            : base(properties)
        {
            TypeBasedCompiler = typeBasedCompiler;
        }

        public override ICompilerTemplate<PropertyContext> RegisterDefaultFunctions()
        {
            Register(PropertiesNames.Array, CompileArray);
            Register(PropertiesNames.Boolean, CompileBoolean);
            Register(PropertiesNames.Dictionary, CompileDictionaryOrObject);
            Register(PropertiesNames.Float, CompileFloat);
            Register(PropertiesNames.Identifier, CompileIdentifier);
            Register(PropertiesNames.Int, CompileInt);
            Register(PropertiesNames.KvpList, CompileKeyValueListOrObject);
            Register(PropertiesNames.Null, CompileNull);
            Register(PropertiesNames.QualifiedIdentifier, CompileQualifiedIdentifierOrEnum);
            Register(PropertiesNames.String, CompileStringOrEnum);

            return this;
        }

        public object? CompileArray(Type? targetType, Annotation annotation, PropertyContext context)
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

                    tempValues[i - 1] = Compile(targetType, child, context);
                    commonArrayType = GetCommonValueType(tempValues[i - 1], commonArrayType, context);
                }

                // default to object in case the entire dictionary consists of null values
                var result = Array.CreateInstance(commonArrayType ?? typeof(object), tempValues.Length);

                Array.Copy(tempValues, result, tempValues.Length);

                return result;
            }

            return null;
        }

        public static object? CompileBoolean(Type? targetType, Annotation annotation, PropertyContext context) =>
            bool.Parse(context.GetText(annotation));

        public object? CompileDictionaryOrObject(Type? targetType, Annotation annotation, PropertyContext context)
        {
            Assertions.RequiresNotNull(TypeBasedCompiler);

            var metaInformationNode = MetaInformation.FindMetaInformation(annotation, context, TypeBasedCompiler);

            return metaInformationNode == null
                ? CompileDictionary(targetType, annotation, context)
                : TypeBasedCompiler.
                        Compile(
                            context.ResolveType(metaInformationNode.ObjectType),
                            annotation,
                            context
                        );
        }

        public object? CompileDictionary(Type? targetType, Annotation annotation, PropertyContext context)
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
                        (
                            Compile(targetType, annotation[i]![0]!, context),
                            Compile(targetType, annotation[i]![1]!, context)
                        );

                    keyValues.Add(keyValuePair);

                    keyType = GetCommonValueType(keyValuePair.key, keyType, context);
                    valueType = GetCommonValueType(keyValuePair.value, valueType, context);
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

        
        public static object? CompileFloat(Type? targetType, Annotation annotation, PropertyContext context) =>
            float.Parse(context.GetText(annotation), CultureInfo.InvariantCulture);

        public static object? CompileIdentifier(Type? targetType, Annotation annotation, PropertyContext context) =>
            context.GetText(annotation);
        
        public static object? CompileInt(Type? targetType, Annotation annotation, PropertyContext context) =>
            int.Parse(context.GetText(annotation));

        public object? CompileKeyValueListOrObject(Type? targetType, Annotation annotation, PropertyContext  context)
        {
            Assertions.RequiresNotNull(TypeBasedCompiler);

            var metaInformationNode = MetaInformation.FindMetaInformation(annotation, context, TypeBasedCompiler);

            return metaInformationNode == null
                ? CompileKeyValueList(targetType, annotation, context)
                : TypeBasedCompiler.
                        Compile(
                            context.ResolveType(metaInformationNode.ObjectType),
                            annotation,
                            context
                        );
        }

        public object? CompileKeyValueList(Type? targetType, Annotation annotation, PropertyContext context)
        {
            if (annotation.Children != null && annotation.Children.Count > 0)
            {
                var result = new Dictionary<string, object?>();

                foreach (var kvpNode in annotation)
                {
                    var key = (string) (kvpNode[0]! == PropertiesNames.String
                        ? CompileString(targetType, kvpNode[0]!, context)
                        : CompileIdentifier(targetType, kvpNode[0]!, context))!;

                    var value = Compile(targetType, kvpNode[1]!, context);

                    result[key] = value;
                }

                return result;
            }

            return null;
        }

        public static object? CompileNull(Type? targetType, Annotation annotation, PropertyContext context) => 
            null;

        public static object? CompileQualifiedIdentifierOrEnum(Type? targetType, Annotation annotation, PropertyContext context)
        {
            var text = context.GetText(annotation);

            return EnumProperty.IsEnum(text)
                ? EnumProperty.Parse(text, context.AllowedTypes)
                : text;
        }

        public static object? CompileString(Type? targetType, Annotation annotation, PropertyContext context) =>
            context.GetText(annotation)[1..^1];

        public static object? CompileStringOrEnum(Type? targetType, Annotation annotation, PropertyContext context)
        { 
            var text = context.GetText(annotation)[1..^1];

            return EnumProperty.IsEnum(text)
                ? EnumProperty.Parse(text, context.AllowedTypes)
                : text;
        }


        // -- Protected methods ---------------------------------------------------------------------------------------

        protected override string SelectKey(Type? targetType, Annotation annotation, PropertyContext context) =>
            annotation.Rule.Name;

        // -- Private methods -----------------------------------------------------------------------------------------

        private static Type ResolveType(object value, PropertyContext context)
        {
            var primaryType = value.GetType();

            if (primaryType == typeof(string))
            { 
                if (EnumProperty.IsEnum((string) value))
                {
                    return context.ResolveType((string) value);
                }
            }

            return primaryType;
        }

        private static Type? GetCommonValueType(object? value, Type? currentType, PropertyContext context)
        {
            if (value != null)
            {
                if (currentType == null)
                {
                    return ResolveType(value, context);
                }
                else if (currentType != typeof(object) && ResolveType(value, context) != currentType)
                {
                    // multiple different types can only be covered by object
                    return typeof(object);
                }
            }

            return currentType;
        }
    }
}
