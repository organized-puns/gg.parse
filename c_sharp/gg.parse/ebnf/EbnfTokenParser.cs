using gg.parse.rulefunctions;

using static gg.parse.rulefunctions.CommonRuleTableRules;

namespace gg.parse.ebnf
{
    /// <summary>
    /// Parses
    /// </summary>
    public class EbnfTokenParser : RuleGraph<int>
    {
        public EbnfTokenizer Tokenizer { get; init; }      

        public MatchOneOfFunction<int>     MatchLiteral { get; private set; }

        public MatchSingleData<int>        MatchAnyToken { get; private set; }

        public MatchSingleData<int>        MatchTransitiveSelector { get; private set; }

        public MatchSingleData<int>        MatchNoProductSelector { get; private set; }

        public MatchSingleData<int>        MatchRuleName { get; private set; }

        public MatchFunctionSequence<int>  MatchIdentifier { get; private set; }

        public MatchFunctionSequence<int>  MatchSequence { get; private set; }

        public MatchFunctionSequence<int>  MatchOption { get; private set; }

        public MatchFunctionSequence<int>  MatchCharacterSet { get; private set; }

        public MatchFunctionSequence<int>  MatchCharacterRange { get; private set; }

        public MatchFunctionSequence<int>  MatchGroup { get; private set; }

        public MatchFunctionSequence<int>  MatchZeroOrMoreOperator { get; private set; }

        public MatchFunctionSequence<int>  MatchZeroOrOneOperator { get; private set; }

        public MatchFunctionSequence<int>  MatchOneOrMoreOperator { get; private set; }

        public MatchFunctionSequence<int>  MatchNotOperator { get; private set; }

        public MatchFunctionSequence<int>  MatchError { get; private set; }

        public EbnfTokenParser()
            : this(new EbnfTokenizer())
        {
        }

