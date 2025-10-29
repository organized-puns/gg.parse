// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse
{
    public partial interface IRule 
    {
        /// <summary>
        /// Unique id, used to identify a rule in a rule graph. Also used as tokens in the grammar parsers.
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Unique, preferably somewhat human-readable, name of the rule. Used to identify a rule in a rule graph
        /// </summary>
        string Name { get; init; }
        
        int Precedence { get; init; }

        AnnotationPruning Prune { get; init; }
    }
}
