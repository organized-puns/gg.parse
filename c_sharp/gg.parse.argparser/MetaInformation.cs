// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.properties;
using gg.parse.script.compiler;
using gg.parse.util;
using System.Collections.Immutable;
using System.Text;

namespace gg.parse.argparser
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

        public Type ResolveObjectType()
        {
            if (!string.IsNullOrEmpty(ObjectType))
            {
                return Type.GetType(ObjectType) ?? throw new TypeNotFoundException($"Cannot resolve type '{ObjectType}'.");
            }

            throw new NullReferenceException($"{nameof(MetaInformation)} No object type defined");
        }

        public static MetaInformation? FindMetaInformation(
            Annotation annotation,
            CompileContext context,
            PropertyReaderr reader)
        {
            var predicate = new Func<Annotation, bool>(a =>
            {
                if (a == PropertyFileNames.KvpPair)
                {
                    var keyName = context.GetText(a![0]!);
                    return keyName == Key
                        || keyName == QuotedKey;
                }

                return false;
            });
            var metaInformationNode = annotation.FirstOrDefaultDfs(predicate);

            return metaInformationNode != null && metaInformationNode.Count >= 2
                ? (MetaInformation?) reader.CompileClass(typeof(MetaInformation), metaInformationNode[1]!, context)
                : null;
        }


        public static StringBuilder AppendMetaInformation(StringBuilder builder, object target, in PropertiesConfig config)
        {
            var key = config.Format == PropertiesFormat.Default ? Key : $"\"{Key}\"";

            builder.Indent(config.IndentCount, config.Indent)
                .Append($"{key}{PropertyFileTokens.KvSeparatorColon} ")
                .Append($"{{\"{nameof(ObjectType)}\"{PropertyFileTokens.KvSeparatorColon} \"{target.GetType().AssemblyQualifiedName}\"}}");

            return builder;
        }

        //public static MetaInformation? Read(Annotation annotation, CompileContext context) =>
          //  (MetaInformation?) PropertyReaderr.CompileClass(typeof(MetaInformation), annotation, context);
    }
}
