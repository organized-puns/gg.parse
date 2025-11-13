// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.script.compiler;
using gg.parse.script.parser;
using gg.parse.util;
using System.Collections;
using System.Globalization;

namespace gg.parse.properties
{
    public enum TypeCategory
    {
        Array,
        Boolean,
        Char,
        Class,
        Dictionary,
        Double,
        Enum,
        Float,
        Int,
        List,
        KeyValuePairs,
        None,
        Set,
        String,
        Struct
    }

    public sealed class TypeToPropertyCompiler : CompilerTemplate<TypeCategory, PropertyContext>
    {
        public ICompilerTemplate<PropertyContext> AnnotationCompiler
        {
            get;
            init;
        }

        public TypeToPropertyCompiler(ICompilerTemplate<PropertyContext> annotationBasedCompiler)
        {
            Assertions.RequiresNotNull(annotationBasedCompiler);
            
            AnnotationCompiler = annotationBasedCompiler;

            RegisterDefaultFunctions();
        }

        public TypeToPropertyCompiler(
            Dictionary<TypeCategory, CompileFunc<PropertyContext>> properties,
            ICompilerTemplate<PropertyContext> annotationBasedCompiler)
            : base(properties) 
        {
            Assertions.RequiresNotNull(annotationBasedCompiler);

            AnnotationCompiler = annotationBasedCompiler;
        }

        public override ICompilerTemplate<PropertyContext> RegisterDefaultFunctions()
        {
            Register(TypeCategory.Array, CompileArray);
            Register(TypeCategory.Boolean, CompileBoolean);
            Register(TypeCategory.Char, CompileChar);
            Register(TypeCategory.Class, CompileClass);
            Register(TypeCategory.Dictionary, CompileDictionary);
            Register(TypeCategory.Double, CompileDouble);
            Register(TypeCategory.Enum, CompileEnum);
            Register(TypeCategory.Float, DecimalProperty.CompileDecimal);
            Register(TypeCategory.Int, CompileInt);
            Register(TypeCategory.KeyValuePairs, CompileKeyValuePairs);
            Register(TypeCategory.List, CompileList);
            Register(TypeCategory.Set, CompileSet);
            Register(TypeCategory.String, CompileString);
            Register(TypeCategory.Struct, CompileClass);

            Register(TypeCategory.None, (type, annotation, context) => {
                context.ReportError($"No backing implementation to parse type {type}.", annotation);
                return null;
            });

            return this;
        }

        public override object? Compile(Type? targetType, Annotation annotation, PropertyContext context)
        {
            Assertions.RequiresNotNull(AnnotationCompiler, 
                "Initialization not complete, property 'AnnotationCompiler' set.");

            // need to intercept "object" type as this can't be compiled by type
            return targetType == typeof(object)
                ? AnnotationCompiler.Compile(targetType, annotation, context)
                : base.Compile(targetType, annotation, context);

        }

        public object? CompileArray(Type? targetType, Annotation annotation, PropertyContext context)
        {
            Assertions.RequiresNotNull(targetType);

            var arrayType = targetType.GetElementType();

            if (arrayType != null && annotation == PropertiesNames.Array)
            {
                // remove the start and end of the array from the count
                var result = Array.CreateInstance(arrayType, annotation.Count - 2);
                var valueCompiler = SelectCompiler(arrayType);

                // need to skip the array start and end, so start at 1 and end at -1 
                for (var i = 1; i < annotation.Count - 1; i++)
                {
                    result.SetValue(valueCompiler.Compile(arrayType, annotation[i]!, context), i - 1);
                }

                return result;
            }
            else if (annotation == PropertiesNames.Null)
            {
                return null;
            }

            if (arrayType == null)
            {
                context.ReportError(
                    $"Trying to compile an array but provided target type '{targetType}' element type is null.",
                    annotation
                );
            }
            else
            {
                context.ReportError(
                    $"Trying to compile an array<{ arrayType }>, but the annotation is not marked as an array but '{annotation.Rule.Name}'",
                    annotation
                );
            }

            return null;
        }

