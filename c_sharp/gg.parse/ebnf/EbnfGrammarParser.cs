using gg.parse.rulefunctions;

namespace gg.parse.ebnf
{
    /// <summary>
    /// Generates a tokenizer (RuleTable<char>) based on an EBNF spec
    /// </summary>
    public class EbnfGrammarParser : RuleTable<int>
    {
        public EbnfTokenizer Tokenizer { get; init; }      

        public MatchOneOfFunction<int>        MatchLiteral { get; private set; }

        public MatchOneOfFunction<int>      MatchRuleDefinition { get; private set; }

        public MatchSingleData<int>        MatchAnyToken { get; private set; }

        public MatchSingleData<int>        MatchTransitiveSelector { get; private set; }

        public MatchSingleData<int>        MatchNoProductSelector { get; private set; }

        public MatchSingleData<int>        MatchRuleName { get; private set; }

        public MatchSingleData<int>        MatchIdentifier { get; private set; }

        public MatchFunctionSequence<int>  MatchSequence { get; private set; }

        public MatchFunctionSequence<int>  MatchOption { get; private set; }

        //public MatchFunctionSequence<int>  MatchCharacterSet { get; private set; }

        //public MatchFunctionSequence<int>  MatchCharacterRange { get; private set; }

        public MatchFunctionSequence<int>  MatchGroup { get; private set; }

        public MatchFunctionSequence<int>  MatchZeroOrMoreOperator { get; private set; }

        public MatchFunctionSequence<int>  MatchZeroOrOneOperator { get; private set; }

        public MatchFunctionSequence<int>  MatchOneOrMoreOperator { get; private set; }

        public MatchFunctionSequence<int>  MatchNotOperator { get; private set; }

        public MatchFunctionSequence<int>  MatchError { get; private set; }

