// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.properties;
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
            ImmutableList<Annotation> tokenList,
            string text)
        {
            var predicate = new Func<Annotation, bool>(a =>
            {
                if (a == PropertyFileNames.KvpPair)
                {
                    var keyName = a![0]!.GetText(text, tokenList);
                    return keyName == MetaInformation.Key
                        || keyName == MetaInformation.QuotedKey;
                }

                return false;
            });
            var metaInformationNode = annotation.FirstOrDefaultDfs(predicate);

            return metaInformationNode != null && metaInformationNode.Count >= 2
                ? Read(metaInformationNode[1]!, tokenList, text)
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

        public static MetaInformation? Read(Annotation annotation, ImmutableList<Annotation> tokenList, string text) =>
            (MetaInformation?) PropertyReader.OfObject(typeof(MetaInformation), annotation, tokenList, text);
    }
}
