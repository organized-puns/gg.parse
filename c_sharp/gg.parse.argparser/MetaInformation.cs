// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.properties;
using gg.parse.util;
using System.Collections.Immutable;
using System.Text;

namespace gg.parse.argparser
{
    public class MetaInformation
    {
        public static readonly string Key = "__meta_information";

        public string? ObjectType { get; set; } 

        public static void Write(object target, StringBuilder builder, in PropertiesConfig config)
        {
            var key = config.Format == PropertiesFormat.Default ? Key : $"\"{Key}\"";

            builder.Indent(config.IndentCount, config.Indent)
                .Append($"{key}{PropertyFileTokens.KvSeparatorColon} ")
                .Append($"{{\"{nameof(ObjectType)}\"{PropertyFileTokens.KvSeparatorColon} \"{target.GetType().AssemblyQualifiedName}\"}}");
        }

        public static MetaInformation? Read(Annotation annotation, ImmutableList<Annotation> tokenList, string text) =>
            (MetaInformation?) PropertyReader.OfObject(typeof(MetaInformation), annotation, tokenList, text);
    }
}
