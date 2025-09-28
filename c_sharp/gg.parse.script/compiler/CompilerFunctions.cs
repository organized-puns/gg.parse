using System.Text.RegularExpressions;

using gg.parse.rules;

namespace gg.parse.script.compiler
{
    /// <summary>
    /// Functions used by the compiler which produce a rule either specifically for
    /// a tokenizer or a generic rulebase
    /// </summary>
    public static class CompilerFunctions
    {
        /// -- Compiler functions for a Tokenizer ---------------------------------------------------------------------
        
        public static RuleBase<char> CompileLiteral(
           RuleCompiler<char> _,
           RuleDeclaration declaration,
           CompileSession context)
        {
            var ruleDefinition = declaration.RuleBodyAnnotation;
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
           CompileSession session) where T : IComparable<T>
        {
            var ruleDefinition = declaration.RuleBodyAnnotation;
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
                compiler.TryGetProduct(ruleDefinition.Children[0]!.Rule.Id, out product);
            }
            else
            {
                product = IRule.Output.Self;
            }

            return new RuleReference<T>(declaration.Name, referenceName, product, declaration.Precedence);
        }

        public static RuleBase<char> CompileCharacterSet(
           RuleCompiler<char> compiler,
           RuleDeclaration declaration,
           CompileSession session)
        {
            var ruleDefinition = declaration.RuleBodyAnnotation;

            Assertions.Requires(ruleDefinition != null);
            Assertions.Requires(ruleDefinition!.Children != null);
            Assertions.Requires(ruleDefinition.Children!.Count == 1);

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
           CompileSession context)
        {
            var ruleDefinition = declaration.RuleBodyAnnotation;
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

        public static TRule CompileBinaryOperator<T, TRule>(
    RuleCompiler<T> compiler,
    RuleDeclaration declaration,
    CompileSession session) where T : IComparable<T> where TRule : RuleBase<T>
        {
            RuleBase<T>[] elementArray = null;
            var ruleDefinition = declaration.RuleBodyAnnotation;

            if (ruleDefinition.Children != null)
            {
                elementArray = new RuleBase<T>[ruleDefinition.Children.Count];
                for (var i = 0; i < ruleDefinition.Children.Count; i++)
                {
                    var elementAnnotation = ruleDefinition.Children[i];
                    var (compilationFunction, elementName) = compiler.Functions[elementAnnotation.Rule.Id];

                    var elementDeclaration =
                        new RuleDeclaration(
                            IRule.Output.Children,
                            $"{declaration.Name}[{i}], type: {elementName}",
                            0,
                            elementAnnotation
                        );

                    var element = compilationFunction(compiler, elementDeclaration, session);

                    if (element == null)
                    {
                        // xxx add context errors if fatal
                        // xxx resolve rule
                        throw new CompilationException<char>("Cannot compile rule definition for sequence.", elementAnnotation.Range, null);
                    }

                    elementArray[i] = element;
                }
            }

            return (TRule)Activator.CreateInstance(typeof(TRule), declaration.Name, declaration.Product, declaration.Precedence, elementArray);
        }

        public static RuleBase<T> CompileSequence<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession session) where T: IComparable<T> =>
        
            CompileBinaryOperator<T, MatchRuleSequence<T>>(compiler, declaration, session);
        

