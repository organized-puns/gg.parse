using gg.parse.rulefunctions;
using System.Data;

namespace gg.parse.examples
{
  
    /// <summary>
    /// Generates a RuleTable<T> based on an EBNF spec
    /// </summary>
    public abstract class EbnfParserBase : RuleTable<int>
    {  
        public EbnfTokenizer Tokenizer { get; init; }

        public MatchOneOfFunction<int>? LiteralRule { get; private set; }
            
        public MatchSingleData<int>? AnyCharacterRule { get; private set; }
            
        public MatchSingleData<int>? TransitiveSelectorRule { get; private set; }
            
        public MatchSingleData<int>? NoProductSelectorRule { get; private set; }
            
        public MatchSingleData<int>? RuleNameRule { get; private set; }
            
        public MatchSingleData<int>? Identifier { get; private set; }
            
        public MatchFunctionSequence<int>? SequenceRule { get; private set; }
            
        public MatchFunctionSequence<int>? OptionRule { get; private set; }

        public MatchFunctionSequence<int>? CharacterSetRule { get; private set; }

        public MatchFunctionSequence<int>? CharacterRangeRule { get; private set; }
            
        public MatchFunctionSequence<int>? GroupRule { get; private set; }
            
        public MatchFunctionSequence<int>? ZeroOrMoreOperatorRule { get; private set; }
        
        public MatchFunctionSequence<int>? ZeroOrOneOperatorRule { get; private set; }
            
        public MatchFunctionSequence<int>? OneOrMoreOperatorRule { get; private set; }
        
        public MatchFunctionSequence<int>? NotOperatorRule { get; private set; }

        public MatchFunctionSequence<int>? ErrorRule { get; private set; }

        public MatchOneOfFunction<int> UnaryRuleTerms { get; private set; }

        public MatchOneOfFunction<int> CompositeRuleTerms { get; private set; }

        public MatchOneOfFunction<int> RuleDefinition { get; private set; }

        public EbnfParserBase() : this(new EbnfTokenizer())
        {
        }
           
        public EbnfParserBase(EbnfTokenizer tokenizer)
        {
            Tokenizer = tokenizer;

            UnaryRuleTerms = OneOf("#UnaryRuleTerms", AnnotationProduct.Transitive, []);
            CompositeRuleTerms = OneOf("#CompositeRuleTerms", AnnotationProduct.Transitive, []);
            RuleDefinition = OneOf("#RuleDefinition", AnnotationProduct.Transitive,
                CompositeRuleTerms,
                UnaryRuleTerms);

            TransitiveSelectorRule = Token("TransitiveSelector", AnnotationProduct.Annotation, TokenNames.TransitiveSelector);
            NoProductSelectorRule = Token("NoProductSelector", AnnotationProduct.Annotation, TokenNames.NoProductSelector);

            var ruleProduction = ZeroOrOne("#RuleProduction", AnnotationProduct.Transitive,
                OneOf("ProductionSelection", AnnotationProduct.Transitive,
                    TransitiveSelectorRule,
                    NoProductSelectorRule
                )
            );

            RuleNameRule = Token("RuleName", AnnotationProduct.Annotation, TokenNames.Identifier);

            var rule = Sequence("Rule", AnnotationProduct.Annotation,
                    ruleProduction,
                    RuleNameRule,
                    Token(TokenNames.Assignment),
                    RuleDefinition,
                    Token(TokenNames.EndStatement));

            Root = ZeroOrMore("#Root", AnnotationProduct.Transitive, rule);
        }

        public void RegisterError()
        {
            if (LiteralRule == null)
            {
                RegisterLiteral();
            }

            ErrorRule = Sequence("Error", AnnotationProduct.Annotation,
                    Token("ErrorKeyword", AnnotationProduct.Annotation, TokenNames.MarkError),
                    LiteralRule,
                    RuleDefinition
            );

            AddUnaryTerm(ErrorRule);
        }

        private void AddUnaryTerm(RuleBase<int> rule)
        {
            if (Array.IndexOf(UnaryRuleTerms.Options, rule) == -1)
            {
                UnaryRuleTerms.Options = [.. UnaryRuleTerms.Options, rule];
            }
        }

        private void AddCompositeTerm(RuleBase<int> rule)
        {
            if (Array.IndexOf(CompositeRuleTerms.Options, rule) == -1)
            {
                CompositeRuleTerms.Options = [.. CompositeRuleTerms.Options, rule];
            }
        }

