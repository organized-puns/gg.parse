using gg.parse.rulefunctions;
using gg.parse.rulefunctions.datafunctions;
using gg.parse.rulefunctions.rulefunctions;
using gg.parse.rules;
using System.ComponentModel.DataAnnotations;
using static gg.parse.rulefunctions.CommonRules;

namespace gg.parse.ebnf
{
    /// <summary>
    /// Turns a list of tokens into an abstract syntax tree according to EBNF(like) grammar.
    /// </summary>
    public class EbnfTokenParser : CommonGraphWrapper<int>
    {
        public EbnfTokenizer Tokenizer { get; init; }

        /// <summary>
        /// If set to true an exception will be thrown when a warning is encountered
        /// </summary>
        public bool FailOnWarning { get; set; } = false;

        public MatchOneOfFunction<int> MatchLiteral { get; private set; }

        public MatchSingleData<int> MatchAnyToken { get; private set; }

        public MatchSingleData<int> MatchTransitiveSelector { get; private set; }

        public MatchSingleData<int> MatchNoProductSelector { get; private set; }

        public MatchFunctionSequence<int> MatchRule { get; private set; }

        public MatchSingleData<int> MatchRuleName { get; private set; }

        public MatchSingleData<int> MatchPrecedence { get; private set; }

        public MatchFunctionSequence<int> MatchIdentifier { get; private set; }

        public MatchFunctionSequence<int> MatchSequence { get; private set; }

        public MatchFunctionSequence<int> MatchOption { get; private set; }

        public MatchFunctionSequence<int> MatchEval { get; private set; }

        public MatchFunctionSequence<int> MatchCharacterSet { get; private set; }

        public MatchFunctionSequence<int> MatchCharacterRange { get; private set; }

        public MatchFunctionSequence<int> MatchGroup { get; private set; }

        public MatchFunctionSequence<int> MatchZeroOrMoreOperator { get; private set; }

        public MatchFunctionSequence<int> MatchZeroOrOneOperator { get; private set; }

        public MatchFunctionSequence<int> MatchOneOrMoreOperator { get; private set; }

        public MatchFunctionSequence<int> MatchNotOperator { get; private set; }

        public MatchFunctionSequence<int> TryMatchOperator { get; private set; }

        public MatchFunctionSequence<int> MatchLog { get; private set; }

        public MatchFunctionSequence<int> Include { get; private set; }

        public MatchFunctionSequence<int> MatchUnexpectedProductError { get; private set; }

        public LogRule<int> UnexpectedProductError { get; private set; }

        public LogRule<int> RuleBodyError { get; private set; }

        public LogRule<int> UnknownInputError { get; private set; }

        public LogRule<int> ExpectedOperatorError { get; private set; }

        public LogRule<int> MissingRuleEndError { get; private set; }

        public Dictionary<string, LogRule<int>> MissingOperatorError { get; init; } = [];

        public Dictionary<string, LogRule<int>> MissingTermAfterOperatorInRemainderError { get; init; } = [];

        public Dictionary<string, LogRule<int>> MissingTermAfterOperatorError { get; init; } = [];

        public Dictionary<string, LogRule<int>> WrongOperatorTokenError { get; init; } = [];

        private MatchNotFunction<int> Eof { get; set; }

        private MatchAnyData<int> MatchAny { get; set; }

        private MatchSingleData<int> GroupStartToken { get; set; }

        private MatchSingleData<int> GroupEndToken { get; set; }

        private MatchSingleData<int> RuleEndToken { get; set; }



        public EbnfTokenParser()
            : this(new EbnfTokenizer())
        {
        }

        public EbnfTokenParser(EbnfTokenizer tokenizer)
        {
            Tokenizer = tokenizer;

            MatchTransitiveSelector = Token("TransitiveSelector", AnnotationProduct.Annotation, CommonTokenNames.TransitiveSelector);
            MatchNoProductSelector = Token("NoProductSelector", AnnotationProduct.Annotation, CommonTokenNames.NoProductSelector);

            RuleEndToken = Token(CommonTokenNames.EndStatement);
            GroupStartToken = Token(CommonTokenNames.GroupStart);
            GroupEndToken = Token(CommonTokenNames.GroupEnd);
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

            var ruleBody = this.OneOf(
                "#RuleBody",
                AnnotationProduct.Transitive,
                // match this before unary terms
                this.OneOf("#BinaryRuleTerms", AnnotationProduct.Transitive, mainSequence, mainOption, mainEval),
                unaryAndDataTerms
            );

            // ( a, b, c )
            MatchGroup = this.Sequence("#Group", AnnotationProduct.Transitive,
                GroupStartToken,
                ruleBody,
                GroupEndToken);

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
            UnexpectedProductError = this.LogError(
                    "UnexpectedProductionModifier",
                    AnnotationProduct.Annotation,
                    "Found an unexpected annotation production modifier. These can only appear in front of references to other rules or rule declarations."
            );

            MatchUnexpectedProductError = this.Sequence(
                "UnexpectedProductErrorMatch",
                AnnotationProduct.Annotation,
                ruleProduction,
                this.OneOf(
                    "#UnexpectedProductErrorMatchTerm",
                    AnnotationProduct.Transitive,
                    [.. matchDataRules, .. unaryOperators, MatchGroup]
                ),
                UnexpectedProductError
            );

            MatchLog = CreateMatchLog(ruleBody);

            unaryAndDataTerms.RuleOptions = [
                ..unaryAndDataTerms.RuleOptions,
                ..unaryOperators,
                MatchGroup,
                MatchLog,
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

            RuleBodyError = error(
                "RuleBodyError",
                "Unable to parse the rule body, please check the definition for mistakes.",
                this.Skip(stopCondition: RuleEndToken, failOnEoF: false)
            );

            var emptyBodyWarning = warning("NoRuleBodyWarning", "Rule has no body.", ifMatches(RuleEndToken));

            var ruleBodyOptions = oneOf("#RuleBodyOptions", ruleBody, emptyBodyWarning, RuleBodyError);

            MissingRuleEndError = this.LogError(
                "MissingEndRule",
                AnnotationProduct.Annotation,
                "Missing end of rule (;) at the given position.",
                // skip until the start of the next rule, if any
                skip("~skipUntilNextHeaderOrEof", ruleHeader, failOnEoF: false)
            );

            var endStatementOptions = this.OneOf(
                "#EndStatementOptions",
                AnnotationProduct.Transitive,
                RuleEndToken,
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
            UnknownInputError = this.LogError(
                "UnknownInput",
                AnnotationProduct.Annotation,
                "Can't match the token at the given position to a astNode.",
                skip("~skipUntilNextValidStatement", stopCondition: validStatement, failOnEoF: false)
            );            

            Root = this.ZeroOrMore("#Root", AnnotationProduct.Transitive,
                this.OneOf("#Statement", AnnotationProduct.Transitive, validStatement, UnknownInputError));
        }

        /// <summary>
        /// Parse and validate the results of the various steps involved.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <exception cref="TokenizeException">Thrown when the tokenization step results in errors.</exception>
        /// <exception cref="ParseException">Thrown when parsing reports error.</exception>
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
                        tokenizerErrors,
                        text
                    );
                }

