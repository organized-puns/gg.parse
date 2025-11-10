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
        public static readonly string Key = "__meta_information";
        public static readonly string QuotedKey = "\"__meta_information\"";

        public string? ObjectType { get; set; }

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

            return metaInformationNode != null && metaInformationNode.Count >= 2
                ? (MetaInformation?) compiler.Compile(typeof(MetaInformation), metaInformationNode[1]!, context)
                : null;
        }


        public static StringBuilder AppendMetaInformation(StringBuilder builder, object target, in PropertiesConfig config)
        {
            var key = config.Format == PropertiesFormat.Default ? Key : $"\"{Key}\"";
            var typeName = config.AllowedTypes.ResolveName(target.GetType());

            builder.Indent(config.IndentCount, config.Indent)
                .Append($"{key}{PropertiesTokens.KvSeparatorColon} ")
                .Append($"{{\"{nameof(ObjectType)}\"{PropertiesTokens.KvSeparatorColon} \"{typeName}\"}}");

            return builder;
        }
    }
}
