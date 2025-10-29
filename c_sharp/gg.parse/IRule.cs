// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse
{
    public partial interface IRule 
    {
        // xxx remove - name is the identifier
        int Id { get; set; }

        string Name { get; init; }
        
        int Precedence { get; init; }

        AnnotationPruning Prune { get; init; }
    }
}