        public static RuleBase<T> CompileOption<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession session) where T : IComparable<T> =>
        
            CompileBinaryOperator<T, MatchOneOf<T>>(compiler, declaration, session);
        
       

        public static RuleBase<T> CompileEvaluation<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession session) where T : IComparable<T> =>
        
            CompileBinaryOperator<T, MatchEvaluation<T>>(compiler, declaration, session);
        

        public static RuleBase<T> CompileGroup<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession context) where T : IComparable<T>
        {
            var ruleDefinition = declaration.RuleBodyAnnotation;

            Assertions.Requires(ruleDefinition != null);
            Assertions.Requires(ruleDefinition!.Children != null);
            Assertions.Requires(ruleDefinition.Children!.Count > 0);

            var elementAnnotation = ruleDefinition.Children[0];
            var (compilationFunction,_) = compiler.Functions[elementAnnotation.Rule.Id];
            var groupDeclaration = new RuleDeclaration(declaration.Product, declaration.Name, elementAnnotation);

            return compilationFunction(compiler, groupDeclaration, context);
        }

        public static RuleBase<T> CompileZeroOrMore<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession context) where T : IComparable<T> =>
            
            CompileCount(compiler, declaration, context, 0, 0);


        public static RuleBase<T> CompileOneOrMore<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession context) where T : IComparable<T> =>

            CompileCount(compiler, declaration, context, 1, 0);

        public static RuleBase<T> CompileZeroOrOne<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession session) where T : IComparable<T> =>

            CompileCount(compiler, declaration, session, 0, 1);

        public static RuleBase<T> CompileCount<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession session,
            int min, int max) where T : IComparable<T>
        {
            var ruleDefinition = declaration.RuleBodyAnnotation;

            Assertions.Requires(ruleDefinition != null);
            Assertions.Requires(ruleDefinition!.Children != null);
            Assertions.Requires(ruleDefinition.Children!.Count > 0);

            var elementAnnotation = ruleDefinition.Children[0];
            var (compilationFunction, elementName) = compiler.Functions[elementAnnotation.Rule.Id];
            var elementDeclaration = new RuleDeclaration(IRule.Output.Children, $"{declaration.Name} of {elementName}", elementAnnotation);
            var subFunction = compilationFunction(compiler, elementDeclaration, session);

            if (subFunction == null)
            {
                // xxx add context errors if fatal
                // xxx resolve rule
                throw new CompilationException<char>("Cannot compile subFunction definition for match count.", elementAnnotation.Range, null);
            }

            return new MatchCount<T>(declaration.Name, subFunction, declaration.Product, min, max, declaration.Precedence);
        }

        public static RuleBase<T> CompileNot<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession session) where T : IComparable<T>
        {
            var ruleDefinition = declaration.RuleBodyAnnotation;

            Assertions.Requires(ruleDefinition != null);
            Assertions.Requires(ruleDefinition!.Children != null);
            Assertions.Requires(ruleDefinition.Children!.Count > 0);

            var elementAnnotation = ruleDefinition.Children[0];
            var (compilationFunction, elementName) = compiler.Functions[elementAnnotation.Rule.Id];
            // xxx add human understandable name instead of subfunction
            var elementDeclaration = new RuleDeclaration(IRule.Output.Self, $"{declaration.Name}, type: Not({elementName})", elementAnnotation);
            var subFunction = compilationFunction(compiler, elementDeclaration, session);

            if (subFunction == null)
            {
                // xxx add errors
                // xxx resolve rule
                throw new CompilationException<char>("Cannot compile subFunction definition for Not.", elementAnnotation.Range, null);
            }

            return new MatchNot<T>(declaration.Name, declaration.Product, subFunction, declaration.Precedence);
        }

        // xxx unary functions are copy/pasting code, could roll these up
        public static RuleBase<T> CompileSkip<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession session) where T : IComparable<T>
        {
            var ruleDefinition = declaration.RuleBodyAnnotation;

            Assertions.Requires(ruleDefinition != null);
            Assertions.Requires(ruleDefinition!.Children != null);
            Assertions.Requires(ruleDefinition.Children!.Count > 0);

            var elementAnnotation = ruleDefinition.Children[0];
            var (compilationFunction, elementName) = compiler.Functions[elementAnnotation.Rule.Id];
            // xxx add human understandable name instead of subfunction
            var elementDeclaration = new RuleDeclaration(IRule.Output.Self, $"{declaration.Name}, type: Skip({elementName})", elementAnnotation);
            var subFunction = compilationFunction(compiler, elementDeclaration, session);

            if (subFunction == null)
            {
                // xxx add errors
                // xxx resolve rule
                throw new CompilationException<char>("Cannot compile subFunction definition for Skip.", elementAnnotation.Range, null);
            }

            return new SkipRule<T>(declaration.Name, declaration.Product, subFunction, failOnEof: false);
        }

        public static RuleBase<T> CompileFind<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession session) where T : IComparable<T>
        {
            var ruleDefinition = declaration.RuleBodyAnnotation;

            Assertions.Requires(ruleDefinition != null);
            Assertions.Requires(ruleDefinition!.Children != null);
            Assertions.Requires(ruleDefinition.Children!.Count > 0);

            var elementAnnotation = ruleDefinition.Children[0];
            var (compilationFunction, elementName) = compiler.Functions[elementAnnotation.Rule.Id];
            // xxx add human understandable name instead of subfunction
            var elementDeclaration = new RuleDeclaration(IRule.Output.Self, $"{declaration.Name}, type: Find({elementName})", elementAnnotation);
            var subFunction = compilationFunction(compiler, elementDeclaration, session);

            if (subFunction == null)
            {
                // xxx add errors
                // xxx resolve rule
                throw new CompilationException<char>("Cannot compile subFunction definition for Find.", elementAnnotation.Range, null);
            }

            return new SkipRule<T>(declaration.Name, declaration.Product, subFunction, failOnEof: true);
        }

        public static RuleBase<T> CompileTryMatch<T>(
            RuleCompiler<T> compiler,
            RuleDeclaration declaration,
            CompileSession session) where T : IComparable<T>
        {
            var ruleDefinition = declaration.RuleBodyAnnotation;

            Assertions.Requires(ruleDefinition != null);
            Assertions.Requires(ruleDefinition!.Children != null);
            Assertions.Requires(ruleDefinition.Children!.Count > 0);

            var elementAnnotation = ruleDefinition.Children[0];
            var (compilationFunction, elementName) = compiler.Functions[elementAnnotation.Rule.Id];
            // xxx add human understandable name instead of subfunction
            var elementDeclaration = new RuleDeclaration(IRule.Output.Self, $"{elementName}, type: {declaration.Name}", elementAnnotation);
            var subFunction = compilationFunction(compiler, elementDeclaration, session);

            if (subFunction == null)
            {
                // xxx add errors
                // xxx resolve rule
                throw new CompilationException<char>("Cannot compile subFunction definition for Try match.", elementAnnotation.Range, null);
            }

            return new MatchCondition<T>(declaration.Name, declaration.Product, subFunction, declaration.Precedence);
        }

        public static RuleBase<T> CompileAny<T>(
            RuleCompiler<T> _,
            RuleDeclaration declaration,
            CompileSession __) where T : IComparable<T>
        {
            Assertions.Requires(declaration != null);

            return new MatchAnyData<T>(declaration.Name, declaration.Product, precedence: declaration.Precedence);
        }

        public static RuleBase<T> CompileLog<T>(
           RuleCompiler<T> compiler,
           RuleDeclaration declaration,
           CompileSession context) where T : IComparable<T>
        {
            var ruleDefinition = declaration.RuleBodyAnnotation;

            Assertions.Requires(ruleDefinition != null);
            Assertions.Requires(ruleDefinition!.Children != null);
            Assertions.Requires(ruleDefinition.Children!.Count > 0);

            var logLevelText = context.GetText(ruleDefinition.Children[0].Range);
            var logLevel = Enum.Parse<LogLevel>(logLevelText, ignoreCase: true);

            var message = context.GetText(ruleDefinition.Children[1].Range);

            if (string.IsNullOrEmpty(message) || message.Length < 2)
            {
                throw new CompilationException<T>("LogText is missing (quotes).",
                    ruleDefinition.Range,
                    annotation: declaration.RuleBodyAnnotation);
            }

            message = message.Substring(1, message.Length - 2);

            RuleBase<T>? condition = null;

            if (ruleDefinition.Children.Count == 3)
            {
                var conditionDefinition = ruleDefinition.Children[2];
                var (compilationFunction, elementName) = compiler.Functions[conditionDefinition.Rule.Id];
                var conditionDeclaration = new RuleDeclaration(IRule.Output.Self, $"{declaration.Name} condition: {elementName}", conditionDefinition);
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
