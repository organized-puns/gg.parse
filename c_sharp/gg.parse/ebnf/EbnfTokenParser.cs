using gg.parse.rulefunctions;
using gg.parse.rulefunctions.datafunctions;
using gg.parse.rulefunctions.rulefunctions;

using static gg.parse.rulefunctions.CommonRules;

namespace gg.parse.ebnf
{
    /// <summary>
    /// Turns a list of tokens into an abstract syntax tree according to EBNF(like) grammar.
    /// </summary>
    public class EbnfTokenParser : RuleGraph<int>
    {
        public EbnfTokenizer               Tokenizer { get; init; }      

        public MatchOneOfFunction<int>     MatchLiteral { get; private set; }

        public MatchSingleData<int>        MatchAnyToken { get; private set; }

        public MatchSingleData<int>        MatchTransitiveSelector { get; private set; }

        public MatchSingleData<int>        MatchNoProductSelector { get; private set; }

        public MatchFunctionSequence<int>  MatchRule { get; private set; }

        public MatchSingleData<int>        MatchRuleName { get; private set; }

        public MatchSingleData<int>        MatchPrecedence { get; private set; }

        public MatchFunctionSequence<int>  MatchIdentifier { get; private set; }

        public MatchFunctionSequence<int>  MatchSequence { get; private set; }

        public MatchFunctionSequence<int>  MatchOption { get; private set; }

        public MatchFunctionSequence<int>  MatchEval { get; private set; }

        public MatchFunctionSequence<int>  MatchCharacterSet { get; private set; }

        public MatchFunctionSequence<int>  MatchCharacterRange { get; private set; }

        public MatchFunctionSequence<int>  MatchGroup { get; private set; }

        public MatchFunctionSequence<int>  MatchZeroOrMoreOperator { get; private set; }

        public MatchFunctionSequence<int>  MatchZeroOrOneOperator { get; private set; }

        public MatchFunctionSequence<int>  MatchOneOrMoreOperator { get; private set; }

        public MatchFunctionSequence<int>  MatchNotOperator { get; private set; }

        public MatchFunctionSequence<int>  TryMatchOperator { get; private set; }

        public MatchFunctionSequence<int>  MatchError { get; private set; }

        public MatchFunctionSequence<int> Include { get; private set; }

        public MatchFunctionSequence<int> MatchUnexpectedProductError { get; private set; }

        public MarkError<int> UnexpectedProductError { get; private set; }

        public MarkError<int> InvalidRuleDefinitionError { get; private set; }

        public MarkError<int> UnknownInputError { get; private set; }

        public MarkError<int> MissingRuleEndError { get; private set; }

        public EbnfTokenParser()
            : this(new EbnfTokenizer())
        {
        }

