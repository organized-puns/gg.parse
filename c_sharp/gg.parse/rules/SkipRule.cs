// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.util;

namespace gg.parse.rules
{
    /// <summary>
    /// Rule which skips data until some EndCondition is met or EoF is encountered. 
    /// Mainly added to optimize / reduce typing for *(try(!endCondition), (eof | .)).
    /// In script: skip_until {condition} (implicit failOnEoF = true) 
    /// skip_until_eof_or {condition} (implicit failOnEoF = false) 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SkipRule<T> : MetaRuleBase<T>  where T : IComparable<T>
    {
        /// <summary>
        /// If initialized to true, this rule will fail when encountering eof (succeed otherwise)
        /// </summary>
        public bool FailOnEoF { get; init; }

        /// <summary>
        /// If set to true , the rule will stop before the condition match, otherwise it will include the 
        /// condition match in the result.
        /// </summary>
        public bool StopBeforeCondition { get; init; } 

        public SkipRule(
            string name,
            AnnotationPruning product,
            int precedence,
            IRule subject,
            bool failOnEof = true,
            bool stopBeforeCondition = true
        ) 
            : base(name, product, precedence, subject)
        {
            Assertions.RequiresNotNull(subject);

            FailOnEoF = failOnEof;
            StopBeforeCondition = stopBeforeCondition;
        }

        public override ParseResult Parse(T[] input, int start)
        {
            var idx = start;
            
            while (idx < input.Length)
            {
                var conditionalResult = Subject!.Parse(input, idx);
                
                if (conditionalResult)
                {
                    var matchLength = StopBeforeCondition 
                        ? idx - start 
                        : (idx - start) + conditionalResult.MatchLength;

                    var annotations = StopBeforeCondition
                        ? null
                        : conditionalResult.Annotations;

                    return BuildResult(new(start, matchLength), annotations);
                }

                idx++;
            }

            return FailOnEoF ? ParseResult.Failure : BuildDataRuleResult(new(start, idx - start));
        }

        public override SkipRule<T> CloneWithSubject(IRule subject) =>
            new (Name, Prune, Precedence, subject, FailOnEoF, StopBeforeCondition);
    }
}
