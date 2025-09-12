using gg.parse.rulefunctions;
using gg.parse.rulefunctions.datafunctions;
using gg.parse.rulefunctions.rulefunctions;

using static gg.parse.rulefunctions.CommonRules;
using static System.Net.Mime.MediaTypeNames;

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

        public MarkError<int> ExpectedOperatorError { get; private set; }

        public MarkError<int> MissingRuleEndError { get; private set; }

        public Dictionary<string, LogRule<int>> MissingOperatorError { get; init; } = [];

        public Dictionary<string, LogRule<int>> MissingTermAfterOperatorInRemainderError { get; init; } = [];

        public Dictionary<string, LogRule<int>> MissingTermAfterOperatorError { get; init; } = [];

        public Dictionary<string, LogRule<int>> WrongOperatorTokenError { get; init; } = [];

        private MatchNotFunction<int> Eof { get; set; }

        private MatchAnyData<int> MatchAny { get; set; }

        private MatchSingleData<int> GroupStart { get; set; }
        
        private MatchSingleData<int> GroupEnd { get; set; }

        private MatchSingleData<int> RuleEnd { get; set; }



        public EbnfTokenParser()
            : this(new EbnfTokenizer())
        {
        }

        public EbnfTokenParser(EbnfTokenizer tokenizer)
        {
            Tokenizer = tokenizer;

            MatchTransitiveSelector = Token("TransitiveSelector", AnnotationProduct.Annotation, CommonTokenNames.TransitiveSelector);
            MatchNoProductSelector = Token("NoProductSelector", AnnotationProduct.Annotation, CommonTokenNames.NoProductSelector);

            RuleEnd = Token(CommonTokenNames.EndStatement);
            GroupStart = Token(CommonTokenNames.GroupStart);
            GroupEnd = Token(CommonTokenNames.GroupEnd);
            MatchAny = new MatchAnyData<int>("Any");
            Eof = new MatchNotFunction<int>("~EOF", MatchAny);

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
            // mainSequence contains both the match and error handling
            (var mainSequence, MatchSequence) = CreateBinaryOperator("Sequence", CommonTokenNames.CollectionSeparator, unaryAndDataTerms);

            // a | b | c
            // mainOption contains both the match and error handling
            (var mainOption, MatchOption) = CreateBinaryOperator("Option", CommonTokenNames.Option, unaryAndDataTerms);

            // a / b / c
            // mainEval contains both the match and error handling
            (var mainEval, MatchEval) = CreateBinaryOperator("Evaluation", CommonTokenNames.OptionWithPrecedence, unaryAndDataTerms);

            var ruleDefinition = this.OneOf(
                "#RuleDefinition", 
                AnnotationProduct.Transitive,
                // match this before unary terms
                this.OneOf("#BinaryRuleTerms", AnnotationProduct.Transitive, mainSequence, mainOption, mainEval), 
                unaryAndDataTerms
            );

            // ( a, b, c )
            MatchGroup = this.Sequence("#Group", AnnotationProduct.Transitive,
                GroupStart,
                ruleDefinition,
                GroupEnd);

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
                    this.OneOf(Eof, MatchAny),
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

            var ruleHeader = this.Sequence(
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
                    this.OneOf(Eof, RuleEnd),
                    maxSkip: 1
            );

            var ruleBodyOptions = this.OneOf(
                "#RuleDefinitionOptions",
                AnnotationProduct.Transitive,
                
                ruleDefinition,
                // xxx should mark this as a warning - empty rule
                this.TryMatch(RuleEnd),
                InvalidRuleDefinitionError
            );

            MissingRuleEndError = this.Error(
                "MissingEndRule",
                AnnotationProduct.Annotation,
                "Missing end of rule (;) at the given position.",
                this.OneOf(Eof, MatchAny),
                1
            );            

            var endStatementOptions = this.OneOf(
                "#EndStatementOptions",
                AnnotationProduct.Transitive,
                RuleEnd,
                MissingRuleEndError
            );

            MatchRule = this.Sequence(
                "Rule", 
                AnnotationProduct.Annotation,
                ruleHeader,
                Token(CommonTokenNames.Assignment),
                ruleBodyOptions,
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
            var functionIds = tokens.Select(t => t.RuleId).ToArray();
            return Root.Parse(functionIds, 0);
        }

        public (List<Annotation> tokens, List<Annotation> astNodes) Parse(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var tokenizerTokens = TokenizeText(text);
                
                if (tokenizerTokens.Count > 0)
                { 
                    return ParseGrammar(text, tokenizerTokens!);
                }
            }

            return ([], []);

        }

        private List<Annotation> TokenizeText(string text)
        {
            var tokenizationResult = Tokenizer.Tokenize(text);

            if (tokenizationResult.FoundMatch && tokenizationResult.Annotations != null)
            {
                var tokenizerTokens = tokenizationResult.Annotations;

                var unknownTokenId = Tokenizer.FindRule(CommonTokenNames.UnknownToken).Id;
                var errorPredicate = new Func<Annotation, bool>(a => a.RuleId == unknownTokenId);
                var tokenizerErrors = new List<Annotation>();

                foreach (var annotation in tokenizationResult.Annotations)
                {
                    annotation.Collect(errorPredicate, tokenizerErrors);
                }

                if (tokenizerErrors.Count > 0)
                {
                    throw new TokenizeException(
                        "input contains characters which could not be mapped to a token.",
                        tokenizerErrors
                    );
                }

                return tokenizerTokens!;
            }

            throw new TokenizeException("input contains no valid tokens.");
        }

        private (List<Annotation> tokens, List<Annotation> astNodes) ParseGrammar(string text, List<Annotation> tokens)
        {
            var astResult = Parse(tokens);

            if (astResult.FoundMatch)
            {
                var astNodes = astResult.Annotations;

                if (astNodes == null)
                {
                    throw new ParseException("input contains no valid grammar.");
                }

                var errorPredicate = new Func<Annotation, bool>(
                    a => FindRule(a.RuleId) is MarkError<int> or LogRule<int>
                );
                var grammarErrors = new List<Annotation>();

                foreach (var annotation in astNodes)
                {
                    annotation.Collect(errorPredicate, grammarErrors);
                }

                if (grammarErrors.Count > 0)
                {
                    throw new ParseException(
                            "input contains tokens which could not be mapped to grammar.",
                            grammarErrors,
                            text,
                            tokens
                    );
                }

                return (tokens, astResult.Annotations!);
            }
            else
            {
                return (tokens, []);
            }
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

        /// <summary>
        /// Create a rule to match a binary operator such as "a | b | c". Also takes in account
        /// the various errors which could occur such as missing operators (see the code for more details).
        /// </summary>
        /// <param name="name">Name of the returned matchOperationFunction</param>
        /// <param name="operatorTokenName">The name of the operator token (eg Option)</param>
        /// <param name="ruleTerms">A function which can match the rule terms </param>
        /// <returns>A tuple which contains the main function, ie the function which holds all
        /// success and error conditions and matchOperationFunction which holds the success condition</returns>
        private (MatchOneOfFunction<int> mainFunction, MatchFunctionSequence<int> matchOperationFunction) 
            CreateBinaryOperator(string name, string operatorTokenName, MatchOneOfFunction<int> ruleTerms)
        {
            // end_of_operator = (eof | ruleEnd | endGroup)

            // remainder       = *((operator, ruleTerms)            -- next element 
            //                  | (!end_of_operator, (
            //                      error_if ruleTerms                      "missing operator." 
            //                      | error_if (operator, !ruleTerms)       "missing rule terms after operator." 
            //                      | error_if (!operator, ., ruleTerms)    "expector operator, found another token." 
            //                    )

            // binary_operator = (ruleTerms, operator, ruleterms, remainder)
            //                  | error_if (ruleTerms, operator, !ruleterms), "missing ruleterms"

            var operatorToken = Token(operatorTokenName);

            var nextElement = this.Sequence(
                    $"#Next{name}Element", 
                    AnnotationProduct.Transitive,
                    Token(operatorTokenName),
                    ruleTerms);

            var notEndOfOperator = this.Not(this.OneOf(Eof, RuleEnd, GroupEnd));

            // user forgot an operator eg: a, b  c;
            var matchMissingOperatorError = this.MatchError(
                $"MissingOperatorError({operatorTokenName})",
                AnnotationProduct.Annotation,
                $"Expected an operator ({operatorTokenName}) but did not find any.",
                ruleTerms
            );

            MissingOperatorError[operatorTokenName] = matchMissingOperatorError;

            // user forgot an term after the operator eg: a, b, ;
            var matchMissingTermError = this.MatchError(
                $"MissingTermError({operatorTokenName})",
                AnnotationProduct.Annotation,
                $"Expected an rule term after operator ({operatorTokenName}) but did not find any.",
                this.Sequence(Token(operatorTokenName), this.Not(ruleTerms))
            );

            MissingTermAfterOperatorInRemainderError[operatorTokenName] = matchMissingTermError;

            // user used a different operator by mistake (?) eg: a, b | c, d or a wrong token altogether
            // eg a, b, . c
            var matchWrongOperatorError = this.MatchError(
                $"WrongOperatorError({operatorTokenName})",
                AnnotationProduct.Annotation,
                $"Expected an operator ({operatorTokenName}) but found something else.",
                this.Sequence(this.Not(operatorToken), MatchAny, ruleTerms)
            );

            WrongOperatorTokenError[operatorTokenName] = matchWrongOperatorError;

            var matchOperatorError = this.Sequence(
                $"#MatchOperatorErrors({operatorTokenName})",
                AnnotationProduct.Transitive,
                notEndOfOperator,
                this.OneOf(
                    $"#OneOfOperatorError({operatorTokenName})", 
                    AnnotationProduct.Transitive,
                    matchMissingTermError,
                    matchMissingOperatorError,
                    matchWrongOperatorError
                )
            );

            var remainder = this.OneOf(
                $"#OperatorRemainder({operatorTokenName})",
                AnnotationProduct.Transitive,
                nextElement,
                matchOperatorError
            );

            var matchOperatorSequence = this.Sequence(
                name,
                AnnotationProduct.Annotation,
                ruleTerms,
                operatorToken,
                ruleTerms,
                this.ZeroOrMore($"#{name}Remainder", AnnotationProduct.Transitive, remainder)
            );

            var matchOperatorSequenceError = this.MatchError(
                $"MatchOperatorSequenceError({operatorTokenName})",
                AnnotationProduct.Annotation,
                $"Expected a rule term after operator ({operatorTokenName}) but found something else.",
                this.Sequence(ruleTerms, operatorToken, this.Not(ruleTerms))
            );

            MissingTermAfterOperatorError[operatorTokenName] = matchOperatorSequenceError;

            return (
                this.OneOf(
                    $"#OneOfBinaryOperator(({operatorTokenName})", 
                    AnnotationProduct.Transitive,
                    matchOperatorSequence,
                    matchOperatorSequenceError
                ),
                matchOperatorSequence
            );
        }
    }
}