        public static object? CompileBoolean(Type? targetType, Annotation annotation, CompileContext context) =>
            // xxx add exception in case of failure
            bool.Parse(context.GetText(annotation));

        public static object? CompileChar(Type? targetType, Annotation annotation, CompileContext context) =>
            // xxx add exception in case of failure
            context.GetText(annotation)[0];

        public object? CompileClass(Type? targetType, Annotation annotation, PropertyContext context)
        {
            Assertions.RequiresNotNull(targetType);

            if (annotation == PropertiesNames.Dictionary)
            {
                var result = Activator.CreateInstance(targetType);

                if (result == null)
                {
                    context.ReportError($"Can't create an instance of object type <{targetType}>.", annotation);
                    return null;
                }

                // need to skip scope start and end, so start at 1 and end at -1
                for (var i = 1; i < annotation.Count - 1; i++)
                {
                    var kvp = annotation[i];

                    Assertions.RequiresNotNull(kvp);
                    Assertions.Requires(kvp == PropertiesNames.KvpPair);
                    Assertions.Requires(kvp.Count >= 2);

                    // xxx note we don't care if this fails, but ideally we should issue a warning
                    TrySetObjectProperty(result, kvp, context);
                }

                return result;
            }
            else if (annotation == PropertiesNames.Null)
            {
                return null;
            }

            // don't throw try to find as many errors as possible
            context.ReportError(
                $"Trying to compile an object of type '{targetType}' but the annotation is marked as but a '{annotation.Rule.Name}'.",
                annotation
            );
            
            return null;
        }

        public object? CompileDictionary(Type? targetType, Annotation annotation, PropertyContext context)
        {
            Assertions.RequiresNotNull(targetType);
            Assertions.RequiresNotNull(annotation);

            var keyType = targetType.GetGenericArguments()[0];
            var valueType = targetType.GetGenericArguments()[1];

            var keyCompiler = SelectCompiler(keyType);
            var valueCompiler = SelectCompiler(keyType);

            if (annotation == PropertiesNames.Dictionary)
            {
                var genericType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                var result = Activator.CreateInstance(genericType) as IDictionary
                    ?? throw new ArgumentException($"Can't create an instance of dictionary with key/value type <{keyType}, {valueType}>.");

                // may need to skip scope start and end, so start at 1 and end at -1
                for (var i = 1; i < annotation.Count - 1; i++)
                {
                    var keyNode = annotation[i]![0]!;

                    if (keyNode != PropertiesNames.Null)
                    {
                        var key = keyCompiler.Compile(keyType, keyNode, context);

                        // something went wrong - hope this is reported by the actual compile
                        if (key != null)
                        {
                            var valueNode = annotation[i]![1]!;
                            var value = valueCompiler.Compile(valueType, valueNode, context);

                            try
                            {
                                result.Add(key, value);
                            }
                            catch (Exception e)
                            {
                                context.ReportError(
                                    $"Can't add '{key}', with value annotation: '{valueNode.Name}'", 
                                    valueNode,
                                    e
                                );
                            }
                        }
                    }
                    else
                    {
                        context.ReportError(
                            $"Can't create an instance of dictionary with a null key.", 
                            annotation[i]![0]!
                        );
                    }
                }

                return result;
            }
            else if (annotation == PropertiesNames.Null)
            {
                return null;
            }

            // don't throw try to find as many errors as possible
            context.ReportError(
                $"Trying to compile Dictionary<{keyType}, {valueType}>, but the annotation is not marked as a dictionary but '{annotation.Rule.Name}'.",
                annotation
            );

            return null;
        }

        public static object? CompileDouble(Type? targetType, Annotation annotation, PropertyContext context) =>
            double.Parse(context.GetText(annotation), CultureInfo.InvariantCulture);

        public static object? CompileEnum(Type? targetType, Annotation annotation, PropertyContext context) =>
            EnumProperty.Parse(annotation.KeyToPropertyName(context.GetText(annotation)), context.AllowedTypes);

        public static object? CompileInt(Type? targetType, Annotation annotation, PropertyContext context) =>
            int.Parse(context.GetText(annotation), CultureInfo.InvariantCulture);

