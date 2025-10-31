// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Reflection;

namespace gg.parse.argparser
{
    public partial class ArgsReader<T>
    {
        private class PropertyArgs(PropertyInfo? propertyInfo, FieldInfo? fieldInfo, int index, ArgAttribute? attribute)
        {
            public PropertyInfo? ArgPropertyInfo { get; init; } = propertyInfo;

            public FieldInfo? ArgFieldInfo { get; init; } = fieldInfo;

            public int ArgIndex { get; init; } = index;

            public ArgAttribute? Attribute { get; init; } = attribute;
                        

            public bool MatchesFullName(string key)
            {
                if (Attribute != null)
                {
                    return Attribute.FullName == key;
                }
                else if (ArgPropertyInfo != null)
                {
                    return ArgPropertyInfo.Name == key;
                }
                else if (ArgFieldInfo != null)
                {
                    return ArgFieldInfo.Name == key;
                }

                throw new InvalidProgramException("PropertyArgs not initialized correctly.");
            }

            public bool MatchesShortName(string key) =>
                Attribute != null && Attribute.ShortName == key;

            public bool MatchesIndex(int index) =>
                Attribute == null
                    ? ArgIndex == index
                    : Attribute.Index == index;

            public bool IsRequired =>
                Attribute != null && Attribute.IsRequired;

            public string KeyToString()
            {
                var key = ArgIndex >= 0 ? $"index: {ArgIndex}" : "";

                if (Attribute != null)
                {
                    key = !string.IsNullOrEmpty(Attribute.ShortName) ? $"{key}, short name: -{Attribute.ShortName}" : key;
                    key = !string.IsNullOrEmpty(Attribute.FullName) ? $"{key}, full name: --{Attribute.FullName}" : key;
                }
                return key;
            }

            public void SetValue(object target, object value)
            {
                if (ArgPropertyInfo != null)
                {
                    ArgPropertyInfo.SetValue(target, value);
                }
                else if (ArgFieldInfo != null)
                {
                    ArgFieldInfo.SetValue(target, value);
                }
                else
                {
                    throw new InvalidProgramException("PropertyArgs not initialized correctly.");
                }
            }

            public string ArgName
            {
                get
                {
                    if (ArgPropertyInfo != null)
                    {
                        return ArgPropertyInfo.Name;
                    }
                    else if (ArgFieldInfo != null)
                    {
                        return ArgFieldInfo.Name;
                    }
                    throw new InvalidProgramException("PropertyArgs not initialized correctly.");
                }
            }

            public Type ArgType
            {
                get
                {
                    if (ArgPropertyInfo != null)
                    {
                        return ArgPropertyInfo.PropertyType;
                    }
                    else if (ArgFieldInfo != null)
                    {
                        return ArgFieldInfo.FieldType;
                    }
                    throw new InvalidProgramException("PropertyArgs not initialized correctly.");
                }
            }

        }
    }
}
