// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.util
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> values, Action<T> action)
        {
            foreach (var item in values)
            {
                action(item);
            }

            return values;
        }

        public static IEnumerable<T> Replace<T>(this IEnumerable<T> values, T original, T replacement)
            where T : class
        {
            var array = values.ToArray();

            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == original)
                {
                    array[i] = replacement;
                }
            }

            return array;
        }
    }
}
