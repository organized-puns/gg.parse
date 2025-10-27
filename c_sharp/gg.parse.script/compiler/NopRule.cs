// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.script.compiler
{
    /// <summary>
    /// Rule used in compilation where the rule body is empty. Can be (out) optimized later
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NopRule<T> : RuleBase<T> where T: IComparable<T>
    {
        public NopRule(string name) 
            : base(name, AnnotationPruning.All)
        {
        }

        public override ParseResult Parse(T[] input, int start) => ParseResult.Success;
    }
}
