// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.properties
{
    public readonly struct PropertiesConfig
    {
        public TypePermissions AllowedTypes
        {
            get;
            init;
        }

        public bool AddMetaInformation
        {
            get;
            init;
        }

        public PropertiesFormat Format 
        { 
            get; 
            init; 
        }

        public string Indent 
        { 
            get; 
            init; 
        }

        public int IndentCount 
        { 
            get; 
            init; 
        }

        public PropertiesConfig()
        {
            Indent = "    ";
            Format = PropertiesFormat.Default;
            AddMetaInformation = true;
            AllowedTypes = new TypePermissions();
        }

        public PropertiesConfig(
            PropertiesFormat format = PropertiesFormat.Default, 
            string indent = "    ", 
            bool addMetaInfo = false,
            TypePermissions? allowedTypes = null)
        {
            Indent = indent;
            Format = format;
            AddMetaInformation = addMetaInfo;
            AllowedTypes = allowedTypes ?? new TypePermissions();
        }

        public static PropertiesConfig operator +(PropertiesConfig left, int count)
        {
            return new PropertiesConfig(left.Format, left.Indent, left.AddMetaInformation, left.AllowedTypes)
            {
                IndentCount = left.IndentCount + count,
            };
        }

        public Type ResolveType(string? name) =>
            AllowedTypes.ResolveType(name);
    }
}