        public EbnfTokenParser(EbnfTokenizer tokenizer)
        {
            Tokenizer = tokenizer;

            MatchTransitiveSelector = Token("TransitiveSelector", AnnotationProduct.Annotation, CommonTokenNames.TransitiveSelector);
            MatchNoProductSelector = Token("NoProductSelector", AnnotationProduct.Annotation, CommonTokenNames.NoProductSelector);

            var endStatement = Token(CommonTokenNames.EndStatement);
            var any = new MatchAnyData<int>("Any");
            var eof = new MatchNotFunction<int>("~EOF", any);

            var ruleProduction = this.ZeroOrOne("#RuleProduction", AnnotationProduct.Transitive,
                this.OneOf("ProductionSelection", AnnotationProduct.Transitive,
                    MatchTransitiveSelector,
                    MatchNoProductSelector
                )
            );

            // "abc" or 'abc'
            MatchLiteral = this.OneOf("Literal", AnnotationProduct.Annotation,
                    Token(CommonTokenNames.SingleQuotedString),
                    Token(CommonTokenNames.DoubleQuotedString)
            );

            // .
            MatchAnyToken = Token("AnyToken", AnnotationProduct.Annotation, CommonTokenNames.AnyCharacter);

            // { "abcf" }
            MatchCharacterSet = this.Sequence("CharacterSet", AnnotationProduct.Annotation,
                    Token(CommonTokenNames.ScopeStart),
                    MatchLiteral,
                    Token(CommonTokenNames.ScopeEnd)
            );

            // { 'a' .. 'z' }
            MatchCharacterRange = this.Sequence("CharacterRange", AnnotationProduct.Annotation,
                    Token(CommonTokenNames.ScopeStart),
                    MatchLiteral,
                    Token(CommonTokenNames.Elipsis),
                    MatchLiteral,
                    Token(CommonTokenNames.ScopeEnd)
            );

            MatchIdentifier = this.Sequence(
                "Identifier", 
                AnnotationProduct.Annotation,
                ruleProduction,
                Token("IdentifierToken", AnnotationProduct.Annotation, CommonTokenNames.Identifier)
            );

            var matchDataRules = new RuleBase<int>[] {
                MatchLiteral,
                MatchAnyToken,
                MatchCharacterSet,
                MatchCharacterRange,
                MatchIdentifier
            };

            var unaryAndDataTerms = this.OneOf(
                "#DataMatchers", 
                AnnotationProduct.Transitive, 
                [.. matchDataRules]
            );

            // a, b, c
            MatchSequence = BinaryOperator("Sequence", CommonTokenNames.CollectionSeparator, unaryAndDataTerms);

            // a | b | c
            MatchOption = BinaryOperator("Option", CommonTokenNames.Option, unaryAndDataTerms);

            // a / b / c
            MatchEval = BinaryOperator("Evaluation", CommonTokenNames.OptionWithPrecedence, unaryAndDataTerms);

            var ruleDefinition = this.OneOf(
                "#RuleDefinition", 
                AnnotationProduct.Transitive,
                // match this before unary terms
                this.OneOf("#BinaryRuleTerms", AnnotationProduct.Transitive, MatchSequence, MatchOption, MatchEval), 
                unaryAndDataTerms
            );

            // ( a, b, c )
            MatchGroup = this.Sequence("#Group", AnnotationProduct.Transitive,
                Token(CommonTokenNames.GroupStart),
                ruleDefinition,
                Token(CommonTokenNames.GroupEnd));

            // *(a | b | c)
            MatchZeroOrMoreOperator = this.Sequence("ZeroOrMore", AnnotationProduct.Annotation,
                Token(CommonTokenNames.ZeroOrMoreOperator),
                unaryAndDataTerms);

            // ?(a | b | c)
            MatchZeroOrOneOperator = this.Sequence("ZeroOrOne", AnnotationProduct.Annotation,
                Token(CommonTokenNames.ZeroOrOneOperator),
                unaryAndDataTerms);

            // +(a | b | c)
            MatchOneOrMoreOperator = this.Sequence("OneOrMore", AnnotationProduct.Annotation,
                Token(CommonTokenNames.OneOrMoreOperator),
                unaryAndDataTerms);

            // !(a | b | c)
            MatchNotOperator = this.Sequence("Not", AnnotationProduct.Annotation,
                Token(CommonTokenNames.NotOperator),
                unaryAndDataTerms);

            // >(a | b | c) / try ( a | b | c)
            TryMatchOperator = this.Sequence("TryMatch", AnnotationProduct.Annotation,
                this.OneOf(Token(CommonTokenNames.TryMatchOperator), Token(CommonTokenNames.TryMatchOperatorShortHand)),
                unaryAndDataTerms);

            var unaryOperators = new RuleBase<int>[]
            {
                MatchZeroOrMoreOperator,
                MatchZeroOrOneOperator,
                MatchOneOrMoreOperator,
                MatchNotOperator,
                TryMatchOperator
            };

            // A stray production modifier found, production modifier can only appear in front of references
            // because they don't make any sense elsewhere (or at least I'm not aware of a valid use case).
            // Match ~ or # inside the rule, if found, raise an error and skip until the next token,
            // in script: (~|#), error "unexpected product modifier" .
            UnexpectedProductError = this.Error(
                    "UnexpectedProductionModifier",
                    AnnotationProduct.Annotation,
                    "Found an unexpected annotation production modifier. These can only appear in front of references to other rules or rule declarations.",
                    this.OneOf(eof, this.Any()),
                    maxSkip: 1
            );

            MatchUnexpectedProductError = this.Sequence(
                "UnexpectedProductErrorMatch",
                AnnotationProduct.Annotation,
                ruleProduction,
                this.OneOf(
                    "#UnexpectedProductErrorMatchTerm", 
                    AnnotationProduct.Transitive, 
                    [..matchDataRules, ..unaryOperators, MatchGroup]
                ), 
                UnexpectedProductError
            );

            MatchError = this.Sequence(
                "Error", 
                AnnotationProduct.Annotation,
                Token("ErrorKeyword", AnnotationProduct.Annotation, CommonTokenNames.MarkError),
                MatchLiteral,
                ruleDefinition
            );

            unaryAndDataTerms.RuleOptions = [
                ..unaryAndDataTerms.RuleOptions, 
                ..unaryOperators,
                MatchGroup, 
                MatchError,
                MatchUnexpectedProductError
            ];

            Include = this.Sequence(
                "Include", 
                AnnotationProduct.Annotation,
                Token(CommonTokenNames.Include),
                MatchLiteral,
                Token(CommonTokenNames.EndStatement)
            );

            MatchRuleName = Token("RuleName", AnnotationProduct.Annotation, CommonTokenNames.Identifier);
            MatchPrecedence = Token("RulePrecedence", AnnotationProduct.Annotation, CommonTokenNames.Integer);

            var ruleDeclaration = this.Sequence(
                "#RuleDeclaration",
                AnnotationProduct.Transitive,
                ruleProduction,
                MatchRuleName,
                this.ZeroOrOne("#RulePrecedence", AnnotationProduct.Transitive, MatchPrecedence)
            );

            InvalidRuleDefinitionError = this.Error(
                    "CannotParseRuleDefinition",
                    AnnotationProduct.Annotation,
                    "Unable to parse the rule definition, please check the definition for mistakes.",
                    this.OneOf(eof, endStatement),
                    maxSkip: 1
            );

            var ruleDefinitionOptions = this.OneOf(
                "#RuleDefinitionOptions",
                AnnotationProduct.Transitive,
                ruleDefinition,
                InvalidRuleDefinitionError
            );

            MissingRuleEndError = this.Error(
                "MissingEndRule",
                AnnotationProduct.Annotation,
                "Missing end of rule (;) at the given position.",
                this.OneOf(eof, this.Any()),
                1
            );            

            var endStatementOptions = this.OneOf(
                "#EndStatementOptions",
                AnnotationProduct.Transitive,
                endStatement,
                MissingRuleEndError
            );

            MatchRule = this.Sequence(
                "Rule", 
                AnnotationProduct.Annotation,
                ruleDeclaration,
                Token(CommonTokenNames.Assignment),
                ruleDefinitionOptions,
                endStatementOptions
            );

            var validStatement = this.OneOf("#ValidStatement", AnnotationProduct.Transitive, Include, MatchRule);

            // fallback in case nothing matches
            UnknownInputError = this.Error(
                "UnknownInput",
                AnnotationProduct.Annotation,
                "Can't match the token at the given position to a astNode.",
                validStatement,
                0
            );

            Root = this.ZeroOrMore("#Root", AnnotationProduct.Transitive,
                this.OneOf("#Statement", AnnotationProduct.Transitive, validStatement, UnknownInputError));           
        }

