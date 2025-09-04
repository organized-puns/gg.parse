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
        public EbnfTokenizer Tokenizer { get; init; }      

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

        public MarkError<int> UnknownInputError { get; private set; }

        public EbnfTokenParser()
            : this(new EbnfTokenizer())
        {
        }

        public EbnfTokenParser(EbnfTokenizer tokenizer)
        {
            Tokenizer = tokenizer;

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

            MatchTransitiveSelector = Token("TransitiveSelector", AnnotationProduct.Annotation, CommonTokenNames.TransitiveSelector);
            MatchNoProductSelector = Token("NoProductSelector", AnnotationProduct.Annotation, CommonTokenNames.NoProductSelector);

            var ruleProduction = this.ZeroOrOne("#RuleProduction", AnnotationProduct.Transitive,
                this.OneOf("ProductionSelection", AnnotationProduct.Transitive,
                    MatchTransitiveSelector,
                    MatchNoProductSelector
                )
            );

            MatchIdentifier = this.Sequence("Identifier", AnnotationProduct.Annotation,
                                ruleProduction,
                                Token("IdentifierToken", AnnotationProduct.Annotation, CommonTokenNames.Identifier));

            // literal | set
            var ruleTerms = this.OneOf("#UnaryRuleTerms", AnnotationProduct.Transitive, 
                MatchLiteral, 
                MatchAnyToken, 
                MatchCharacterSet, 
                MatchCharacterRange, 
                MatchIdentifier
            );

            var nextSequenceElement = this.Sequence("#NextSequenceElement", AnnotationProduct.Transitive,
                    Token(CommonTokenNames.CollectionSeparator),
                    ruleTerms);

            // a, b, c
            MatchSequence = this.Sequence("Sequence", AnnotationProduct.Annotation,
                    ruleTerms,
                    Token(CommonTokenNames.CollectionSeparator),
                    ruleTerms,
                    this.ZeroOrMore("#SequenceRest", AnnotationProduct.Transitive, nextSequenceElement));

            var nextOptionElement = this.Sequence("#NextOptionElement", AnnotationProduct.Transitive,
                    Token(CommonTokenNames.Option),
                    ruleTerms);

            // a | b | c
            MatchOption = this.Sequence("Option", AnnotationProduct.Annotation,
                    ruleTerms,
                    Token(CommonTokenNames.Option),
                    ruleTerms,
                    this.ZeroOrMore("#OptionRest", AnnotationProduct.Transitive, nextOptionElement));

            // xxx this is the same pattern as sequence and option, create a function for it
            var nextEvalElement = this.Sequence("#NextEvalElement", AnnotationProduct.Transitive,
                    Token(CommonTokenNames.OptionWithPrecedence),
                    ruleTerms);


            // a / b / c
            MatchEval = this.Sequence("Evaluation", AnnotationProduct.Annotation,
                    ruleTerms,
                    Token(CommonTokenNames.OptionWithPrecedence),
                    ruleTerms,
                    this.ZeroOrMore("#EvaluationRest", AnnotationProduct.Transitive, nextEvalElement));

            var binaryRuleTerms = this.OneOf("#BinaryRuleTerms", AnnotationProduct.Transitive, MatchSequence, MatchOption, MatchEval);

            var ruleDefinition = this.OneOf("#RuleDefinition", AnnotationProduct.Transitive, 
                binaryRuleTerms, 
                ruleTerms);

            // ( a, b, c )
            MatchGroup = this.Sequence("#Group", AnnotationProduct.Transitive,
                Token(CommonTokenNames.GroupStart),
                ruleDefinition,
                Token(CommonTokenNames.GroupEnd));

            // *(a | b | c)
            MatchZeroOrMoreOperator = this.Sequence("ZeroOrMore", AnnotationProduct.Annotation,
                Token(CommonTokenNames.ZeroOrMoreOperator),
                ruleTerms);

            // ?(a | b | c)
            MatchZeroOrOneOperator = this.Sequence("ZeroOrOne", AnnotationProduct.Annotation,
                Token(CommonTokenNames.ZeroOrOneOperator),
                ruleTerms);

            // +(a | b | c)
            MatchOneOrMoreOperator = this.Sequence("OneOrMore", AnnotationProduct.Annotation,
                Token(CommonTokenNames.OneOrMoreOperator),
                ruleTerms);

            // !(a | b | c)
            MatchNotOperator = this.Sequence("Not", AnnotationProduct.Annotation,
                Token(CommonTokenNames.NotOperator),
                ruleTerms);

            // >(a | b | c) / try ( a | b | c)
            TryMatchOperator = this.Sequence("TryMatch", AnnotationProduct.Annotation,
                this.OneOf(Token(CommonTokenNames.TryMatchOperator), Token(CommonTokenNames.TryMatchOperatorShortHand)),
                ruleTerms);

            MatchError = this.Sequence("Error", AnnotationProduct.Annotation,
                    Token("ErrorKeyword", AnnotationProduct.Annotation, CommonTokenNames.MarkError),
                    MatchLiteral,
                    ruleDefinition
            );

            Include = this.Sequence("Include", AnnotationProduct.Annotation,
                Token(CommonTokenNames.Include), 
                MatchLiteral,
                Token(CommonTokenNames.EndStatement));

            ruleTerms.RuleOptions = [.. ruleTerms.RuleOptions, MatchGroup, MatchZeroOrMoreOperator, 
                                    MatchZeroOrOneOperator, MatchOneOrMoreOperator, MatchNotOperator, TryMatchOperator, MatchError];

            MatchRuleName = Token("RuleName", AnnotationProduct.Annotation, CommonTokenNames.Identifier);

            MatchPrecedence = Token("RulePrecedence", AnnotationProduct.Annotation, CommonTokenNames.Integer);

            MatchRule = this.Sequence("Rule", AnnotationProduct.Annotation,
                    ruleProduction,
                    MatchRuleName,
                    this.ZeroOrOne("#RulePrecedence",AnnotationProduct.Transitive, MatchPrecedence),
                    Token(CommonTokenNames.Assignment),
                    ruleDefinition,
                    Token(CommonTokenNames.EndStatement));

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
    }
}

