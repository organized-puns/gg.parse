// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.core
{
    public interface IMetaRule : IRule
    {
        IRule? Subject { get; }

        IMetaRule CloneWithSubject(IRule subject);

        void MutateSubject(IRule subject);
    }
}
