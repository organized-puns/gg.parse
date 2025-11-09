// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;
using System.Reflection;

using gg.parse.core;
using gg.parse.script.compiler;
using gg.parse.util;

namespace gg.parse.properties
{
    public class PropertyContext : CompileContext
    {
        private Dictionary<string, Type> AllowedTypes { get; init; } = [];

        public bool AllowUnmanagedTypes { get; init; } = false;

        public PropertyContext(string text, ImmutableList<Annotation> tokens) 
            : base(text, tokens)
        {
        }

        public PropertyContext(string text, ImmutableList<Annotation> tokens, ImmutableList<Annotation> syntaxTree)
            : base(text, tokens, syntaxTree)
        {
        }

        public PropertyContext AllowType(string id, Type type)
        {
            AllowedTypes.Add(id, type);
            return this;
        }

        public PropertyContext AllowType(Type type)
        {
            AllowedTypes.Add(type.Name, type);
            return this;
        }

        public PropertyContext AllowTypes(params Type[] types)
        {
            types.ForEach(t => AllowedTypes.Add(t.Name, t));
            return this;
        }

        public PropertyContext AllowTypes(Assembly assembly, string typeNamespace)
        {
            return AllowTypes([.. assembly.GetTypes()
                                .Where(t => t.IsPublic &&
                                        t.Namespace != null &&
                                        t.Namespace.StartsWith(typeNamespace))]);
        }

        public Type FindType(string? name) 
        {
            if (!string.IsNullOrEmpty(name))
            {
                if (AllowedTypes.TryGetValue(name, out var type))
                {
                    return type;
                }

                if (AllowUnmanagedTypes)
                {
                    var unmanagedType = Type.GetType(name);

                    if (unmanagedType != null)
                    {
                        return unmanagedType;
                    }
                }

                throw new PropertiesException($"No allowed type declared for '{name}'.");
            }

            throw new PropertiesException($"No name provided.");
        }
    }
}
