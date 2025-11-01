// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;
using gg.parse.core;
using gg.parse.rules;
using gg.parse.util;

namespace gg.parse.script.compiler
{
    public delegate IRule CompileFunction(RuleHeader header, Annotation bodyNode, CompileSession context);

    public class RuleCompiler
    {
        public const string DefaultRootRuleName = "root";

        public Dictionary<IRule, (CompileFunction function, string? name)> Functions { get; private set; } = [];

        public (int functionId, AnnotationPruning product)[]? RuleOutputLookup { get; set; }

        public RuleCompiler()
        {
        }

        public RuleCompiler((int functionId, AnnotationPruning product)[] outputLookup)
        {
            RuleOutputLookup = outputLookup;
        }

        public RuleCompiler MapRuleToCompilerFunction(IRule rule, CompileFunction function)
        {
            Assertions.Requires(rule != null);
            Assertions.Requires(function != null);

            Functions.Add(rule!, (function!, rule!.Name));
            return this;
        }

        public (CompileFunction function, string? name) FindCompilationFunction(IRule rule)
        {
            if (Functions.TryGetValue(rule, out var compilationFunction))
            {
                return compilationFunction;
            }

            throw new CompilationException(
                $"Cannot find a compilation function referred to by rule {rule.Name}",
                rule: rule);
        }

        public RuleGraph<T> Compile<T>(
            string text,
            ImmutableList<Annotation> tokens,
            ImmutableList<Annotation> syntaxTree,
            RuleGraph<T>? resultGraph = null) where T : IComparable<T>
        {
            Assertions.RequiresNotNull(tokens);
            Assertions.RequiresNotNull(syntaxTree);

            return Compile(new CompileSession(this, text, tokens, syntaxTree), resultGraph ?? new RuleGraph<T>());
        }

        private RuleGraph<T> Compile<T>(CompileSession session, RuleGraph<T> resultGraph) where T : IComparable<T>
        {
            foreach (var node in session.SyntaxTree!)
            {
                try
                {
                    var compiledRule = CompileRule(session, node, resultGraph);
                    var registeredRule = resultGraph.FindOrRegisterRuleAndSubRules(compiledRule);

                    if (resultGraph.Root == null || registeredRule.Name == DefaultRootRuleName)
                    {
                        resultGraph.Root = registeredRule;
                    }
                }
                catch (Exception ex)
                {
                    // add the exception and continue with the other rules
                    session.Exceptions.Add(ex);
                }
            }

            // no root defined, this can happen when the input is empty or only contains include statements
            if (resultGraph.Root == null)
            {
                session.Exceptions.Add(new CompilationException(
                    "Input text contains no root function. Make sure the main input always "
                    + "contains at least one rule."
                ));
            }

            try
            {
                // after all rules have been compiled we need to resolve any references
                // to other rules
                ResolveReferences(resultGraph);
            }
            catch (AggregateException ae)
            {
                session.Exceptions.AddRange(ae.InnerExceptions);
            }
            catch (Exception ex)
            {
                session.Exceptions.Add(ex);
            }

            if (session.Exceptions.Count > 0)
            {
                throw new AggregateException("One or more errors occurred during compilation.", session.Exceptions);
            }

            return resultGraph;
        }

        private RuleBase<T> CompileRule<T>(
            CompileSession session, 
            Annotation node, 
            RuleGraph<T> resultGraph)
            where T : IComparable<T>
        {
            var ruleHeader = ReadRuleHeader(session, node.Children!, 0);

            // user provided an empty body - which is results in a warning but is allowed
            if (ruleHeader.Length >= node.Children!.Count)
            {
                return new NopRule<T>(ruleHeader.Name);
            }
            else
            {
                var ruleBodyNode = node.Children[ruleHeader.Length];
                var (compilationFunction, _) = FindCompilationFunction(ruleBodyNode.Rule);

                // validate a named rule doesn't get implemented twice
                if (resultGraph.FindRule(ruleHeader.Name) == null)
                {
                    return (RuleBase<T>)compilationFunction(ruleHeader, ruleBodyNode, session);
                }
                else
                {
                    throw new CompilationException(
                        $"Trying to register a rule with the same name ({ruleHeader.Name}).",
                        annotation: node
                    );
                }
            }
        }

        public bool TryMatchOutputModifier(int functionId, out AnnotationPruning output)
        {
            output = AnnotationPruning.None;

            if (RuleOutputLookup != null)
            {
                for (var i = 0; i < RuleOutputLookup.Length; i++)
                {
                    if (functionId == RuleOutputLookup[i].functionId)
                    {
                        output = RuleOutputLookup[i].product;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Captures the rule header properties and body node
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ruleNodes">Nodes that make up the product, rulename, precendence and rulebody</param>
        /// <param name="index"></param>
        /// <returns></returns>
        private RuleHeader ReadRuleHeader(CompileSession context, ImmutableList<Annotation> ruleNodes, int index)
        {
            var idx = index;

            // annotation product is optional, (will default to Annotation)
            if (TryMatchOutputModifier(ruleNodes[idx].Rule.Id, out var product))
            {
                idx++;
            }

            var name = context.GetText(ruleNodes[idx].Range);
            idx++;

            // precedence is optional (will default to 0)
            // can exceed range if the rule is empty (ie rule = ;) so test for that
            var precedence = 0;
            
            if (ruleNodes.Count > idx
                && int.TryParse(context.GetText(ruleNodes[idx].Range), out precedence))
            {
                idx++;
            }

            return new(product, name, precedence, idx - index);
        }

        /// <summary>
        /// Try find the rule referred to by name in the graph. If the name is not found an exception is thrown.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="RuleReferenceException"></exception>
        private static IRule FindRule<T>(RuleGraph<T> graph, string name) where T : IComparable<T>
        {
            var referredRule = graph.FindRule(name);

            return referredRule ?? throw new CompilationException($"Cannot find rule refered to by name: {name}.");
        }

        /// <summary>
        /// In all RuleReference rules in the graph find and set the actual rule they refer to.
        /// </summary>
        /// <param name="graph"></param>
        private static void ResolveReferences<T>(RuleGraph<T> graph) where T : IComparable<T>
        {
            List<Exception> exceptions = [];

            foreach (var rule in graph)
            {
                try
                {
                    ResolveReference(graph, rule, true);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException("One or more errors occurred during reference resolution.", exceptions);
            }
        }

        private static void ResolveReference<T>(RuleGraph<T> graph, IRule rule, bool isTopLevel) where T : IComparable<T>
        {
            if (rule is RuleReference<T> referenceRule)
            {
                referenceRule.MutateSubject(FindRule(graph, referenceRule.ReferenceName));
                referenceRule.IsTopLevel = isTopLevel;
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
