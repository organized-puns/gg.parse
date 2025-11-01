// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.core
{
    /// <summary>
    /// Rule which contains other rules 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRuleComposition : IRule 
    {
        /// <summary>
        /// 0 or more rules which make up this composition. 
        /// </summary>
        IEnumerable<IRule>? Rules { get; }

        /// <summary>
        /// Return the number of rules in this composition.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Return the index-th rule in this composition.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IRule? this[int index] { get; }

        /// <summary>
        /// Clone this rule taking in account the provided rule composition.
        /// </summary>
        /// <param name="composition"></param>
        /// <returns></returns>
        IRuleComposition CloneWithComposition(IEnumerable<IRule> composition);

        /// <summary>
        /// Replace the current composition with the provided one. We need this to support
        /// forward declarations. As the name suggest, this should be used with care and
        /// prefer CloneWithComposition where possible.
        /// </summary>
        /// <param name="composition"></param>
        void MutateComposition(IEnumerable<IRule> composition);
    }
}
