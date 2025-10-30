// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse
{
    /// <summary>
    /// Rule which contains other rules 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRuleComposition<T> : IRule where T : IComparable<T>
    {
        /// <summary>
        /// 0 or more rules which make up this composition. 
        /// </summary>
        IEnumerable<RuleBase<T>>? Rules { get; }

        /// <summary>
        /// Return the number of rules in this composition.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Return the index-th rule in this composition.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        RuleBase<T>? this[int index] { get; }

        /// <summary>
        /// Clone this rule taking in account the provided rule composition.
        /// </summary>
        /// <param name="composition"></param>
        /// <returns></returns>
        IRuleComposition<T> CloneWithComposition(IEnumerable<RuleBase<T>> composition);

        /// <summary>
        /// Replace the current composition with the provided one. We need this to support
        /// forward declarations. As the name suggest, this should be used with care and
        /// prefer CloneWithComposition where possible.
        /// </summary>
        /// <param name="composition"></param>
        void MutateComposition(IEnumerable<RuleBase<T>> composition);
    }
}