        public EbnfTokenParser(EbnfTokenizer tokenizer)
        {
            Tokenizer = tokenizer;

            // "abc" or 'abc'
            MatchLiteral = this.OneOf("Literal", AnnotationProduct.Annotation,
                    Token(TokenNames.SingleQuotedString),
                    Token(TokenNames.DoubleQuotedString)
            );

            // .
            MatchAnyToken = Token("AnyToken", AnnotationProduct.Annotation, TokenNames.AnyCharacter);

            // { "abcf" }
            MatchCharacterSet = this.Sequence("CharacterSet", AnnotationProduct.Annotation,
                    Token(TokenNames.ScopeStart),
                    MatchLiteral,
                    Token(TokenNames.ScopeEnd)
            );

            // { 'a' .. 'z' }
            MatchCharacterRange = this.Sequence("CharacterRange", AnnotationProduct.Annotation,
                    Token(TokenNames.ScopeStart),
                    MatchLiteral,
                    Token(TokenNames.Elipsis),
                    MatchLiteral,
                    Token(TokenNames.ScopeEnd)
            );

            MatchTransitiveSelector = Token("TransitiveSelector", AnnotationProduct.Annotation, TokenNames.TransitiveSelector);
            MatchNoProductSelector = Token("NoProductSelector", AnnotationProduct.Annotation, TokenNames.NoProductSelector);


            var ruleProduction = this.ZeroOrOne("#RuleProduction", AnnotationProduct.Transitive,
                this.OneOf("ProductionSelection", AnnotationProduct.Transitive,
                    MatchTransitiveSelector,
                    MatchNoProductSelector
                )
            );

            MatchIdentifier = this.Sequence("Identifier", AnnotationProduct.Annotation,
                                ruleProduction,
                                Token("IdentifierToken", AnnotationProduct.Annotation, TokenNames.Identifier));

            // literal | set
            var ruleTerms = this.OneOf("#UnaryRuleTerms", AnnotationProduct.Transitive, 
                MatchLiteral, 
                MatchAnyToken, 
                MatchCharacterSet, 
                MatchCharacterRange, 
                MatchIdentifier
            );

            var nextSequenceElement = this.Sequence("#NextSequenceElement", AnnotationProduct.Transitive,
                    Token(TokenNames.CollectionSeparator),
                    ruleTerms);

            // a, b, c
            MatchSequence = this.Sequence("Sequence", AnnotationProduct.Annotation,
                    ruleTerms,
                    Token(TokenNames.CollectionSeparator),
                    ruleTerms,
                    this.ZeroOrMore("#SequenceRest", AnnotationProduct.Transitive, nextSequenceElement));

            var nextOptionElement = this.Sequence("#NextOptionElement", AnnotationProduct.Transitive,
                    Token(TokenNames.Option),
                    ruleTerms);

            // a | b | c
            MatchOption = this.Sequence("Option", AnnotationProduct.Annotation,
                    ruleTerms,
                    Token(TokenNames.Option),
                    ruleTerms,
                    this.ZeroOrMore("#OptionRest", AnnotationProduct.Transitive, nextOptionElement));

            var binaryRuleTerms = this.OneOf("#BinaryRuleTerms", AnnotationProduct.Transitive, MatchSequence, MatchOption);

            var ruleDefinition = this.OneOf("#RuleDefinition", AnnotationProduct.Transitive, 
                binaryRuleTerms, 
                ruleTerms);

            // ( a, b, c )
            MatchGroup = this.Sequence("#Group", AnnotationProduct.Transitive,
                Token(TokenNames.GroupStart),
                ruleDefinition,
                Token(TokenNames.GroupEnd));

            // *(a | b | c)
            MatchZeroOrMoreOperator = this.Sequence("ZeroOrMore", AnnotationProduct.Annotation,
                Token(TokenNames.ZeroOrMoreOperator),
                ruleTerms);

            // ?(a | b | c)
            MatchZeroOrOneOperator = this.Sequence("ZeroOrOne", AnnotationProduct.Annotation,
                Token(TokenNames.ZeroOrOneOperator),
                ruleTerms);

            // +(a | b | c)
            MatchOneOrMoreOperator = this.Sequence("OneOrMore", AnnotationProduct.Annotation,
                Token(TokenNames.OneOrMoreOperator),
                ruleTerms);

            // !(a | b | c)
            MatchNotOperator = this.Sequence("Not", AnnotationProduct.Annotation,
                Token(TokenNames.NotOperator),
                ruleTerms);

            MatchError = this.Sequence("Error", AnnotationProduct.Annotation,
                    Token("ErrorKeyword", AnnotationProduct.Annotation, TokenNames.MarkError),
                    MatchLiteral,
                    ruleDefinition
            );

            ruleTerms.RuleOptions = [.. ruleTerms.RuleOptions, MatchGroup, MatchZeroOrMoreOperator, MatchZeroOrOneOperator, MatchOneOrMoreOperator, MatchNotOperator, MatchError];

            MatchTransitiveSelector = Token("TransitiveSelector", AnnotationProduct.Annotation, TokenNames.TransitiveSelector);
            MatchNoProductSelector = Token("NoProductSelector", AnnotationProduct.Annotation, TokenNames.NoProductSelector);

            MatchRuleName = Token("RuleName", AnnotationProduct.Annotation, TokenNames.Identifier);
            
            var rule = this.Sequence("Rule", AnnotationProduct.Annotation,
                    ruleProduction,
                    MatchRuleName,
                    Token(TokenNames.Assignment),
                    ruleDefinition,
                    Token(TokenNames.EndStatement));

            Root = this.ZeroOrMore("#Root", AnnotationProduct.Transitive, rule);           
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
                var tokenResults = Tokenizer.Tokenize(text);

                if (tokenResults.FoundMatch)
                {
                    if (tokenResults.Annotations != null)
                    {
                        var astResults = Parse(tokenResults.Annotations);

                        if (astResults.FoundMatch)
                        {
                            return (tokenResults.Annotations, astResults.Annotations);
                        }
                        else
                        {
                            return (tokenResults.Annotations, []);
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
    }
}

