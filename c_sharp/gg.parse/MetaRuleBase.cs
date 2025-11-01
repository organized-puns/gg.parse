// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse
{
    public abstract class MetaRuleBase<T> : RuleBase<T>, IMetaRule where T : IComparable<T>
    {
        public IRule? Subject { get; protected set; }
        
        public MetaRuleBase(string name, AnnotationPruning pruning, int precedence, IRule? subject)
            : base(name, pruning, precedence)
        {
            Subject = subject;
        }

        public void MutateSubject(IRule subject)
        {
            Subject = subject;
        }

        public abstract IMetaRule CloneWithSubject(IRule subject);
            
    }
}