        /// <summary>
        /// Create a grammar parser 
        /// </summary>
        /// <param name="tokenizer">Tokenizer used to turn the grammar-input text into tokens.</param>
        /// <param name="inputTokens">List of rules found in the token-input text which are used to identify tokens 
        /// in the grammar-input text</param>
        public EbnfGrammarParser(EbnfTokenizer tokenizer, RuleTable<char> inputTokens)
        {
            Tokenizer = tokenizer;

            // "abc" or 'abc'
            MatchLiteral = OneOf("Literal", AnnotationProduct.Annotation,
                    Token(TokenNames.SingleQuotedString),
                    Token(TokenNames.DoubleQuotedString)
            );

            // .
            MatchAnyToken = Token("AnyToken", AnnotationProduct.Annotation, TokenNames.AnyCharacter);

            // { "abcf" }
            /*MatchCharacterSet = Sequence("CharacterSet", AnnotationProduct.Annotation,
                    Token(TokenNames.ScopeStart),
                    MatchLiteral,
                    Token(TokenNames.ScopeEnd)
            );

            // { 'a' .. 'z' }
            MatchCharacterRange = Sequence("CharacterRange", AnnotationProduct.Annotation,
                    Token(TokenNames.ScopeStart),
                    MatchLiteral,
                    Token(TokenNames.Elipsis),
                    MatchLiteral,
                    Token(TokenNames.ScopeEnd)
            );*/

            MatchIdentifier = Token("Identifier", AnnotationProduct.Annotation, TokenNames.Identifier);
            
            // literal | set
            var ruleTerms = OneOf("#UnaryRuleTerms", AnnotationProduct.Transitive, 
                MatchLiteral, 
                MatchAnyToken, 
                //MatchCharacterSet, 
                //MatchCharacterRange, 
                MatchIdentifier
            );

            var nextSequenceElement = Sequence("#NextSequenceElement", AnnotationProduct.Transitive,
                    Token(TokenNames.CollectionSeparator),
                    ruleTerms);

            // a, b, c
            MatchSequence = Sequence("Sequence", AnnotationProduct.Annotation,
                    ruleTerms,
                    Token(TokenNames.CollectionSeparator),
                    ruleTerms,
                    ZeroOrMore("#SequenceRest", AnnotationProduct.Transitive, nextSequenceElement));

            var nextOptionElement = Sequence("#NextOptionElement", AnnotationProduct.Transitive,
                    Token(TokenNames.Option),
                    ruleTerms);

            // a | b | c
            MatchOption = Sequence("Option", AnnotationProduct.Annotation,
                    ruleTerms,
                    Token(TokenNames.Option),
                    ruleTerms,
                    ZeroOrMore("#OptionRest", AnnotationProduct.Transitive, nextOptionElement));

            var binaryRuleTerms = OneOf("#BinaryRuleTerms", AnnotationProduct.Transitive, MatchSequence, MatchOption);

            MatchRuleDefinition = OneOf("#RuleDefinition", AnnotationProduct.Transitive, 
                binaryRuleTerms, 
                ruleTerms);

            // ( a, b, c )
            MatchGroup = Sequence("#Group", AnnotationProduct.Transitive,
                Token(TokenNames.GroupStart),
                MatchRuleDefinition,
                Token(TokenNames.GroupEnd));

            // *(a | b | c)
            MatchZeroOrMoreOperator = Sequence("ZeroOrMore", AnnotationProduct.Annotation,
                Token(TokenNames.ZeroOrMoreOperator),
                ruleTerms);

            // ?(a | b | c)
            MatchZeroOrOneOperator = Sequence("ZeroOrOne", AnnotationProduct.Annotation,
                Token(TokenNames.ZeroOrOneOperator),
                ruleTerms);

            // +(a | b | c)
            MatchOneOrMoreOperator = Sequence("OneOrMore", AnnotationProduct.Annotation,
                Token(TokenNames.OneOrMoreOperator),
                ruleTerms);

            // !(a | b | c)
            MatchNotOperator = Sequence("Not", AnnotationProduct.Annotation,
                Token(TokenNames.NotOperator),
                ruleTerms);

            MatchError = Sequence("Error", AnnotationProduct.Annotation,
                    Token("ErrorKeyword", AnnotationProduct.Annotation, TokenNames.MarkError),
                    MatchLiteral,
                    MatchRuleDefinition
            );

            ruleTerms.RuleOptions = [.. ruleTerms.RuleOptions, MatchGroup, MatchZeroOrMoreOperator, MatchZeroOrOneOperator, MatchOneOrMoreOperator, MatchNotOperator, MatchError];

            MatchTransitiveSelector = Token("TransitiveSelector", AnnotationProduct.Annotation, TokenNames.TransitiveSelector);
            MatchNoProductSelector = Token("NoProductSelector", AnnotationProduct.Annotation, TokenNames.NoProductSelector);

            var ruleProduction = ZeroOrOne("#RuleProduction", AnnotationProduct.Transitive,
                OneOf("ProductionSelection", AnnotationProduct.Transitive,
                    MatchTransitiveSelector,
                    MatchNoProductSelector
                )
            );

            MatchRuleName = Token("RuleName", AnnotationProduct.Annotation, TokenNames.Identifier);
            
            var rule = Sequence("Rule", AnnotationProduct.Annotation,
                    ruleProduction,
                    MatchRuleName,
                    Token(TokenNames.Assignment),
                    MatchRuleDefinition,
                    Token(TokenNames.EndStatement));

            Root = ZeroOrMore("#Root", AnnotationProduct.Transitive, rule);        
            
            //foreach (var tokenFunctionName in inputTokens.FunctionNames)
            //{
            //    var tokenFunction = inputTokens.FindRule(tokenFunctionName);

            //    if (tokenFunction.Production == AnnotationProduct.Annotation)
            //    {
            //        RegisterRule(new MatchSingleData<int>($"input_token({tokenFunctionName}", tokenFunction.Id));
            //    }
            //}
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
            return Single($"{AnnotationProduct.None.GetPrefix()}Token({tokenName})", AnnotationProduct.None, rule.Id);
        }

        private MatchSingleData<int> Token(string ruleName, AnnotationProduct product, string tokenName)
        {
            var rule = Tokenizer.FindRule(tokenName);
            return Single(ruleName, product, rule.Id);
        }
    }
}

