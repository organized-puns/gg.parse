// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Text.RegularExpressions;

using gg.parse.rules;
using gg.parse.util;

using static gg.parse.script.compiler.CompilerFunctionNameGenerator;

namespace gg.parse.script.compiler
{
    /// <summary>
    /// Functions used by the compiler which produce a rule either specifically for
    /// a tokenizer or a generic rulebase
    /// </summary>
    public static class CompilerFunctions
    {
        /// -- Compiler functions for a Tokenizer ---------------------------------------------------------------------
        
        public static RuleBase<char> CompileLiteral(RuleHeader header, Annotation bodyNode, CompileSession context)
        {
            var literalText = context.GetText(bodyNode.Range);
#pragma warning disable IDE0057 // Use range operator
            var unescapedLiteralText = Regex.Unescape(literalText.Substring(1, literalText.Length - 2));
#pragma warning restore IDE0057 // Use range operator

            if (string.IsNullOrEmpty(unescapedLiteralText))
            {
                throw new CompilationException("Literal text is empty (somehow...).", annotation: bodyNode);
            }

            return new MatchDataSequence<char>(header.Name, unescapedLiteralText.ToCharArray(), header.Prune, header.Precedence);
        }        

        public static RuleBase<char> CompileCharacterSet( RuleHeader header, Annotation bodyNode, CompileSession session)
        {
            Assertions.Requires(bodyNode != null);
            Assertions.Requires(bodyNode!.Children != null);
            Assertions.Requires(bodyNode.Children!.Count == 1);

            var setText = session.GetText(bodyNode.Children[0].Range);

            if (string.IsNullOrEmpty(setText) || setText.Length <= 2)
            {
                throw new CompilationException("Text defining the MatchDataSet text is null or empty", annotation: bodyNode);
            }

#pragma warning disable IDE0057 // Use range operator
            setText = Regex.Unescape(setText.Substring(1, setText.Length - 2));
#pragma warning restore IDE0057 // Use range operator

            return new MatchDataSet<char>(header.Name, header.Prune, [.. setText], header.Precedence);
        }

        public static RuleBase<char> CompileCharacterRange(RuleHeader declaration, Annotation bodyNode, CompileSession context)
        {
            Assertions.Requires(bodyNode != null);
            Assertions.Requires(bodyNode!.Children != null);

            var lowerRange = bodyNode.Children![0].Range;
            var minText = context.GetText(lowerRange);

            if (minText.Length != 3)
            {
                throw new CompilationException($"CompileCharacterRange: invalid range definition {minText}.",
                            annotation: bodyNode);
            }

            var upperRange = bodyNode.Children[1].Range;
            var maxText = context.GetText(upperRange);

            if (maxText.Length != 3)
            {
                throw new CompilationException($"CompileCharacterRange: invalid range definition {maxText}.",
                            annotation: bodyNode);
            }

            return new MatchDataRange<char>(
                declaration.Name, 
                minText[1], 
                maxText[1], 
                declaration.Prune, 
                declaration.Precedence
            );
        }

        // -- Generic functions ---------------------------------------------------------------------------------------

        public static RuleBase<T> CompileIdentifier<T>(RuleHeader declaration, Annotation bodyNode, CompileSession session) 
            where T : IComparable<T>
        {
            var hasOutputModifier = (bodyNode.Children != null && bodyNode.Children.Count > 1);
            var referenceName = hasOutputModifier
                    // ref name contains a output - take the name only 
                    ? session.GetText(bodyNode.Children![1].Range)
                    // no operator, use the entire span
                    : session.GetText(bodyNode.Range);

            if (string.IsNullOrEmpty(referenceName))
            {
                throw new CompilationException("ReferenceName text is empty (somehow...).", annotation: bodyNode);
            }

            var modifier = declaration.Prune;

            // xxx should raise a warning if product is anything else than annotation eg
            // the user specifies #rule = ~ref; the outcome for the product is ~ but that's
            // arbitrary. The user should either go #rule = ref or rule = ~ref...
            if (hasOutputModifier)
            {
                session.Compiler.TryMatchOutputModifier(bodyNode.Children![0]!.Rule.Id, out modifier);
            }

            return new RuleReference<T>(declaration.Name, referenceName, modifier, declaration.Precedence);
        }