                return tokenizerTokens!;
            }

            throw new TokenizeException("input contains no valid tokens.");
        }

        private (List<Annotation> tokens, List<Annotation> astNodes) ParseGrammar(string text, List<Annotation> tokens)
        {
            var astResult = Root!.Parse(tokens);

            if (astResult.FoundMatch)
            {
                var astNodes = astResult.Annotations;

                if (astNodes == null)
                {
                    throw new ParseException("input contains no valid grammar.");
                }

                var errorLevel = FailOnWarning 
                    ? LogLevel.Warning | LogLevel.Error 
                    : LogLevel.Error;

                var errorPredicate = new Func<Annotation, bool>(
                    a => {
                        var rule = FindRule(a.RuleId);

                        // only need to capture errors, fatals will throw an exception
                        // so no need to capture them
                        return (rule is LogRule<int> logRule && (logRule.Level & errorLevel) == errorLevel);
                    }
                );
                var grammarErrors = new List<Annotation>();

                foreach (var annotation in astNodes)
                {
                    annotation.Collect(errorPredicate, grammarErrors);
                }

                if (grammarErrors.Count > 0)
                {
                    throw new ParseException(
                            "Parsing encountered some errors (or warnings which are treated as errors).",
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

            var notEndOfOperator = this.Not(this.OneOf(Eof, RuleEndToken, GroupEndToken));

            // user forgot an operator eg: a, b  c;
            var matchMissingOperatorError = this.LogError(
                $"MissingOperatorError({operatorTokenName})",
                AnnotationProduct.Annotation,
                $"Expected an operator ({operatorTokenName}) but did not find any.",
                ruleTerms
            );

            MissingOperatorError[operatorTokenName] = matchMissingOperatorError;

            // user forgot an term after the operator eg: a, b, ;
            var matchMissingTermError = this.LogError(
                $"MissingTermError({operatorTokenName})",
                AnnotationProduct.Annotation,
                $"Expected an rule term after operator ({operatorTokenName}) but did not find any.",
                this.Sequence(Token(operatorTokenName), this.Not(ruleTerms))
            );

            MissingTermAfterOperatorInRemainderError[operatorTokenName] = matchMissingTermError;

            // user used a different operator by mistake (?) eg: a, b | c, d or a wrong token altogether
            // eg a, b, . c
            var matchWrongOperatorError = this.LogError(
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

            var matchOperatorSequenceError = this.LogError(
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

        private MatchFunctionSequence<int> CreateMatchLog(MatchOneOfFunction<int> condition)
        {
            var matchLogLevel = this.OneOf(
                "LogLevel",
                AnnotationProduct.Annotation,
                Token(CommonTokenNames.LogFatal, AnnotationProduct.Annotation, CommonTokenNames.LogFatal),
                Token(CommonTokenNames.LogError, AnnotationProduct.Annotation, CommonTokenNames.LogError),
                Token(CommonTokenNames.LogWarning, AnnotationProduct.Annotation, CommonTokenNames.LogWarning),
                Token(CommonTokenNames.LogInfo, AnnotationProduct.Annotation, CommonTokenNames.LogInfo),
                Token(CommonTokenNames.LogDebug, AnnotationProduct.Annotation, CommonTokenNames.LogDebug)
            );

            var matchText = MatchLiteral;

            var matchOptionalCondition = this.ZeroOrOne(
                "#OptionalLogCondition", 
                AnnotationProduct.Transitive, 
                this.Sequence(
                    "LogCondition",
                    AnnotationProduct.Transitive,
                    Token(CommonTokenNames.If),
                    condition
                )
            );

            return this.Sequence(
                "MatchLog",
                AnnotationProduct.Annotation,
                matchLogLevel,
                matchText,
                matchOptionalCondition
            );
        }
    }
}