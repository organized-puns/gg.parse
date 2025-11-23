// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;
using System.Reflection;

using gg.parse.core;
using gg.parse.script.compiler;

namespace gg.parse.properties
{
    public enum NumericPrecision
    {
        Float,
        Double
    }

    public class PropertyContext : CompileContext
    {
        public TypePermissions AllowedTypes
        {
            get;
            private set;
        }        

        public NumericPrecision Precision
        {
            get;
            init;
        }

        public PropertyContext(
            string text, 
            ImmutableList<Annotation> tokens, 
            bool allowUnmanagedTypes = false,
            NumericPrecision precision = NumericPrecision.Float) 
            : base(text, tokens)
        {
            AllowedTypes = new TypePermissions()
            {
                AllowUnmanagedTypes = allowUnmanagedTypes
            };

            Precision = precision;
        }

        public PropertyContext(
            string text,
            ImmutableList<Annotation> tokens,
            TypePermissions permissions,
            NumericPrecision precision = NumericPrecision.Float)
            : base(text, tokens)
        {
            AllowedTypes = permissions;
            Precision = precision;
        }

        public PropertyContext(
            string text, 
            ImmutableList<Annotation> tokens, 
            ImmutableList<Annotation> syntaxTree,
            bool allowUnmanagedTypes = false,
            NumericPrecision precision = NumericPrecision.Float)
            : base(text, tokens, syntaxTree)
        {
            AllowedTypes = new TypePermissions()
            {
                AllowUnmanagedTypes = allowUnmanagedTypes
            };

            Precision = precision;
        }

        public PropertyContext(
            string text,
            ImmutableList<Annotation> tokens,
            ImmutableList<Annotation> syntaxTree,
            TypePermissions allowedTypes,
            NumericPrecision precision = NumericPrecision.Float)
            : base(text, tokens, syntaxTree)
        {
            AllowedTypes = allowedTypes;
            Precision = precision;
        }

        public PropertyContext AllowType(Type type)
        {
            AllowedTypes.AllowType(type);
            return this;
        }

        public PropertyContext AllowTypes(params Type[] types)
        {
            AllowedTypes.AllowTypes(types);
            return this;
        }

        public PropertyContext AllowTypes(Assembly assembly, string typeNamespace)
        {
            AllowedTypes.AllowTypes(assembly, typeNamespace);
            return this;
        }

        public Type ResolveType(string? name) => 
            AllowedTypes.ResolveType(name);
        
    }
}
