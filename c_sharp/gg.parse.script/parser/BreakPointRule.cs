// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Diagnostics;

using gg.parse.core;
using gg.parse.util;

namespace gg.parse.script.parser
{
    public sealed class BreakPointRule<T> : MetaRuleBase<T> where T : IComparable<T>
    {

        public BreakPointRule(string name, IRule? rule)
            : base(name, AnnotationPruning.Root, 0, rule)
        {
        }
        
        public BreakPointRule(string name, AnnotationPruning pruning, int precedence, IRule? rule)
            : base(name, pruning, precedence, rule)
        {
        }

        public override ParseResult Parse(T[] input, int start)
        {
            Assertions.RequiresNotNull(Subject);
            
            Debugger.Break();

            var result = Subject.Parse(input, start);
            return BuildResult(new util.Range(start, result.MatchLength), result.Annotations);
        }

        public override BreakPointRule<T> CloneWithSubject(IRule subject) =>
            new(Name, AnnotationPruning.Root, 0, subject);
    }

    public static class BreakPointRuleExtensions
    {
        public static BreakPointRule<T> AddBreakpoint<T>(this MutableRuleGraph<T> graph, string ruleName) 
            where T : IComparable<T>
        {
            var original = graph[ruleName];
            var breakPoint = graph.ReplaceRule(original, new BreakPointRule<T>(ruleName, null));
            breakPoint.MutateSubject(original);
            return breakPoint;
        }

        /// <summary>
        /// Note: this currently only works if the breakpoint was added by AddBreakpoint, NOT
        /// when the breakpoint is in a script because it will have a different name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="graph"></param>
        /// <param name="breakPoint"></param>
        public static void RemoveBreakpoint<T>(this MutableRuleGraph<T> graph, BreakPointRule<T> breakPoint)
            where T : IComparable<T>
        {
            Assertions.RequiresNotNull(breakPoint.Subject, 
                "BreakPointRule subject cannot be null when removing breakpoint.");
            
            graph.ReplaceRule(breakPoint, breakPoint.Subject);
        }
    }
}
