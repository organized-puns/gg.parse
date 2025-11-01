// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.core
{
    public enum AnnotationPruning
    {
        /// <summary>
        /// Prunes nothing and returns an annotation for the matched item.
        /// </summary>
        None,

        /// <summary>
        /// Prunes the root and returns the annotations produced by any child rules.
        /// </summary>
        Root,

        /// <summary>
        /// Prunes the children and returns only the root.
        /// </summary>
        Children,

        /// <summary>
        /// Prunes the root and the children.
        /// </summary>
        All
    }
}