        public ParseResult Parse(List<Annotation> tokens)
        {
            var functionIds = tokens.Select(t => t.FunctionId).ToArray();
            return Root.Parse(functionIds, 0);
        }

        public (List<Annotation> tokens, List<Annotation> astNodes) Parse(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var tokenizationResult = Tokenizer.Tokenize(text);

                if (tokenizationResult.FoundMatch)
                {
                    var tokenizerTokens = tokenizationResult.Annotations;

                    if (tokenizerTokens == null)
                    {
                        throw new TokenizeException("input contains no valid tokens.");
                    }

                    if (tokenizationResult.Annotations != null)
                    {
                        var tokenizerErrors = CollectErrors(tokenizerTokens, Tokenizer.FindRule(CommonTokenNames.UnknownToken).Id);

                        if (tokenizerErrors.Count > 0)
                        {
                            throw new TokenizeException(
                                "input contains characters which could not be mapped to a token.", 
                                tokenizerErrors
                            );
                        }

                        var astResult = Parse(tokenizationResult.Annotations);

                        if (astResult.FoundMatch)
                        {
                            var astNodes = astResult.Annotations;

                            if (astNodes == null)
                            {
                                throw new ParseException("input contains no valid grammar.");
                            }

                            var grammarErrors = CollectErrors(astNodes, UnknownInputError.Id);

                            if (grammarErrors.Count > 0)
                            {
                                throw new ParseException(
                                        "input contains tokens which could not be mapped to grammar.", 
                                        grammarErrors,
                                        text,
                                        tokenizationResult.Annotations
                                );
                            }

                            return (tokenizationResult.Annotations, astResult.Annotations);
                        }   
                        else
                        {
                            return (tokenizationResult.Annotations, []);
                        }
                    }
                    else
                    {
                        return ([], []);
                    }
                }
            }
            else
            {
                return ([], []);
            }

            throw new ArgumentException("Invalid input");
        }

        private MatchSingleData<int> Token(string tokenName)
        {
            var rule = Tokenizer.FindRule(tokenName);
            return this.Single($"{AnnotationProduct.None.GetPrefix()}Token({tokenName})", AnnotationProduct.None, rule.Id);
        }

        private MatchSingleData<int> Token(string ruleName, AnnotationProduct product, string tokenName)
        {
            var rule = Tokenizer.FindRule(tokenName);
            return this.Single(ruleName, product, rule.Id);
        }

        private static List<Annotation> CollectErrors(List<Annotation> annotationList, int errorId, List<Annotation>? collectedErrors = null)
        {
            var result = collectedErrors ?? new List<Annotation>();

            foreach (var annotation in annotationList)
            {
                if (annotation.FunctionId == errorId)
                {
                    result.Add(annotation);
                }

                if (annotation.Children != null && annotation.Children.Count > 0)
                {
                    CollectErrors(annotation.Children, errorId, result);
                }
            }

            return result;
        }

        private MatchFunctionSequence<int> BinaryOperator(string name, string operatorTokenName, MatchOneOfFunction<int> ruleTerms)
        {
            var nextSequenceElement = this.Sequence($"#Next{name}Element", AnnotationProduct.Transitive,
                    Token(operatorTokenName),
                    ruleTerms);

            return this.Sequence(name, AnnotationProduct.Annotation,
                    ruleTerms,
                    Token(operatorTokenName),
                    ruleTerms,
                    this.ZeroOrMore($"#{name}Remainder", AnnotationProduct.Transitive, nextSequenceElement));
        }

    }
}

