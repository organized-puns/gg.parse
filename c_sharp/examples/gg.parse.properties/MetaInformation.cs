// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Text;
using gg.parse.core;
using gg.parse.properties;
using gg.parse.script.compiler;
using gg.parse.util;

namespace gg.parse.properties
{
    public class TypeNotFoundException : Exception
    {
        public TypeNotFoundException() { }

        public TypeNotFoundException(string message) : base(message) {}
    }

    public class MetaInformation
    {
        public static readonly string Key = "property.type";
        public static readonly string QuotedKey = "\"property.type\"";

        public string? PropertyType { get; set; }

        public static MetaInformation? FindMetaInformation(
            Annotation annotation,
            PropertyContext context,
            ICompilerTemplate<PropertyContext> compiler)
        {
            var predicate = new Func<Annotation, bool>(a =>
            {
                if (a == PropertiesNames.KvpPair)
                {
                    var keyName = context.GetText(a![0]!);
                    return keyName == Key
                        || keyName == QuotedKey;
                }

                return false;
            });
            
            var metaInformationNode = annotation.FirstOrDefaultDfs(predicate);

            if (metaInformationNode != null && metaInformationNode.Count >= 2)
            {
                var valueNode = metaInformationNode![1]!;
                var nodeText = context.GetText(valueNode);
                return new MetaInformation() { PropertyType = valueNode.KeyToPropertyName(nodeText) };
            }

            return  null;
        }


        public static StringBuilder AppendMetaInformation(StringBuilder builder, object target, in PropertiesConfig config)
        {
            var key = config.Format == PropertiesFormat.Default ? Key : $"\"{Key}\"";
            var typeName = config.AllowedTypes.ResolveName(target.GetType());

            builder.Indent(config.IndentCount, config.Indent)
                .Append($"{key}{PropertiesTokens.KvSeparatorColon} ")
                .Append($"\"{typeName}\"}}");

            return builder;
        }
    }
}
