// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Diagnostics.CodeAnalysis;

namespace gg.parse.util
{
    public class AssertionException : Exception
    {
        public AssertionException()
        {
        }

        public AssertionException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Lightweight implementation of contracts
    /// </summary>
    public static class Assertions
    {
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Fail(string message)
        {
            Requires(false, message);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Fail()
        {
            throw new AssertionException("[Program error] Failed contract.");
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Requires(bool b)
        {
            Requires(b, "Contract violation.");
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Requires(bool b, string message)
        {
            if (!b)
            {
                throw new AssertionException(message);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void RequiresNotNull([NotNull] object? o, string message)
        {
            if (o == null)
            {
                throw new AssertionException(message);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void RequiresNotNull([NotNull] object? o)
        {
            RequiresNotNull(o, "Contract violation, object cannot be null");
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void RequiresNotNullOrEmpty([NotNull] string s, string message)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new AssertionException(message);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void RequiresNotNullOrEmpty([NotNull] string s)
        {
            RequiresNotNullOrEmpty(s, "Contract violation, string cannot be null or empty");
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void RequiresNull(object? o)
        {
            Requires(o == null, "Contract violation, object should be null.");
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void RequiresNotNullOrEmpty<T>(T[] a, string message)
        {
            RequiresNotNull(a, message);
            Requires(a.Length > 0, message);
        }


        [System.Diagnostics.Conditional("DEBUG")]
        public static void RequiresNotNullOrEmpty<T>(T[] a)
        {
            RequiresNotNullOrEmpty(a, "Contract violation, array cannot be null and should have more than zero elements.");
        }


        [System.Diagnostics.Conditional("DEBUG")]
        public static void RequiresNotNullOrEmpty<T>(IEnumerable<T> a, string message)
        {
            RequiresNotNull(a, message);
            Requires(a.Any(), message);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void RequiresNotNullOrEmpty<T>(IEnumerable<T> a)
        {
            RequiresNotNull(a, "Contract violation, enumeration cannot be null.");
            Requires(a.Any(), "Contract violation, enumeration must have at least one element.");
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void RequiresForAll<T>(IEnumerable<T> enumeration, Func<T, bool> predicate, string message)
        {
            RequiresNotNull(enumeration);

            if (enumeration.Any(o => !predicate(o)))
            {
                throw new AssertionException(message);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void RequiresForAll<T>(IEnumerable<T> enumeration, Func<T, bool> predicate)
        {
            RequiresForAll(enumeration, predicate, "Contract violation, predicate did not apply to all in the given enumeration.");
        }
    }
}