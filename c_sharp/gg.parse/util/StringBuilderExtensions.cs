// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Text;

namespace gg.parse.util
{
    public static class StringBuilderExtensions
    {
        /// <summary>
        /// Appends indent count times to the builder
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="count"></param>
        /// <param name="indent"></param>
        /// <returns></returns>
        public static StringBuilder Indent(this StringBuilder builder, int count, string indent)
        {
            for (var i = 0; i < count; i++)
            {
                builder.Append(indent);
            }

            return builder;
        }
    }
}