        public static TRule CompileBinaryOperator<T, TRule>(RuleHeader header, Annotation body, CompileSession session) 
            where T : IComparable<T> where TRule : RuleBase<T>
        {
            RuleBase<T>[]? elementArray = null;

            if (body.Children != null)
            {
                elementArray = new RuleBase<T>[body.Children.Count];
                
                for (var i = 0; i < body.Children.Count; i++)
                {
                    var elementBody = body.Children[i];
                    var (compilationFunction, functionName) = session.Compiler.Functions[elementBody.Rule];

                    var elementName = elementBody.GenerateUnnamedRuleName(session, header.Name, i); 
                    var elementHeader = new RuleHeader(AnnotationPruning.None, elementName, 0, 0, false);

                    elementArray[i] = compilationFunction(elementHeader, elementBody, session) as RuleBase<T> 
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

            return (TRule) Activator.CreateInstance(
                typeof(TRule), 
                header.Name, 
                output, 
                header.Precedence, 
                elementArray
            )!;
        }

        public static RuleBase<T> CompileSequence<T>(
            RuleHeader header,
            Annotation bodyNode,
            CompileSession session) where T: IComparable<T> =>
        
            CompileBinaryOperator<T, MatchRuleSequence<T>>(header, bodyNode, session);
        
        public static RuleBase<T> CompileOption<T>(
            RuleHeader header,
            Annotation bodyNode,
            CompileSession session) where T : IComparable<T> =>
        
            CompileBinaryOperator<T, MatchOneOf<T>>(header, bodyNode, session);               

        public static RuleBase<T> CompileEvaluation<T>(
            RuleHeader header,
            Annotation bodyNode,
            CompileSession session) where T : IComparable<T> =>
        
            CompileBinaryOperator<T, MatchEvaluation<T>>(header, bodyNode,session);        

        public static RuleBase<T> CompileGroup<T>(
            RuleHeader header,
            Annotation bodyNode,
            CompileSession session) where T : IComparable<T>
        {
            Assertions.Requires(bodyNode != null);
            Assertions.Requires(bodyNode!.Children != null);
            Assertions.Requires(bodyNode.Children!.Count > 0);

            var (compilationFunction,_) = session.Compiler.Functions[bodyNode.Children[0].Rule];
            var groupDeclaration = new RuleHeader(header.Prune, header.Name, 0, 0);
            var result = compilationFunction(groupDeclaration, bodyNode.Children[0], session) as RuleBase<T>;

            // xxx needs more info
            return result ?? throw new CompilationException("Failed to compile group");
        }

        public static RuleBase<T> CompileCount<T>(
            RuleHeader header,
            Annotation bodyNode,
            CompileSession session,
            int min, 
            int max) where T : IComparable<T>
        {
            Assertions.Requires(bodyNode != null);
            Assertions.Requires(bodyNode!.Children != null);
            Assertions.Requires(bodyNode.Children!.Count > 0);

            var elementBody = bodyNode.Children[0];
            var (compilationFunction, _) = session.Compiler.Functions[elementBody.Rule];

            var elementName = elementBody.GenerateUnnamedRuleName(session, header.Name, 0);

            var elementHeader = new RuleHeader(AnnotationPruning.Root, elementName, 0, 0, false);

            if (compilationFunction(elementHeader, elementBody, session) is not RuleBase<T> countRule)
            {
                throw new CompilationException("Cannot compile countRule definition for MatchCount.", annotation: elementBody);
            }

            // by default unary (and binary) operators pass the result of the children because
            // in most cases we're interested in the values in the operation not the fact that there is an binary operation.
            // The latter would result in much more overhead in specifying the parsers. Only when the rule is a 
            // toplevel rule (ie rule = a, b, c) we use the user specified output.
            var output =
                header.IsTopLevel
                    ? header.Prune
                    : AnnotationPruning.Root;

            return new MatchCount<T>(header.Name, countRule, output, min, max, header.Precedence);
        }

        public static RuleBase<T> CompileZeroOrMore<T>(
            RuleHeader header,
            Annotation bodyNode,
            CompileSession session) where T : IComparable<T> =>
            
            CompileCount<T>(header, bodyNode, session, 0, 0);

        public static RuleBase<T> CompileOneOrMore<T>(
            RuleHeader header,
            Annotation bodyNode,
            CompileSession session) 
            where T : IComparable<T> =>

            CompileCount<T>(header, bodyNode, session, 1, 0);

        public static RuleBase<T> CompileZeroOrOne<T>(
            RuleHeader header,
            Annotation bodyNode,
            CompileSession session) 
            where T : IComparable<T> =>

            CompileCount<T>(header, bodyNode, session, 0, 1);


        public static TRule CompileUnary<T, TRule>(
            RuleHeader header,
            Annotation bodyNode,
            CompileSession session,
            params object[] creationParams)
            where T : IComparable<T>
            where TRule : RuleBase<T>
        {
            Assertions.Requires(bodyNode != null);
            Assertions.Requires(bodyNode!.Children != null);
            Assertions.Requires(bodyNode.Children!.Count > 0);

            var elementBody = bodyNode.Children[0];
            var (compilationFunction, _) = session.Compiler.Functions[elementBody.Rule];
            var elementName = elementBody.GenerateUnnamedRuleName(session, header.Name, 0);
            var elementHeader = new RuleHeader(AnnotationPruning.None, elementName);

            var unaryRule = compilationFunction(elementHeader, elementBody, session) as RuleBase<T>
                ?? throw new CompilationException($"Cannot compile unary rule definition for {typeof(TRule)}.", annotation: elementBody);

            TRule? result; 
            if (creationParams == null || creationParams.Length == 0)
            {
                result = (TRule?) Activator.CreateInstance(typeof(TRule), header.Name, header.Prune, header.Precedence, unaryRule);
            }
            else
            {
                result = (TRule?) Activator.CreateInstance(typeof(TRule), [header.Name, header.Prune, header.Precedence, unaryRule, .. creationParams]);
            }

            if (result == null)
            {
                throw new CompilationException($"Unable to create a rule of type {typeof(TRule)} with the provided parameters.");
            }

            return result;
        }

        public static RuleBase<T> CompileNot<T>(
            RuleHeader header,
            Annotation bodyNode,
            CompileSession session) 
            where T : IComparable<T> =>
        
            CompileUnary<T, MatchNot<T>>(header, bodyNode, session);
        

        public static RuleBase<T> CompileSkip<T>(
            RuleHeader header,
            Annotation bodyNode,
            CompileSession session) 
            where T : IComparable<T> =>
        
            CompileUnary<T, SkipRule<T>>(header, bodyNode, session, false);

        public static RuleBase<T> CompileFind<T>(
            RuleHeader header, 
            Annotation bodyNode,
            CompileSession session) 
            where T : IComparable<T> =>
        
            CompileUnary<T, SkipRule<T>>(header, bodyNode, session, true);
        

        public static RuleBase<T> CompileTryMatch<T>(
            RuleHeader header,
            Annotation bodyNode,
            CompileSession session) where T : IComparable<T> =>
        
        CompileUnary<T, MatchCondition<T>>(header, bodyNode, session);
        

        public static RuleBase<T> CompileAny<T>(RuleHeader header, Annotation _, CompileSession __) 
            where T : IComparable<T>
        {
            Assertions.RequiresNotNull(header);

            return new MatchAnyData<T>(header.Name, header.Prune, precedence: header.Precedence);
        }

        public static RuleBase<T> CompileLog<T>(
            RuleHeader header, 
            Annotation bodyNode,
            CompileSession session) 
            where T : IComparable<T>
        {
            Assertions.Requires(bodyNode != null);
            Assertions.Requires(bodyNode!.Children != null);
            Assertions.Requires(bodyNode.Children!.Count > 0);

            var logLevelText = session.GetText(bodyNode.Children[0].Range);
            var logLevel = Enum.Parse<LogLevel>(logLevelText, ignoreCase: true);

            var message = session.GetText(bodyNode.Children[1].Range);

            if (string.IsNullOrEmpty(message) || message.Length < 2)
            {
                throw new CompilationException("LogText is missing (quotes).",
                    annotation: bodyNode);
            }

#pragma warning disable IDE0057 // Use range operator
            message = message.Substring(1, message.Length - 2);
#pragma warning restore IDE0057 // Use range operator

            RuleBase<T>? condition = null;

            if (bodyNode.Children.Count == 3)
            {
                var elementBody = bodyNode.Children[2];
                var (compilationFunction, _) = session.FindFunction(elementBody.Rule);

                if (compilationFunction != null)
                {
                    var elementName = elementBody.GenerateUnnamedRuleName(session, header.Name, 0);
                    var conditionHeader = new RuleHeader(AnnotationPruning.None, elementName);

                    condition = compilationFunction(conditionHeader, elementBody, session) as RuleBase<T>
                        ?? throw new CompilationException("Cannot compile condition for Log.", annotation: elementBody);
                }
                else
                {
                    throw new CompilationException($"Could not find a compilation function for rule: '{elementBody.Rule}'");
                }
            }

            return new LogRule<T>(header.Name, header.Prune, message, condition, logLevel);
        }
    }
}
