using System.Text.RegularExpressions;

using gg.core.util;
using gg.parse.rulefunctions;
using gg.parse.rulefunctions.datafunctions;
using gg.parse.rulefunctions.rulefunctions;
using static gg.parse.rulefunctions.CommonRules;

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
           RuleCompiler<char> compiler,
           RuleDeclaration declaration,
           CompileSession<char> context)
        {
            var ruleDefinition = declaration.AssociatedAnnotation;
            var literalText = context.GetText(ruleDefinition.Range);
            var unescapedLiteralText = Regex.Unescape(literalText.Substring(1, literalText.Length - 2));

            if (string.IsNullOrEmpty(unescapedLiteralText))
            {
                // xxx add warnings
                // xxx resolve rule
                throw new CompilationException<char>("Literal text is empty", ruleDefinition.Range, null);
            }

            return new MatchDataSequence<char>(declaration.Name, unescapedLiteralText.ToCharArray(), declaration.Product, declaration.Precedence);
        }

        public static RuleBase<T> CompileIdentifier<T>(
           RuleCompiler<T> compiler,
           RuleDeclaration declaration,
           CompileSession<T> session) where T : IComparable<T>
        {
            var ruleDefinition = declaration.AssociatedAnnotation;
            var hasProductionOperator = (ruleDefinition.Children != null && ruleDefinition.Children.Count > 1);
            var referenceName = hasProductionOperator
                    // ref name contains a production
                    ? session.GetText(ruleDefinition.Children[1].Range)
                    // no operator, use the entire span
                    : session.GetText(ruleDefinition.Range);

            if (string.IsNullOrEmpty(referenceName))
            {
                // xxx add context errors if fatal
                // xxx resolve rule
                throw new CompilationException<char>("ReferenceName text is empty", ruleDefinition.Range, null);
            }

            var product = declaration.Product;

            // xxx shouldn't raise a warning if product is anything else than annotation eg
            // the user specifies #rule = ~ref; the outcome for the product is ~ but that's
            // arbitrary. The user should either go #rule = ref or rule = ~ref...
            if (hasProductionOperator)
            {
                compiler.TryGetProduct(ruleDefinition.Children[0].RuleId, out product);
            }

            return new RuleReference<T>(declaration.Name, referenceName, product, declaration.Precedence);
        }

        public static RuleBase<char> CompileCharacterSet(
           RuleCompiler<char> compiler,
           RuleDeclaration declaration,
           CompileSession<char> session)
        {
            var ruleDefinition = declaration.AssociatedAnnotation;

            Contract.Requires(ruleDefinition != null);
            Contract.Requires(ruleDefinition!.Children != null);
            Contract.Requires(ruleDefinition.Children!.Count == 1);

            var setText = session.GetText(ruleDefinition.Children[0].Range);

            if (string.IsNullOrEmpty(setText) || setText.Length <= 2)
            {
                // xxx add context errors if fatal
                // xxx resolve rule
                throw new CompilationException<char>("Text defining the set text is null or empty", ruleDefinition.Range, null);
            }

            setText = Regex.Unescape(setText.Substring(1, setText.Length - 2));

            return new MatchDataSet<char>(declaration.Name, declaration.Product, setText.ToArray(), declaration.Precedence);
        }

        public static RuleBase<char> CompileCharacterRange(
           RuleCompiler<char> compiler,
           RuleDeclaration declaration,
           CompileSession<char> context)
        {
            var ruleDefinition = declaration.AssociatedAnnotation;
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

            return 
                // xxx parameter order...
                new MatchDataRange<char>(declaration.Name, minText[1], maxText[1], declaration.Product, declaration.Precedence);
        }
       
        // -- Generic functions ---------------------------------------------------------------------------------------

        // xxx check the overlap with option and eval (and club together)
        public static RuleBase<T> CompileSequence<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession<T> session) where T: IComparable<T>
        {
            var sequenceElements = new List<RuleBase<T>>();
            var ruleDefinition = declaration.AssociatedAnnotation;

            if (ruleDefinition.Children != null)
            {
                for (var i = 0; i < ruleDefinition.Children.Count; i++)
                {
                    var elementAnnotation = ruleDefinition.Children[i];
                    var (compilationFunction, elementName) = compiler.Functions[elementAnnotation.RuleId];

                    var elementDeclaration = new RuleDeclaration(elementAnnotation, AnnotationProduct.Transitive, $"{declaration.Name}[{i}], type: {elementName}");
                    var sequenceElement = compilationFunction(compiler, elementDeclaration, session);

                    if (sequenceElement == null)
                    {
                        // xxx add context errors if fatal
                        // xxx resolve rule
                        throw new CompilationException<char>("Cannot compile rule definition for sequence.", elementAnnotation.Range, null);
                    }

                    sequenceElements.Add(sequenceElement);
                }
            }

            return new MatchFunctionSequence<T>(declaration.Name, declaration.Product, declaration.Precedence, [.. sequenceElements]);
        }

        public static RuleBase<T> CompileOption<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession<T> context) where T : IComparable<T>
        {
            var optionElements = new List<RuleBase<T>>();
            var ruleDefinition = declaration.AssociatedAnnotation;

            if (ruleDefinition.Children != null)
            {
                for (var i = 0; i < ruleDefinition.Children.Count; i++)
                {
                    var elementAnnotation = ruleDefinition.Children[i];
                    var (compilationFunction, elementName) = compiler.FindCompilationFunction(elementAnnotation.RuleId);
                    var elementDeclaration = new RuleDeclaration(elementAnnotation,AnnotationProduct.Annotation, $"{declaration.Name}[{i}], type: {elementName}");
                    var optionElement = compilationFunction(compiler, elementDeclaration, context);

                    if (optionElement == null)
                    {
                        // xxx add context errors if fatal
                        // xxx resolve rule
                        throw new CompilationException<char>("Cannot compile rule definition for option.", elementAnnotation.Range, null);
                    }

                    optionElements.Add(optionElement);
                }
            }

            return new MatchOneOfFunction<T>(declaration.Name, declaration.Product, declaration.Precedence, [.. optionElements]);
        }

        

        public static RuleBase<T> CompileEvaluation<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession<T> session) where T : IComparable<T>
        {
            var evaluationElements = new List<RuleBase<T>>();
            var ruleDefinition = declaration.AssociatedAnnotation;

            if (ruleDefinition.Children != null)
            {
                for (var i = 0; i < ruleDefinition.Children.Count; i++)
                {
                    var elementAnnotation = ruleDefinition.Children[i];
                    var (compilationFunction, elementName) = compiler.Functions[elementAnnotation.RuleId];

                    var elementDeclaration = new RuleDeclaration(elementAnnotation, AnnotationProduct.Transitive, $"{declaration.Name}[{i}], type: {elementName}");
                    var elementFunction = 
                        compilationFunction(compiler, elementDeclaration, session) 
                        ?? throw new CompilationException<char>($"Compiling evaluation, can't find function for element at {elementAnnotation.Range}.", elementAnnotation.Range, null);

                    evaluationElements.Add(elementFunction);
                }
            }

            return new MatchEvaluation<T>(declaration.Name, declaration.Product, declaration.Precedence, [.. evaluationElements]);
        }

        public static RuleBase<T> CompileGroup<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession<T> context) where T : IComparable<T>
        {
            var ruleDefinition = declaration.AssociatedAnnotation;

            Contract.Requires(ruleDefinition != null);
            Contract.Requires(ruleDefinition!.Children != null);
            Contract.Requires(ruleDefinition.Children!.Count > 0);

            var elementAnnotation = ruleDefinition.Children[0];
            var (compilationFunction,_) = compiler.Functions[elementAnnotation.RuleId];
            var groupDeclaration = new RuleDeclaration(elementAnnotation, declaration.Product, declaration.Name);

            return compilationFunction(compiler, groupDeclaration, context);
        }

        public static RuleBase<T> CompileZeroOrMore<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession<T> context) where T : IComparable<T> =>
            
            CompileCount(compiler, declaration, context, 0, 0);


        public static RuleBase<T> CompileOneOrMore<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession<T> context) where T : IComparable<T> =>

            CompileCount(compiler, declaration, context, 1, 0);

        public static RuleBase<T> CompileZeroOrOne<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession<T> session) where T : IComparable<T> =>

            CompileCount(compiler, declaration, session, 0, 1);

        public static RuleBase<T> CompileCount<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession<T> session,
            int min, int max) where T : IComparable<T>
        {
            var ruleDefinition = declaration.AssociatedAnnotation;

            Contract.Requires(ruleDefinition != null);
            Contract.Requires(ruleDefinition!.Children != null);
            Contract.Requires(ruleDefinition.Children!.Count > 0);

            var elementAnnotation = ruleDefinition.Children[0];
            var (compilationFunction, elementName) = compiler.Functions[elementAnnotation.RuleId];
            var elementDeclaration = new RuleDeclaration(elementAnnotation,AnnotationProduct.Transitive, $"{declaration.Name} of {elementName}");
            var subFunction = compilationFunction(compiler, elementDeclaration, session);

            if (subFunction == null)
            {
                // xxx add context errors if fatal
                // xxx resolve rule
                throw new CompilationException<char>("Cannot compile subFunction definition for match count.", elementAnnotation.Range, null);
            }

            return new MatchFunctionCount<T>(declaration.Name, subFunction, declaration.Product, min, max, declaration.Precedence);
        }

        public static RuleBase<T> CompileNot<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession<T> session) where T : IComparable<T>
        {
            var ruleDefinition = declaration.AssociatedAnnotation;

            Contract.Requires(ruleDefinition != null);
            Contract.Requires(ruleDefinition!.Children != null);
            Contract.Requires(ruleDefinition.Children!.Count > 0);

            var elementAnnotation = ruleDefinition.Children[0];
            var (compilationFunction, elementName) = compiler.Functions[elementAnnotation.RuleId];
            // xxx add human understandable name instead of subfunction
            var elementDeclaration = new RuleDeclaration(elementAnnotation, AnnotationProduct.Annotation, $"{declaration.Name}, type: Not({elementName})");
            var subFunction = compilationFunction(compiler, elementDeclaration, session);

            if (subFunction == null)
            {
                // xxx add errors
                // xxx resolve rule
                throw new CompilationException<char>("Cannot compile subFunction definition for Not.", elementAnnotation.Range, null);
            }

            return new MatchNotFunction<T>(declaration.Name, declaration.Product, subFunction, declaration.Precedence);
        }

        public static RuleBase<T> CompileTryMatch<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession<T> session) where T : IComparable<T>
        {
            var ruleDefinition = declaration.AssociatedAnnotation;

            Contract.Requires(ruleDefinition != null);
            Contract.Requires(ruleDefinition!.Children != null);
            Contract.Requires(ruleDefinition.Children!.Count > 0);

            var elementAnnotation = ruleDefinition.Children[0];
            var (compilationFunction, elementName) = compiler.Functions[elementAnnotation.RuleId];
            // xxx add human understandable name instead of subfunction
            var elementDeclaration = new RuleDeclaration(elementAnnotation, AnnotationProduct.Annotation, $"{elementName}, type: {declaration.Name}");
            var subFunction = compilationFunction(compiler, elementDeclaration, session);

            if (subFunction == null)
            {
                // xxx add errors
                // xxx resolve rule
                throw new CompilationException<char>("Cannot compile subFunction definition for Try match.", elementAnnotation.Range, null);
            }

            return new TryMatchFunction<T>(declaration.Name, declaration.Product, subFunction, declaration.Precedence);
        }

        public static RuleBase<T> CompileAny<T>(
            RuleCompiler<T> _,
            RuleDeclaration declaration,
            CompileSession<T> __) where T : IComparable<T>
        {
            Contract.Requires(declaration != null);

            return new MatchAnyData<T>(declaration.Name, declaration.Product, precedence: declaration.Precedence);
        }

        public static RuleBase<T> CompileError<T>(
           RuleCompiler<T> compiler,
           RuleDeclaration declaration,
           CompileSession<T> context) where T : IComparable<T>
        {
            var ruleDefinition = declaration.AssociatedAnnotation;

            Contract.Requires(ruleDefinition != null);
            Contract.Requires(ruleDefinition!.Children != null);
            Contract.Requires(ruleDefinition.Children!.Count > 0);

            var message = context.GetText(ruleDefinition.Children[1].Range);

            if (string.IsNullOrEmpty(message) || message.Length <= 2)
            {
                throw new CompilationException<T>("Text defining the error is null or empty", 
                    ruleDefinition.Range, 
                    annotation: declaration.AssociatedAnnotation);
            }

            message = message.Substring(1, message.Length - 2);

            var skipAnnotation = ruleDefinition.Children[2];
            var (compilationFunction, elementName) = compiler.Functions[skipAnnotation.RuleId];
            var skipDeclaration = new RuleDeclaration(skipAnnotation, AnnotationProduct.Annotation, $"{declaration.Name} skip_until {elementName}");
            var testFunction = compilationFunction(compiler, skipDeclaration, context);

            if (testFunction == null)
            {
                // xxx add warnings
                // xxx resolve rule
                throw new CompilationException<char>("Cannot compile subFunction definition for Error.", skipAnnotation.Range, null);
            }

            return new MarkError<T>(declaration.Name, declaration.Product, message, testFunction, 0);
        }

        public static RuleBase<T> CompileLog<T>(
           RuleCompiler<T> compiler,
           RuleDeclaration declaration,
           CompileSession<T> context) where T : IComparable<T>
        {
            var ruleDefinition = declaration.AssociatedAnnotation;

            Contract.Requires(ruleDefinition != null);
            Contract.Requires(ruleDefinition!.Children != null);
            Contract.Requires(ruleDefinition.Children!.Count > 0);

            var logLevelText = context.GetText(ruleDefinition.Children[0].Range);
            var logLevel = Enum.Parse<LogLevel>(logLevelText, ignoreCase: true);

            var message = context.GetText(ruleDefinition.Children[1].Range);

            if (string.IsNullOrEmpty(message) || message.Length < 2)
            {
                throw new CompilationException<T>("LogText is missing (quotes).",
                    ruleDefinition.Range,
                    annotation: declaration.AssociatedAnnotation);
            }

            message = message.Substring(1, message.Length - 2);

            RuleBase<T>? condition = null;

            if (ruleDefinition.Children.Count == 3)
            {
                var conditionDefinition = ruleDefinition.Children[2];
                var (compilationFunction, elementName) = compiler.Functions[conditionDefinition.RuleId];
                var conditionDeclaration = new RuleDeclaration(conditionDefinition, AnnotationProduct.Annotation, $"{declaration.Name} skip_until {elementName}");
                condition = compilationFunction(compiler, conditionDeclaration, context);

                if (condition == null)
                {
                    // xxx add warnings
                    // xxx resolve rule
                    throw new CompilationException<char>("Cannot compile condition for Log.", conditionDefinition.Range, null);
                }
            }

            return new LogRule<T>(declaration.Name, declaration.Product, message, condition, logLevel);
        }
    }
}
