// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.rules;
using gg.parse.util;

namespace gg.parse.script.compiler
{
    public static class MutableGraphExtensions
    {
        /// <summary>
        /// For all RuleReference rules in the graph find and set the actual rule they refer to.
        /// </summary>
        /// <param name="graph"></param>
        public static MutableRuleGraph<T> ResolveReferences<T>(
            this MutableRuleGraph<T> graph, 
            RuleCompilationContext? context = null
        ) where T : IComparable<T>
        {
            foreach (var rule in graph.Cast<IRule>())
            {
                try
                {
                    ResolveReference(graph, rule, true);
                }
                catch (Exception ex)
                {
                    if (context == null)
                    {
                        throw;
                    }
                    else
                    {
                        context.ReportError("Exception thrown while resolving references", ex);
                    }
                }
            }

            return graph;
        }

        private static void ResolveReference<T>(MutableRuleGraph<T> graph, IRule rule, bool isTopLevel)
            where T : IComparable<T>
        {
            if (rule is RuleReference<T> referenceRule)
            {
                if (graph.TryFindRule(referenceRule.ReferenceName, out var subject))
                {
                    referenceRule.MutateSubject(subject!);
                    referenceRule.IsTopLevel = isTopLevel;
                }
                else
                {
                    throw new CompilationException($"Can't find rule '{referenceRule.ReferenceName}'.");
                }

            }
            else if (rule is IMetaRule metaRule && metaRule.Subject != null)
            {
                ResolveReference(graph, metaRule.Subject, false);
            }
            else if (rule is IRuleComposition composition && composition.Rules != null)
            {
                composition.Rules.ForEach(rule => ResolveReference(graph, rule, false));
            }
        }
    }
}
