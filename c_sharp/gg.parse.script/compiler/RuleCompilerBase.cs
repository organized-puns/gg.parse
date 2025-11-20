// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;
using System.Diagnostics;

using gg.parse.core;
using gg.parse.rules;
using gg.parse.script.parser;
using gg.parse.util;

using static gg.parse.util.Assertions;

namespace gg.parse.script.compiler
{
    public sealed class RuleCompilationContext : CompileContext
    {
        public RuleHeader? RuleHeader { get; init; }

        public RuleCompilationContext(string text, ImmutableList<Annotation> tokens) 
            : base(text, tokens)
        {
        }

        public RuleCompilationContext(
            string text, 
            ImmutableList<Annotation> tokens, 
            ImmutableList<Annotation> grammar
            )
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

    /// <summary>
    /// Abstract class implementing a compiler base for compiling rules.
    /// 
    /// Covered by: <see cref="gg.parse.script.tests.compiler.TokenizerCompilerTests"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RuleCompilerBase<T> : CompilerTemplate<string, RuleCompilationContext>
        where T : IComparable<T>
    {
        public const string DefaultRootName = "root";

        public string RootName { get; init; }


        [DebuggerStepThrough]
        public RuleCompilerBase(string rootName = DefaultRootName)
        {
            RootName = rootName;
        }

        public RuleCompilerBase(
            Dictionary<string, CompileFunc<RuleCompilationContext>> functions,
            string rootName = DefaultRootName
        )
            : base(functions)
        {
            RootName = rootName;
        }

        public override ICollection<TOutput> Compile<TOutput>(
            Type? targetType, 
            ImmutableList<Annotation> annotations, 
            RuleCompilationContext context, 
            ICollection<TOutput> container)
        {
            var graph = container as MutableRuleGraph<T>;

            RequiresNotNull(graph);

            // reset the root. Included files may have set a root but the root needs to be the 
            // one of the topmost file included. 
            graph.Root = null;

            return base.Compile(targetType, annotations, context, container);
        }

        protected override string SelectKey(
            Type? targetType, 
            Annotation annotation, 
            RuleCompilationContext context) =>
            annotation.Name;


        protected override void AddOutput<TOutput>(
            TOutput output, 
            ICollection<TOutput> targetCollection,
            RuleCompilationContext context
        )
        {
            // xxx add output type
            var rule = output as IRule;
            var graph = targetCollection as MutableRuleGraph<T>;
            
            RequiresNotNull(rule);
            RequiresNotNull(graph);

            var registeredRule = graph.FindOrRegisterRuleAndSubRules(rule);

            if (graph.Root == null || rule.Name == RootName)
            {
                graph.Root = registeredRule;
            }
        }

        #region -- Compilation methods --------------------------------------------------------------------------------

        public object? CompileAny(Type? _, Annotation __, RuleCompilationContext context)
        {
            var header = context.RuleHeader;

            RequiresNotNull(header);

            return new MatchAnyData<T>(header.Name, header.Prune, precedence: header.Precedence);
        }
        
        public object? CompileRule(Type? _, Annotation annotation, RuleCompilationContext context)
        {
            var header = RuleCompilerBase<T>.ReadRuleHeader(annotation, context);

            // verify if there is any content, if not return a NoP and add a warning
            if (annotation.Count <= header.Length)
            {
                context.Log(LogLevel.Warning, $"No rule body defined for rule {header.Name}.", annotation);
                return new NopRule<T>(header.Name);
            }
                 
            return Compile(_, annotation[header.Length]!, new RuleCompilationContext(context, header));
        }        

        /// <summary>
        /// Used for indentifiers and rule references
        /// </summary>
        /// <param name="_"></param>
        /// <param name="annotation"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="CompilationException"></exception>
        public static object? CompileIdentifier(Type? _, Annotation annotation, RuleCompilationContext context)
        {
            var header = context.RuleHeader;

            RequiresNotNull(header);

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
            RequiresNotNull(type);

            var header = context.RuleHeader;

            RequiresNotNull(header);

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

        public object? CompileSequence(Type? _, Annotation annotation, RuleCompilationContext context) =>
            CompileBinaryOperator(typeof(MatchRuleSequence<T>), annotation, context);

        public object? CompileOneOf(Type? _, Annotation annotation, RuleCompilationContext context) =>
            CompileBinaryOperator(typeof(MatchOneOf<T>), annotation, context);

        public object? CompileEvaluation(Type? _, Annotation annotation, RuleCompilationContext context) =>
            CompileBinaryOperator(typeof(MatchEvaluation<T>), annotation, context);


        public object? CompileGroup(Type? _, Annotation annotation, RuleCompilationContext context)
        {
            RequiresNotNull(annotation);
            Requires(annotation.Count == 1);
            Requires(annotation == ScriptParser.Names.Group);

            return Compile(null, annotation[0]!, context)
                ?? throw new CompilationException("Cannot compile group.", annotation: annotation[0]);
        }

        /// <summary>
        /// Parses a finite number of terms, expressed in script by for instance [3..4]'foo'
        /// </summary>
        /// <param name="_"></param>
        /// <param name="annotation"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="CompilationException"></exception>
        public MatchCount<T> CompileRangedCount(Type? _, Annotation annotation, RuleCompilationContext context)
        {
            RequiresNotNull(annotation);
            Requires(annotation.Count == 3);
            
            var header = context.RuleHeader;

            RequiresNotNull(header);

            var min = int.Parse(context.GetText(annotation[0]!));
            var max = int.Parse(context.GetText(annotation[1]!));

            if (min > max && max > 0)
            {
                throw new CompilationException($"min ({min}) should be equal or larger than max ({max}).");
            }

            var subjectAnnotation = annotation[2]!;
            var subjectName = subjectAnnotation.GenerateUnnamedRuleName(context, header.Name, 0);
            var subjectHeader = new RuleHeader(AnnotationPruning.None, subjectName, 0, 0, false);

            var subject = 
                Compile(
                    _, 
                    annotation[2]!, 
                    new RuleCompilationContext(context, subjectHeader)
                ) as IRule
                ?? throw new CompilationException("Cannot compile subject for MatchCountRange.", subjectAnnotation);

            // By default unary (and binary) operators pass the result of the children because
            // in most cases we're interested in the values in the operation not the fact that there is an binary
            // operation.
            // The latter would result in much more overhead in specifying the parsers. Only when the rule is a 
            // toplevel rule (ie rule = a, b, c) we use the user specified output.
            var pruning =
                header.IsTopLevel
                    ? header.Prune
                    : AnnotationPruning.Root;

            return new MatchCount<T>(header.Name, pruning, header.Precedence, subject, min, max);
        }

        public MatchCount<T> CompileMatchCount(
            Type? _, 
            Annotation annotation, 
            RuleCompilationContext context,
            int min,
            int max
        ) 
        {
            RequiresNotNull(annotation);
            Requires(annotation.Count == 1);

            var header = context.RuleHeader;

            RequiresNotNull(header);

            var subjectAnnotation = annotation[0]!;
            var subjectName = subjectAnnotation.GenerateUnnamedRuleName(context, header.Name, 0);

            var subjectHeader = new RuleHeader(AnnotationPruning.None, subjectName, 0, 0, false);

            var subject =
                Compile(
                    _,
                    subjectAnnotation,
                    new RuleCompilationContext(context, subjectHeader)
                ) as IRule
                ?? throw new CompilationException("Cannot compile subject for MatchCount.", subjectAnnotation);

            // by default unary (and binary) operators pass the result of the children because
            // in most cases we're interested in the values in the operation not the fact that there is an binary operation.
            // The latter would result in much more overhead in specifying the parsers. Only when the rule is a 
            // toplevel rule (ie rule = a, b, c) we use the user specified output.
            var pruning =
                header.IsTopLevel
                    ? header.Prune
                    : AnnotationPruning.Root;

            return new MatchCount<T>(header.Name, pruning, header.Precedence, subject, min, max);
        }

        public MatchCount<T> CompileZeroOrMore(Type? _, Annotation annotation, RuleCompilationContext context) =>
            CompileMatchCount(_, annotation, context, 0, 0);

        public MatchCount<T> CompileZeroOrOne(Type? _, Annotation annotation, RuleCompilationContext context) =>
            CompileMatchCount(_, annotation, context, 0, 1);

        public MatchCount<T> CompileOneOrMore(Type? _, Annotation annotation, RuleCompilationContext context) =>
            CompileMatchCount(_, annotation, context, 1, 0);


        public IMetaRule CompileUnary(
            Type type, 
            Annotation annotation, 
            RuleCompilationContext context,
            params object[] creationParams
        )
        {
            RequiresNotNull(annotation);
            Requires(annotation.Count == 1);

            var header = context.RuleHeader;

            RequiresNotNull(header);

            // prune all if this is an inline rule as lookaheads never have a size
            // if it's a toplevel, it is up to the user 
            // UNLESS it's a rule reference which will override the pruning anyway
            var subjectAnnotation = annotation[0]!;
            var subjectName = subjectAnnotation.GenerateUnnamedRuleName(context, header.Name, 0);

            var subjectHeader = new RuleHeader(AnnotationPruning.None, subjectName, 0, 0, false);
            var pruning = header.IsTopLevel ? header.Prune : AnnotationPruning.All;

            var subject =
                Compile(
                    type,
                    subjectAnnotation,
                    new RuleCompilationContext(context, subjectHeader)
                ) as IRule
                ?? throw new CompilationException($"Cannot compile subject for {type}.", subjectAnnotation);

            IMetaRule? result;
            if (creationParams == null || creationParams.Length == 0)
            {
                result = (IMetaRule?)Activator.CreateInstance(type, header.Name, pruning, header.Precedence, subject);
            }
            else
            {
                result = (IMetaRule?)Activator.CreateInstance(type, [header.Name, pruning, header.Precedence, subject, .. creationParams]);
            }


            if (result == null)
            {
                throw new CompilationException($"Unable to create a rule of type {type} with the provided parameters.");
            }

            return result;
        }

        public MatchNot<T> CompileNot(Type? _, Annotation annotation, RuleCompilationContext context) =>
            (MatchNot<T>) CompileUnary(typeof(MatchNot<T>), annotation, context);
        
        public SkipRule<T> CompileStopAt(Type? _, Annotation annotation, RuleCompilationContext context) =>
            (SkipRule<T>)CompileUnary(typeof(SkipRule<T>), annotation, context, false, true);

        public SkipRule<T> CompileStopAfter(Type? _, Annotation annotation, RuleCompilationContext context) =>
            (SkipRule<T>)CompileUnary(typeof(SkipRule<T>), annotation, context, false, false);

        public SkipRule<T> CompileFind(Type? _, Annotation annotation, RuleCompilationContext context) =>
            (SkipRule<T>)CompileUnary(typeof(SkipRule<T>), annotation, context, true, true);

        public MatchCondition<T> CompileIf(Type? _, Annotation annotation, RuleCompilationContext context) =>
            (MatchCondition<T>)CompileUnary(typeof(MatchCondition<T>), annotation, context);

        public BreakPointRule<T> CompileBreak(Type? _, Annotation annotation, RuleCompilationContext context) =>
            (BreakPointRule<T>)CompileUnary(typeof(BreakPointRule<T>), annotation, context);

        public LogRule<T> CompileLog(Type? _, Annotation annotation, RuleCompilationContext context)
        {
            RequiresNotNull(annotation);
            Requires(annotation.Count >= 2);

            var header = context.RuleHeader;

            RequiresNotNull(header);

            var logLevelText = context.GetText(annotation[0]!);
            var logLevel = Enum.Parse<LogLevel>(logLevelText, ignoreCase: true);

            var message = context.GetText(annotation[1]!);

            if (string.IsNullOrEmpty(message) || message.Length < 2)
            {
                throw new CompilationException("LogText is missing (quotes).", annotation);
            }

            message = message[1..^1];

            IRule? condition = null;

            if (annotation.Count == 3)
            {
                var subjectAnnotation = annotation[2]!;
                var subjectName = subjectAnnotation.GenerateUnnamedRuleName(context, header.Name, 0);

                var subjectHeader = new RuleHeader(AnnotationPruning.None, subjectName, 0, 0, false);

                condition =
                    Compile(
                        _,
                        subjectAnnotation,
                        new RuleCompilationContext(context, subjectHeader)
                    ) as IRule
                    ?? throw new CompilationException($"Cannot compile subject for LogRule.", subjectAnnotation);
            }

            return new LogRule<T>(header.Name, header.Prune, condition, message, logLevel);
        }

        #endregion

        #region -- Private methods ------------------------------------------------------------------------------------

        /// <summary>
        /// Captures the rule header properties and body node
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ruleNodes">Nodes that make up the product, rulename, precendence and rulebody</param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static RuleHeader ReadRuleHeader(Annotation annotation, CompileContext context)
        {
            RequiresNotNull(annotation);
            RequiresNotNull(context);
            RequiresNotNull(annotation.Count >= 1);

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
                // xxx give good name
                && annotation[idx]! == "rule_precedence"
                && int.TryParse(context.GetText(annotation[idx]!), out precedence))
            {
                idx++;
            }

            return new(pruning, name, precedence, idx);
        }
 
        #endregion
    }
}
