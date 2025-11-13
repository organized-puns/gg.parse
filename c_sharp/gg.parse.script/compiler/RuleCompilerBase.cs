// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;
using System.Diagnostics;

using gg.parse.core;
using gg.parse.rules;
using gg.parse.script.common;
using gg.parse.script.parser;
using gg.parse.util;

namespace gg.parse.script.compiler
{
    public sealed class RuleCompilationContext : CompileContext
    {
        public RuleHeader? RuleHeader { get; init; }

        public RuleCompilationContext(string text, ImmutableList<Annotation> tokens) 
            : base(text, tokens)
        {
        }

        public RuleCompilationContext(string text, ImmutableList<Annotation> tokens, ImmutableList<Annotation> grammar)
            : base(text, tokens, grammar)
        {
        }

        [DebuggerStepThrough]
        public RuleCompilationContext(RuleCompilationContext source, RuleHeader header)
            : base(source.Text, source.Tokens, source.SyntaxTree!, source.Logs)
        {
            RuleHeader = header;
        }
    }

    public abstract class RuleCompilerBase<T> : CompilerTemplate<string, RuleCompilationContext>
        where T : IComparable<T>
    {

        [DebuggerStepThrough]
        public RuleCompilerBase()
        {
        }

        public RuleCompilerBase(Dictionary<string, CompileFunc<RuleCompilationContext>> functions)
            : base(functions)
        {
        }

        protected override string SelectKey(Type? targetType, Annotation annotation, RuleCompilationContext context) =>
            annotation.Name;

        // -- Generic functions ---------------------------------------------------------------------------------------

        public object? CompileRule(Type? _, Annotation annotation, RuleCompilationContext context)
        {
            var header = RuleCompilerBase<T>.ReadRuleHeader(annotation, context);
            return Compile(_, annotation[header.Length]!, new RuleCompilationContext(context, header));
        }

        public object? CompileAny(Type? _, Annotation annotation, RuleCompilationContext context)
        {
            var header = context.RuleHeader;

            Assertions.RequiresNotNull(header);

            return new MatchAnyData<T>(header.Name, header.Prune, precedence: header.Precedence);
        }        

        public static object? CompileIdentifier(Type? _, Annotation annotation, RuleCompilationContext context)
        {
            var header = context.RuleHeader;

            Assertions.RequiresNotNull(header);

            var hasOutputModifier = (annotation.Count > 1);
            var referenceName = hasOutputModifier
                    // ref name contains a output - take the name only 
                    ? context.GetText(annotation[1]!)
                    // no operator, use the entire span
                    : context.GetText(annotation);

            if (string.IsNullOrEmpty(referenceName))
            {
                throw new CompilationException("ReferenceName text is empty (somehow...).", annotation: annotation);
            }

            var referencePruning = AnnotationPruning.None;

            // xxx should raise a warning if product is anything else than annotation eg
            // the user specifies #rule = ~ref; the outcome for the product is ~ but that's
            // arbitrary. The user should either go #rule = ref or rule = ~ref...
            if (hasOutputModifier)
            {
                annotation[0]!.TryReadPruning(out referencePruning);
            }

            return new RuleReference<T>(header.Name, header.Prune, header.Precedence, referenceName, referencePruning);
        }

        public object? CompileBinaryOperator(Type? type, Annotation annotation, RuleCompilationContext context)
        {
            Assertions.RequiresNotNull(type);

            var header = context.RuleHeader;

            Assertions.RequiresNotNull(header);

            IRule[]? elementArray = null;

            if (annotation.Count > 0)
            {
                elementArray = new IRule[annotation.Count];

                for (var i = 0; i < annotation.Count; i++)
                {
                    var elementBody = annotation[i]!;
                    var elementName = elementBody.GenerateUnnamedRuleName(context, header.Name, i);
                    var elementHeader = new RuleHeader(AnnotationPruning.None, elementName, 0, 0, false);

                    elementArray[i] = Compile(null, elementBody, new RuleCompilationContext(context, elementHeader)) as IRule
                            ?? throw new CompilationException("Cannot compile rule definition for sequence.", annotation: elementBody);
                }
            }

            // by default binary (and unary) operators pass the result of the children because
            // in most cases we're interested in the values in the operation not the fact that there is an binary operation.
            // The latter would result in much more overhead in specifying the parsers.
            var output =
                header.IsTopLevel
                    ? header.Prune
                    : AnnotationPruning.Root;

            return Activator.CreateInstance(
                type,
                header.Name,
                output,
                header.Precedence,
                elementArray
            )!;
        }

        public object? CompileSequence(Type? type, Annotation annotation, RuleCompilationContext context) =>
            CompileBinaryOperator(typeof(MatchRuleSequence<T>), annotation, context);

        public object? CompileOneOf(Type? type, Annotation annotation, RuleCompilationContext context) =>
            CompileBinaryOperator(typeof(MatchOneOf<T>), annotation, context);

        public object? CompileEvaluation(Type? type, Annotation annotation, RuleCompilationContext context) =>
            CompileBinaryOperator(typeof(MatchEvaluation<T>), annotation, context);


        // -- Private methods -----------------------------------------------------------------------------------------

        /// <summary>
        /// Captures the rule header properties and body node
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ruleNodes">Nodes that make up the product, rulename, precendence and rulebody</param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static RuleHeader ReadRuleHeader(Annotation annotation, CompileContext context)
        {
            Assertions.RequiresNotNull(annotation);
            Assertions.RequiresNotNull(context);
            Assertions.RequiresNotNull(annotation.Count >= 1);

            var idx = 0;

            // annotation prunig is optional, (will default to None)
            if (annotation[0]!.TryReadPruning(out var pruning))
            {
                idx++;
            }

            var name = context.GetText(annotation[idx]!);
            idx++;

            // precedence is optional (will default to 0)
            // can exceed range if the rule is empty (ie rule = ;) so test for that
            var precedence = 0;

            if (annotation.Count > idx 
                && annotation[idx]! == CommonTokenNames.Integer
                && int.TryParse(context.GetText(annotation[idx]!), out precedence))
            {
                idx++;
            }

            return new(pruning, name, precedence, idx);
        }
    }
}