        public object? CompileKeyValuePairs(Type? targetType, Annotation annotation, PropertyContext context)
        {
            Assertions.RequiresNotNull(targetType);

            if (targetType.IsDictionary())
            {
                return CompileDictionaryKeyValuePairs(targetType, annotation, context);
            }
            else if (targetType.IsClass() || targetType.IsStruct())
            {
                return CompileObjectKeyValuePairs(targetType, annotation, context);
            }

            context.ReportError(
                $"No backing implementation to CompileKeyValuePairs for target type {targetType}",
                annotation
            ); 
            
            return null;
        }

        public object? CompileDictionaryKeyValuePairs(Type? targetType, Annotation annotation, PropertyContext context)
        {
            Assertions.RequiresNotNull(targetType);

            var result = (IDictionary?) Activator.CreateInstance(targetType)
                    ?? throw new ArgumentException($"Can't create an instance of object type <{targetType}>.");

            var keyType = targetType.GetGenericArguments()[0];
            var keyCompiler = SelectCompiler(targetType.GetGenericArguments()[0]);
            var valueCompiler = SelectCompiler(targetType.GetGenericArguments()[1]);

            for (var i = 0; i < annotation.Count; i++)
            {
                var kvp = annotation[i];

                Assertions.RequiresNotNull(kvp);
                Assertions.Requires(kvp == PropertiesNames.KvpPair);
                Assertions.Requires(kvp.Count >= 2);

                result.Add(
                    keyCompiler.Compile(keyType, kvp[0]!, context)!, 
                    valueCompiler.Compile(keyType, kvp[1]!, context)
                );
            }

            return result;
        }

        public object? CompileObjectKeyValuePairs(Type? targetType, Annotation annotation, PropertyContext context)
        {
            Assertions.RequiresNotNull(targetType);
            Assertions.Requires(targetType.IsClass() || targetType.IsStruct());

            var result = Activator.CreateInstance(targetType)
                    ?? throw new ArgumentException($"Can't create an instance of object type <{targetType}>.");

            for (var i = 0; i < annotation.Count; i++)
            {
                var kvp = annotation[i];

                Assertions.RequiresNotNull(kvp);
                Assertions.Requires(kvp == PropertiesNames.KvpPair);
                Assertions.Requires(kvp.Count >= 2);

                // xxx note we don't care if this fails, but ideally we should issue a warning
                TrySetObjectProperty(result, kvp, context);
            }

            return result;
        }

        public object? CompileList(Type? targetType, Annotation annotation, PropertyContext context)
        {
            Assertions.RequiresNotNull(targetType);
            Assertions.RequiresNotNull(annotation);

            var listType = targetType.GetGenericArguments()[0];

            var valueCompiler = SelectCompiler(targetType.GetGenericArguments()[0]);

            Assertions.RequiresNotNull(valueCompiler);

            if (annotation == PropertiesNames.Array)
            {
                var genericType = typeof(List<>).MakeGenericType(listType);
                var result = Activator.CreateInstance(genericType) as IList
                    ?? throw new ArgumentException($"Can't create an instance of list with list type <{listType}>.");

                // need to skip start and end, so start at 1 and end at -1 
                for (var i = 1; i < annotation.Count - 1; i++)
                {
                    result.Add(valueCompiler.Compile(listType, annotation[i]!, context));
                }

                return result;
            }
            else if (annotation == PropertiesNames.Null)
            {
                return null;
            }

            context.ReportError(
                $"Trying to compile List<{listType}> but the annotation doesn't describe a list.",
                annotation
            );

            return null;
        }

        public object? CompileSet(Type? targetType, Annotation annotation, PropertyContext context)
        {
            Assertions.RequiresNotNull(targetType);
            Assertions.RequiresNotNull(annotation);

            var setType = targetType.GetGenericArguments()[0];
            var valueCompiler = SelectCompiler(targetType.GetGenericArguments()[0]);