        public void RegisterOperators()
        {
            // *(a | b | c)
            ZeroOrMoreOperatorRule = Sequence("ZeroOrMore", AnnotationProduct.Annotation,
                Token(TokenNames.ZeroOrMoreOperator),
                UnaryRuleTerms);

            AddUnaryTerm(ZeroOrMoreOperatorRule);


            // ?(a | b | c)
            ZeroOrOneOperatorRule = Sequence("ZeroOrOne", AnnotationProduct.Annotation,
                Token(TokenNames.ZeroOrOneOperator),
                UnaryRuleTerms);

            AddUnaryTerm(ZeroOrOneOperatorRule);

            // +(a | b | c)
            OneOrMoreOperatorRule = Sequence("OneOrMore", AnnotationProduct.Annotation,
                Token(TokenNames.OneOrMoreOperator),
                UnaryRuleTerms);

            AddUnaryTerm(OneOrMoreOperatorRule);

            // !(a | b | c)
            NotOperatorRule = Sequence("Not", AnnotationProduct.Annotation,
                Token(TokenNames.NotOperator),
                UnaryRuleTerms);

            AddUnaryTerm(NotOperatorRule);
        }

        public void RegisterGroupRule(string? name = null)
        {
            // ( a, b, c )
            GroupRule = Sequence("#Group", AnnotationProduct.Transitive,
                Token(TokenNames.GroupStart),
                RuleDefinition,
                Token(TokenNames.GroupEnd));

            AddUnaryTerm(GroupRule);
        }


        public void RegisterSequence(string? name = null)
        {
            var nextSequenceElement = Sequence("#NextSequenceElement", AnnotationProduct.Transitive,
                        Token(TokenNames.CollectionSeparator),
                        UnaryRuleTerms);

            // a, b, c
            SequenceRule = Sequence(name ?? "Sequence", AnnotationProduct.Annotation,
                    UnaryRuleTerms,
                    Token(TokenNames.CollectionSeparator),
                    UnaryRuleTerms,
                    ZeroOrMore("#SequenceRest", AnnotationProduct.Transitive, nextSequenceElement));

            AddCompositeTerm(SequenceRule);
        }

        public void RegisterOption(string? name = null)
        {
            var nextOptionElement = Sequence("#NextOptionElement", AnnotationProduct.Transitive,
                        Token(TokenNames.Option),
                        UnaryRuleTerms);

            // a | b | c
            OptionRule = Sequence(name ?? "Option", AnnotationProduct.Annotation,
                    UnaryRuleTerms,
                    Token(TokenNames.Option),
                    UnaryRuleTerms,
                    ZeroOrMore("#OptionRest", AnnotationProduct.Transitive, nextOptionElement));

            AddCompositeTerm(OptionRule);
        }

        public void RegisterIdentifier(string? name = null)
        {
            Identifier = Token(name ?? "Identifier", AnnotationProduct.Annotation, TokenNames.Identifier);
            AddUnaryTerm(Identifier);
        }
    

        public void RegisterLiteral()
        {
            // "abc" or 'abc'
            LiteralRule = OneOf("Literal", AnnotationProduct.Annotation,
                    Token(TokenNames.SingleQuotedString),
                    Token(TokenNames.DoubleQuotedString)
            );
            
            AddUnaryTerm(LiteralRule);
        }

        public void RegisterCharacterRange(string? name = null)
        {
            if (LiteralRule == null)
            {
                RegisterLiteral();
            }

            // { 'a' .. 'z' }
            CharacterRangeRule = Sequence(name ?? "CharacterRange", AnnotationProduct.Annotation,
                Token(TokenNames.ScopeStart),
                LiteralRule!,
                Token(TokenNames.Elipsis),
                LiteralRule!,
                Token(TokenNames.ScopeEnd)
            );

            AddUnaryTerm(CharacterRangeRule);
        }

        public void RegisterAnyCharacter(string? name = null)
        {
            // .
            AnyCharacterRule = Token(name ?? "AnyCharacter", AnnotationProduct.Annotation, TokenNames.AnyCharacter);

            AddUnaryTerm(AnyCharacterRule);
        }

        public void RegisterCharacterSetRule(string? name = null)
        {
            if (LiteralRule == null)
            {
                RegisterLiteral();
            }

            // { "abcf" }
            CharacterSetRule = Sequence("CharacterSet", AnnotationProduct.Annotation,
                    Token(TokenNames.ScopeStart),
                    LiteralRule!,
                    Token(TokenNames.ScopeEnd)
            );

            AddUnaryTerm(CharacterSetRule);
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

