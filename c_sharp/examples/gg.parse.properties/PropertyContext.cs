// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.script.compiler;
using gg.parse.util;
using System.Collections.Immutable;
using System.Reflection;

namespace gg.parse.properties
{
    public class PropertyContext : CompileContext
    {
        public TypePermissions AllowedTypes
        {
            get;
            private set;
        }        

        public PropertyContext(
            string text, 
            ImmutableList<Annotation> tokens, 
            bool allowUnmanagedTypes = false) 
            : base(text, tokens)
        {
            AllowedTypes = new TypePermissions()
            {
                AllowUnmanagedTypes = allowUnmanagedTypes
            };
        }

        public PropertyContext(
            string text, 
            ImmutableList<Annotation> tokens, 
            ImmutableList<Annotation> syntaxTree,
            bool allowUnmanagedTypes = false)
            : base(text, tokens, syntaxTree)
        {
            AllowedTypes = new TypePermissions()
            {
                AllowUnmanagedTypes = allowUnmanagedTypes
            };
        }

        public PropertyContext(
            string text,
            ImmutableList<Annotation> tokens,
            ImmutableList<Annotation> syntaxTree,
            TypePermissions allowedTypes)
            : base(text, tokens, syntaxTree)
        {
            AllowedTypes = allowedTypes;
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
