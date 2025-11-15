// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.argparser
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ArgAttribute : Attribute
    {
        public int Index { get; set; } = -1;
        
        public string? FullName { get; set; }

        public string? ShortName { get; set; }

        public bool IsRequired { get; set; } = false;

        public object? DefaultValue { get; set; }
    }
}
