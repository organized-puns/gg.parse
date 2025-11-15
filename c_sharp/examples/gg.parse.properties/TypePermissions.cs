// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Reflection;

using gg.parse.util;

namespace gg.parse.properties
{
    public class TypePermissions
    {
        private Dictionary<string, Type> AllowedTypes { get; init; } = [];

        public bool AllowUnmanagedTypes { get; init; } = false;
        
        public TypePermissions()
        {
        }

        public TypePermissions(params Type[] allowedTypes)
        {
            AllowTypes(allowedTypes);
        }


        public TypePermissions AllowType(string id, Type type)
        {
            AllowedTypes.Add(id, type);
            return this;
        }

        public TypePermissions AllowType(Type type)
        {
            AllowedTypes.Add(type.Name, type);
            return this;
        }

        public TypePermissions AllowTypes(params Type[] types)
        {
            types.ForEach(t => AllowedTypes.Add(t.Name, t));
            return this;
        }

        public TypePermissions AllowTypes(Assembly assembly, string typeNamespace)
        {
            return AllowTypes([.. assembly.GetTypes()
                                .Where(t => t.IsPublic &&
                                        t.Namespace != null &&
                                        t.Namespace.StartsWith(typeNamespace))]);
        }

        public string ResolveName(Type t)
        {
            foreach (var kvp in AllowedTypes)
            {
                if (t == kvp.Value)
                {
                    return kvp.Key;
                }
            }

            if (AllowUnmanagedTypes)
            {
                return t.AssemblyQualifiedName!;
            }

            throw new PropertiesException($"No allowed name declared for type '{t.Name}'.");
        }

        public Type ResolveType(string? name)
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
