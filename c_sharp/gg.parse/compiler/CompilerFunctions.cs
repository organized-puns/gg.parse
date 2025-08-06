using gg.parse.rulefunctions;
using System.Diagnostics.Contracts;

namespace gg.parse.compiler
{
    /// <summary>
    /// Functions used by the compiler which produce a rule either specifically for
    /// a tokenizer or a generic rulebase
    /// </summary>
    public static class CompilerFunctions
    {
        /// -- Tokenizer specific functions ---------------------------------------------------------------------------
        
        public static RuleBase<char> CompileLiteral(
           Annotation ruleDefinition,
           RuleDeclaration declaration,
           CompileContext<char> context)
        {
            var literalText = context.GetText(ruleDefinition.Range);
            literalText = literalText.Substring(1, literalText.Length - 2);

            return context.Output.GetOrRegisterRule(declaration.Name, () =>
                            new MatchDataSequence<char>(declaration.Name, literalText.ToCharArray(), declaration.Product));
        }

        public static RuleBase<char> CompileIdentifier(
           Annotation ruleDefinition,
           RuleDeclaration declaration,
           CompileContext<char> context)
        {
            var referenceName = context.GetText(ruleDefinition.Range);

            return context.Output.GetOrRegisterRule(declaration.Name, () =>
                new RuleReference<char>(declaration.Name, referenceName));
        }

        public static RuleBase<char> CompileCharacterSet(
           Annotation ruleDefinition,
           RuleDeclaration declaration,
           CompileContext<char> context)
        {
            var setText = context.GetText(ruleDefinition.Range);
            setText = setText.Substring(1, setText.Length - 2);

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
                // parameter order...
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
                    var elementDeclaration = new RuleDeclaration(AnnotationProduct.Annotation, $"{declaration.Name}[{i}]");
                    var sequenceElement = compilationFunction(elementAnnotation, elementDeclaration, context);

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
            var elementDeclaration = new RuleDeclaration(AnnotationProduct.Annotation, $"{declaration.Name}(subFunction[{min},{max}])");
            var subFunction = compilationFunction(elementAnnotation, elementDeclaration, context);

            return context.Output.GetOrRegisterRule(declaration.Name, () =>
                            new MatchFunctionCount<T>(declaration.Name, subFunction, declaration.Product, min, max));
        }

        // xxx to test
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

            return context.Output.GetOrRegisterRule(declaration.Name, () =>
                            new MatchNotFunction<T>(declaration.Name, declaration.Product, subFunction));
        }
    }
}
