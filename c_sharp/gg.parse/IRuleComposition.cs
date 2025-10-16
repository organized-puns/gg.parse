// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse
{
    /// <summary>
    /// Rule which contains other rules 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRuleComposition<T> where T : IComparable<T>
    {
        /// <summary>
        /// 0 or more rules which make up this composition. 
        /// </summary>
        IEnumerable<RuleBase<T>>? Rules { get; }

        int Count { get; }

        RuleBase<T>? this[int index] { get; set; }
    }
}
