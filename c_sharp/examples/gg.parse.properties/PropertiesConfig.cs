// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.properties
{
    // xxx add allowed types
    public readonly struct PropertiesConfig
    {
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
        }

        public PropertiesConfig(PropertiesFormat format = PropertiesFormat.Default, string indent = "    ", bool addMetaInfo = false)
        {
            Indent = indent;
            Format = format;
            AddMetaInformation = addMetaInfo;
        }

        public static PropertiesConfig operator +(PropertiesConfig left, int count)
        {
            return new PropertiesConfig(left.Format, left.Indent, left.AddMetaInformation)
            {
                IndentCount = left.IndentCount + count
            };
        }
    }
}
