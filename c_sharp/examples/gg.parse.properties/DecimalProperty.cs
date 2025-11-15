// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Globalization;
using System.Text;
using gg.parse.core;
using gg.parse.util;

namespace gg.parse.properties
{
    public static class DecimalProperty
    {
        public static bool IsDecimal(object value) =>
            value is Decimal || value is float || value is double;

        public static object? CompileDecimal(Type? _, Annotation annotation, PropertyContext context)
        {
            if (context.Precision == NumericPrecision.Float)
            {
                return float.Parse(context.GetText(annotation), CultureInfo.InvariantCulture);
            }

            return double.Parse(context.GetText(annotation), CultureInfo.InvariantCulture);
        }

        public static StringBuilder AppendDecimal(this StringBuilder builder, object? value)
        {
            Assertions.RequiresNotNull(value);

            if (value is float f)
            {
                return builder.Append(f.ToString("0.0######", CultureInfo.InvariantCulture));
            }
            if (value is double d)
            {
                return builder.Append(d.ToString("0.0########", CultureInfo.InvariantCulture));
            }
            if (value is Decimal dec)
            {
                return builder.Append(dec.ToString("0.0########", CultureInfo.InvariantCulture));
            }

            throw new PropertiesException($"No backing AppendDecimal implementation for type {value.GetType()}.");
        }
    }
}