using gg.parse.rulefunctions;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace gg.parse.compiler
{
    /// <summary>
    /// Functions used by the compiler which produce a rule either specifically for
    /// a tokenizer or a generic rulebase
    /// </summary>
    public static class CompilerFunctions
    {
        /// -- Compiler functions for a Tokenizer ---------------------------------------------------------------------
        
        public static RuleBase<char> CompileLiteral(
           Annotation ruleDefinition,
           RuleDeclaration declaration,
           CompileContext<char> context)
        {
            var literalText = context.GetText(ruleDefinition.Range);
            literalText = Regex.Unescape(literalText.Substring(1, literalText.Length - 2));

            if (string.IsNullOrEmpty(literalText))
            {
                // xxx add warnings
                // xxx resolve rule
                throw new CompilationException<char>("Literal text is empty", ruleDefinition.Range, null);
            }

            return context.Output.GetOrRegisterRule(declaration.Name, () =>
                            new MatchDataSequence<char>(declaration.Name, literalText.ToCharArray(), declaration.Product));
        }

        public static RuleBase<T> CompileIdentifier<T>(
           Annotation ruleDefinition,
           RuleDeclaration declaration,
           CompileContext<T> context) where T : IComparable<T>
        {
            var hasProductionOperator = (ruleDefinition.Children != null && ruleDefinition.Children.Count > 1);
            var referenceName = hasProductionOperator
                    // ref name contains a production
                    ? context.GetText(ruleDefinition.Children[1].Range)
                    // no operator, use the entire span
                    : context.GetText(ruleDefinition.Range);

            if (string.IsNullOrEmpty(referenceName))
            {
                // xxx add context errors if fatal
                // xxx resolve rule
                throw new CompilationException<char>("ReferenceName text is empty", ruleDefinition.Range, null);
            }

            var product = AnnotationProduct.Annotation;

            if (hasProductionOperator)
            {
                context.TryGetProduct(ruleDefinition.Children[0].FunctionId, out product);
            }

            return context.Output.GetOrRegisterRule(declaration.Name, () =>
                new RuleReference<T>(declaration.Name, referenceName, product));
        }

        public static RuleBase<char> CompileCharacterSet(
           Annotation ruleDefinition,
           RuleDeclaration declaration,
           CompileContext<char> context)
        {
            Contract.Requires(ruleDefinition != null);
            Contract.Requires(ruleDefinition!.Children != null);
            Contract.Requires(ruleDefinition.Children!.Count == 1);

            var setText = context.GetText(ruleDefinition.Children[0].Range);

            if (string.IsNullOrEmpty(setText) || setText.Length <= 2)
            {
                // xxx add context errors if fatal
                // xxx resolve rule
                throw new CompilationException<char>("Text defining the set text is null or empty", ruleDefinition.Range, null);
            }

            setText = Regex.Unescape(setText.Substring(1, setText.Length - 2));

            return context.Output.GetOrRegisterRule(declaration.Name, () =>
                new MatchDataSet<char>(declaration.Name, declaration.Product, setText.ToArray()));
        }

        public static RuleBase<char> CompileCharacterRange(
           Annotation ruleDefinition,
           RuleDeclaration declaration,
           CompileContext<char> context)
        {
            var lowerRange = ruleDefinition.Children[0].Range;
            var minText = context.GetText(lowerRange);

            if (minText.Length != 3)
            {
                throw new CompilationException<char>($"CompileCharacterRange: invalid range definition {minText}.",
                            context.GetTextRange(lowerRange),
                            // xxx resolve rule
                            null);
            }

            var upperRange = ruleDefinition.Children[1].Range;
            var maxText = context.GetText(upperRange);

            if (maxText.Length != 3)
            {
                throw new CompilationException<char>($"CompileCharacterRange: invalid range definition {maxText}.",
                            context.GetTextRange(upperRange),
                            // xxx resolve rule
                            null);
            }

            return context.Output.GetOrRegisterRule(declaration.Name, () =>
                // xxx parameter order...
                new MatchDataRange<char>(declaration.Name, minText[1], maxText[1], declaration.Product));
        }

        

        // -- Generic functions ---------------------------------------------------------------------------------------

        public static RuleBase<T> CompileSequence<T>(
            Annotation ruleDefinition,
            RuleDeclaration declaration,
            CompileContext<T> context) where T: IComparable<T>
        {
            var sequenceElements = new List<RuleBase<T>>();

            if (ruleDefinition.Children != null)
            {
                for (var i = 0; i < ruleDefinition.Children.Count; i++)
                {
                    var elementAnnotation = ruleDefinition.Children[i];
                    var compilationFunction = context.Functions[elementAnnotation.FunctionId];

                    var elementName = "";

                    if (context.Parser != null)
                    {
                        var elementFunction = context.Parser.FindRule(elementAnnotation.FunctionId);
                        elementName = elementFunction is RuleReference<T> refFunction
                                    ? refFunction.Reference
                                    : elementFunction.Name;
                    }

                    var elementDeclaration = new RuleDeclaration(AnnotationProduct.Transitive, $"{elementName}:{declaration.Name}[{i}]");
                    var sequenceElement = compilationFunction(elementAnnotation, elementDeclaration, context);

                    if (sequenceElement == null)
                    {
                        // xxx add context errors if fatal
                        // xxx resolve rule
                        throw new CompilationException<char>("Cannot compile rule definition for sequence.", elementAnnotation.Range, null);
                    }

                    sequenceElements.Add(sequenceElement);
                }
            }

            return context.Output.GetOrRegisterRule(declaration.Name, () =>
                            new MatchFunctionSequence<T>(declaration.Name, declaration.Product, [.. sequenceElements]));
        }

        public static RuleBase<T> CompileOption<T>(
            Annotation ruleDefinition,
            RuleDeclaration declaration,
            CompileContext<T> context) where T : IComparable<T>
        {
            var optionElements = new List<RuleBase<T>>();

            if (ruleDefinition.Children != null)
            {
                for (var i = 0; i < ruleDefinition.Children.Count; i++)
                {
                    var elementAnnotation = ruleDefinition.Children[i];
                    var compilationFunction = context.Functions[elementAnnotation.FunctionId];
                    var elementDeclaration = new RuleDeclaration(AnnotationProduct.Annotation, $"{declaration.Name}[{i}]");
                    var optionElement = compilationFunction(elementAnnotation, elementDeclaration, context);

                    if (optionElement == null)
                    {
                        // xxx add context errors if fatal
                        // xxx resolve rule
                        throw new CompilationException<char>("Cannot compile rule definition for option.", elementAnnotation.Range, null);
                    }

                    optionElements.Add(optionElement);
                }
            }

            return context.Output.GetOrRegisterRule(declaration.Name, () =>
                            new MatchOneOfFunction<T>(declaration.Name, declaration.Product, [.. optionElements]));
        }

        public static RuleBase<T> CompileGroup<T>(
            Annotation ruleDefinition,
            RuleDeclaration declaration,
            CompileContext<T> context) where T : IComparable<T>
        {
            Contract.Requires(ruleDefinition != null);
            Contract.Requires(ruleDefinition!.Children != null);
            Contract.Requires(ruleDefinition.Children!.Count > 0);

            var elementAnnotation = ruleDefinition.Children[0];
            var compilationFunction = context.Functions[elementAnnotation.FunctionId];

            return compilationFunction(elementAnnotation, declaration, context);
        }

        public static RuleBase<T> CompileZeroOrMore<T>(
            Annotation ruleDefinition,
            RuleDeclaration declaration,
            CompileContext<T> context) where T : IComparable<T> =>
            
            CompileCount(ruleDefinition, declaration, context, 0, 0);


        public static RuleBase<T> CompileOneOrMore<T>(
            Annotation ruleDefinition,
            RuleDeclaration declaration,
            CompileContext<T> context) where T : IComparable<T> =>

            CompileCount(ruleDefinition, declaration, context, 1, 0);

        public static RuleBase<T> CompileZeroOrOne<T>(
            Annotation ruleDefinition,
            RuleDeclaration declaration,
            CompileContext<T> context) where T : IComparable<T> =>

            CompileCount(ruleDefinition, declaration, context, 0, 1);

        public static RuleBase<T> CompileCount<T>(
            Annotation ruleDefinition,
            RuleDeclaration declaration,
            CompileContext<T> context,
            int min, int max) where T : IComparable<T>
        {
            Contract.Requires(ruleDefinition != null);
            Contract.Requires(ruleDefinition!.Children != null);
            Contract.Requires(ruleDefinition.Children!.Count > 0);

            var elementAnnotation = ruleDefinition.Children[0];
            var compilationFunction = context.Functions[elementAnnotation.FunctionId];
            // xxx add human understandable name instead of subfunction
            var elementDeclaration = new RuleDeclaration(AnnotationProduct.Transitive, $"{declaration.Name}(subFunction[{min},{max}])");
            var subFunction = compilationFunction(elementAnnotation, elementDeclaration, context);

            if (subFunction == null)
            {
                // xxx add context errors if fatal
                // xxx resolve rule
                throw new CompilationException<char>("Cannot compile subFunction definition for match count.", elementAnnotation.Range, null);
            }

            return context.Output.GetOrRegisterRule(declaration.Name, () =>
                            new MatchFunctionCount<T>(declaration.Name, subFunction, declaration.Product, min, max));
        }

        public static RuleBase<T> CompileNot<T>(
            Annotation ruleDefinition,
            RuleDeclaration declaration,
            CompileContext<T> context) where T : IComparable<T>
        {
            Contract.Requires(ruleDefinition != null);
            Contract.Requires(ruleDefinition!.Children != null);
            Contract.Requires(ruleDefinition.Children!.Count > 0);

            var elementAnnotation = ruleDefinition.Children[0];
            var compilationFunction = context.Functions[elementAnnotation.FunctionId];
            // xxx add human understandable name instead of subfunction
            var elementDeclaration = new RuleDeclaration(AnnotationProduct.Annotation, $"{declaration.Name}(~subFunction)");
            var subFunction = compilationFunction(elementAnnotation, elementDeclaration, context);

            if (subFunction == null)
            {
                // xxx add errors
                // xxx resolve rule
                throw new CompilationException<char>("Cannot compile subFunction definition for Not.", elementAnnotation.Range, null);
            }

            return context.Output.GetOrRegisterRule(declaration.Name, () =>
                            new MatchNotFunction<T>(declaration.Name, declaration.Product, subFunction));
        }

        public static RuleBase<T> CompileAny<T>(
            Annotation ruleDefinition,
            RuleDeclaration declaration,
            CompileContext<T> context) where T : IComparable<T>
        {
            Contract.Requires(ruleDefinition != null);
            
            return context.Output.GetOrRegisterRule(declaration.Name, () =>
                            new MatchAnyData<T>(declaration.Name, declaration.Product));
        }

        public static RuleBase<T> CompileError<T>(
           Annotation ruleDefinition,
           RuleDeclaration declaration,
           CompileContext<T> context) where T : IComparable<T>
        {
            Contract.Requires(ruleDefinition != null);
            Contract.Requires(ruleDefinition!.Children != null);
            Contract.Requires(ruleDefinition.Children!.Count > 0);

            var message = context.GetText(ruleDefinition.Children[1].Range);

            if (string.IsNullOrEmpty(message) || message.Length <= 2)
            {
                // xxx add warnings
                // xxx resolve rule
                throw new CompilationException<T>("Text defining the error is null or empty", ruleDefinition.Range, null);
            }

            message = message.Substring(1, message.Length - 2);

            var skipAnnotation = ruleDefinition.Children[2];
            var compilationFunction = context.Functions[skipAnnotation.FunctionId];
            // xxx add human understandable name instead of subfunction
            var skipDeclaration = new RuleDeclaration(AnnotationProduct.Annotation, $"{declaration.Name}(skip_until)");
            var testFunction = compilationFunction(skipAnnotation, skipDeclaration, context);

            if (testFunction == null)
            {
                // xxx add warnings
                // xxx resolve rule
                throw new CompilationException<char>("Cannot compile subFunction definition for Error.", skipAnnotation.Range, null);
            }

            return context.Output.GetOrRegisterRule(declaration.Name, () =>
                            new MarkError<T>(declaration.Name, declaration.Product, message, testFunction, 0));
        }

    }
}