            if (annotation == PropertiesNames.Array)
            {
                var genericType = typeof(HashSet<>).MakeGenericType(setType);
                var result = Activator.CreateInstance(genericType)
                    ?? throw new ArgumentException($"Can't create an instance of list with list type <{setType}>.");
                var addMethod = genericType.GetMethod("Add");

                // need to skip start and end, so start at 1 and end at -1 
                for (var i = 1; i < annotation.Count - 1; i++)
                {
                    addMethod!.Invoke(result, [valueCompiler.Compile(setType, annotation[i]!, context)]);
                }

                return result;
            }
            else if (annotation == PropertiesNames.Null)
            {
                return null;
            }

            context.ReportError(
                $"Trying to compile Set<{setType}> but the annotation does not describe a set (must be defined as an array).",
                annotation         
            );

            return null;
        }

        public static object? CompileString(Type? targetType, Annotation annotation, PropertyContext context)
        {
            var text = context.GetText(annotation);

            if (annotation == PropertiesNames.String)
            {
                // xxx replace with tokens
                if (text.StartsWith('"') || text.StartsWith('\''))
                {
                    return text[1..^1];
                }
            }
            else if (annotation == PropertiesNames.Null)
            {
                return null;
            }

            return text;
        }

        // -- Protected methods ---------------------------------------------------------------------------------------

        protected override TypeCategory SelectKey(Type? targetType, Annotation annotation, PropertyContext context)
        {
            Assertions.RequiresNotNull(targetType);

            // need to check this annotation first before looking at the types
            // to deal that the format may omit the first values
            if (annotation == PropertiesNames.KvpList)
            {
                return TypeCategory.KeyValuePairs;
            }
            else if (targetType.IsArray)
            {
                return TypeCategory.Array;
            }
            else if (targetType.IsGenericType)
            {
                var interfaces = targetType.GetInterfaces();

                if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                {
                    return TypeCategory.Dictionary;
                }
                else if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
                {
                    return TypeCategory.List;
                }
                else if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>)))
                {
                    return TypeCategory.Set;
                }

                return TypeCategory.None;
            }
            else if (targetType == typeof(string))
            {
                return TypeCategory.String;
            }
            // have this check after string
            else if (targetType.IsClass)
            {
                return TypeCategory.Class;
            }
            else if (targetType.IsEnum)
            {
                return TypeCategory.Enum;
            }
            else if (targetType.IsValueType && !targetType.IsEnum && !targetType.IsPrimitive)
            {
                return TypeCategory.Struct;
            }
            else if (targetType == typeof(bool))
            {
                return TypeCategory.Boolean;
            }
            else if (targetType == typeof(int))
            {
                return TypeCategory.Int;
            }
            else if (targetType == typeof(double))
            {
                return TypeCategory.Double;
            }
            else if (targetType == typeof(float))
            {
                return TypeCategory.Float;
            }
            else if (targetType == typeof(char))
            {
                return TypeCategory.Char;
            }

            return TypeCategory.None;
        }

        // -- Private methods -----------------------------------------------------------------------------------------

        private bool TrySetObjectProperty(
            object target,
            Annotation annotation,
            PropertyContext context
        )
        {
            var keyAnnotation = annotation[0];
            var valueAnnotation = annotation[1];

            Assertions.RequiresNotNull(keyAnnotation);
            Assertions.RequiresNotNull(valueAnnotation);

            var key = keyAnnotation.KeyToPropertyName(context.GetText(keyAnnotation));

            var property = target.GetType().GetProperty(key);

            if (property != null)
            {
                var valueCompiler = SelectCompiler(property.PropertyType);
                property.SetValue(target, valueCompiler.Compile(property.PropertyType, valueAnnotation, context));
                return true;
            }
            else
            {
                var field = target.GetType().GetField(key);

                if (field != null)
                {
                    field.SetValue(target, Compile(field.FieldType, valueAnnotation, context));
                    return true;
                }
            }

            return false;
        }

        private ICompilerTemplate<PropertyContext> SelectCompiler(Type t)
        {
            var result = t == typeof(object) ? AnnotationCompiler : this;
            Assertions.RequiresNotNull(result);
            return result;
        }
    }
}
