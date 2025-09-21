using gg.core.util;

using gg.parse.rulefunctions;
using gg.parse.rulefunctions.datafunctions;
using gg.parse.rulefunctions.rulefunctions;

using gg.parse.script.common;

using static gg.parse.script.common.CommonRules;

namespace gg.parse.script.parser
{
    /// <summary>
    /// Turns a list of tokens into an abstract syntax tree according to EBNF(like) grammar.
    /// </summary>
    public class ScriptParser : CommonGraphWrapper<int>
    {
        public ScriptTokenizer Tokenizer { get; init; }

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

        public MatchFunctionSequence<int> MatchRuleHeader{ get; private set; }

        public MatchFunctionSequence<int> MatchLog { get; private set; }

        public MatchFunctionSequence<int> Include { get; private set; }

        public MatchFunctionSequence<int> MatchUnexpectedProductInBodyError { get; private set; }

        public LogRule<int> UnexpectedProductInBodyError { get; private set; }

        public LogRule<int> RuleBodyError { get; private set; }

        public LogRule<int> UnknownInputError { get; private set; }

        public LogRule<int> ExpectedOperatorError { get; private set; }

        public LogRule<int> MissingRuleEndError { get; private set; }

        public LogRule<int> InvalidProductInHeaderError { get; private set; }

        public LogRule<int> MissingAssignmentError { get; private set; }

        public LogRule<int> InvalidPrecedenceError { get; private set; }

        public LogRule<int> MissingUnaryOperatorTerm { get; private set; }

        public Dictionary<string, LogRule<int>> MissingOperatorError { get; init; } = [];

        public Dictionary<string, LogRule<int>> MissingTermAfterOperatorInRemainderError { get; init; } = [];

        public Dictionary<string, LogRule<int>> MissingTermAfterOperatorError { get; init; } = [];

        public Dictionary<string, LogRule<int>> WrongOperatorTokenError { get; init; } = [];

        private MatchNotFunction<int> Eof { get; set; }

        private MatchAnyData<int> MatchAny { get; set; }

        private MatchSingleData<int> GroupStartToken { get; set; }

        private MatchSingleData<int> GroupEndToken { get; set; }

        private MatchSingleData<int> RuleEndToken { get; set; }

        public MatchSingleData<int> IdentifierToken { get; set; }

        private MatchSingleData<int> AssignmentToken { get; set; }


        public ScriptParser()
            : this(new ScriptTokenizer())
        {
        }
        

        public ScriptParser(ScriptTokenizer tokenizer)
        {
            Tokenizer = tokenizer;

            
            RuleEndToken = Token(CommonTokenNames.EndStatement);
            GroupStartToken = Token(CommonTokenNames.GroupStart);
            GroupEndToken = Token(CommonTokenNames.GroupEnd);
            AssignmentToken = Token(CommonTokenNames.Assignment);
            MatchAny = new MatchAnyData<int>("Any");
            Eof = new MatchNotFunction<int>("~EOF", MatchAny);
            IdentifierToken = Token("IdentifierToken", AnnotationProduct.Annotation, CommonTokenNames.Identifier);

            MatchTransitiveSelector = Token("TransitiveSelector", AnnotationProduct.Annotation, CommonTokenNames.TransitiveSelector);
            MatchNoProductSelector = Token("NoProductSelector", AnnotationProduct.Annotation, CommonTokenNames.NoProductSelector);

            var ruleProduction = CreateMatchAnnotationProduction();

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
                IdentifierToken
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

            MissingUnaryOperatorTerm = error("MissingUnaryOperatorTerm", "Expecting term after an unary operator (try, !,?,+, or *).");

            var unaryDataTermsOptions =
                oneOf(
                    "#UnaryDataTermsOptions",
                    unaryAndDataTerms,
                    MissingUnaryOperatorTerm
                );

            // *(a | b | c)
            MatchZeroOrMoreOperator = this.Sequence("ZeroOrMore", AnnotationProduct.Annotation,
                Token(CommonTokenNames.ZeroOrMoreOperator),
                unaryDataTermsOptions);

            // ?(a | b | c)
            MatchZeroOrOneOperator = this.Sequence("ZeroOrOne", AnnotationProduct.Annotation,
                Token(CommonTokenNames.ZeroOrOneOperator),
                unaryDataTermsOptions);

            // +(a | b | c)
            MatchOneOrMoreOperator = this.Sequence("OneOrMore", AnnotationProduct.Annotation,
                Token(CommonTokenNames.OneOrMoreOperator),
                unaryDataTermsOptions);


            // !(a | b | c)
            MatchNotOperator = sequence(
                "Not", 
                Token(CommonTokenNames.NotOperator),
                unaryDataTermsOptions
            );

            // >(a | b | c) / try ( a | b | c)
            TryMatchOperator = this.Sequence("TryMatch", AnnotationProduct.Annotation,
                this.OneOf(Token(CommonTokenNames.TryMatchOperator), Token(CommonTokenNames.TryMatchOperatorShortHand)),
                unaryDataTermsOptions);

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
            UnexpectedProductInBodyError = this.LogError(
                    "UnexpectedProductionModifier",
                    AnnotationProduct.Annotation,
                    "Found an unexpected annotation production modifier. These can only appear in front of references to other rules or rule declarations."
            );

            MatchUnexpectedProductInBodyError = this.Sequence(
                "UnexpectedProductErrorMatch",
                AnnotationProduct.Annotation,
                ruleProduction,
                this.OneOf(
                    "#UnexpectedProductErrorMatchTerm",
                    AnnotationProduct.Transitive,
                    [.. matchDataRules, .. unaryOperators, MatchGroup]
                ),
                UnexpectedProductInBodyError
            );

            MatchLog = CreateMatchLog(ruleBody);

            unaryAndDataTerms.RuleOptions = [
                ..unaryAndDataTerms.RuleOptions,
                ..unaryOperators,
                MatchGroup,
                MatchLog,
                MatchUnexpectedProductInBodyError
            ];

            Include = this.Sequence(
                "Include",
                AnnotationProduct.Annotation,
                Token(CommonTokenNames.Include),
                MatchLiteral,
                Token(CommonTokenNames.EndStatement)
            );

            MatchRuleName = token("RuleName", Tokenizer.FindRule(CommonTokenNames.Identifier)!.Id);
            MatchPrecedence = token("RulePrecedence", Tokenizer.FindRule(CommonTokenNames.Integer)!.Id);

            InvalidPrecedenceError = error(
                "PrecedenceNotFoundError", 
                "Expecting precedence number.",
                // xxx this is rather weak test eg as it will fail rule () = .; because () are two tokens
                sequence(any(), ifMatches(AssignmentToken))
            );

            MatchRuleHeader = sequence(
                "#RuleDeclaration",
                CreateMatchHeaderAnnotationProduction(),
                MatchRuleName,
                zeroOrOne("#Precedence",
                    oneOf("#RulePrecedenceOptions",
                        // ie no a precedence
                        ifMatches(AssignmentToken),
                        MatchPrecedence,
                        InvalidPrecedenceError
                    )
                )
            );           

            RuleBodyError = error(
                "RuleBodyError",
                "Unexpected token(s) in the rule's body.",
                this.Skip(stopCondition: RuleEndToken, failOnEoF: false)
            );

            var emptyBodyWarning = warning("NoRuleBodyWarning", "Rule has no body.", ifMatches(RuleEndToken));

            var ruleBodyOptions = oneOf("#RuleBodyOptions", ruleBody, emptyBodyWarning, RuleBodyError);

            MissingRuleEndError = this.LogError(
                "MissingEndRule",
                AnnotationProduct.Annotation,
                "Missing end of rule (;) at the given position.",
                // skip until the start of the next rule, if any
                skip("~skipUntilNextHeaderOrEof", MatchRuleHeader, failOnEoF: false)
            );

            var endStatementOptions = this.OneOf(
                "#EndStatementOptions",
                AnnotationProduct.Transitive,
                RuleEndToken,
                MissingRuleEndError
            );

            MissingAssignmentError = error("MissingAssignmentError", "Assignment token '=', expected but encountered something different.");

            MatchRule = sequence(
                "Rule",
                MatchRuleHeader,
                oneOf("#RuleAssignmentToken", AssignmentToken, MissingAssignmentError),
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
            Assertions.RequiresNotNullOrEmpty(text, nameof(text));

            var tokenizationResult = Tokenizer.Tokenize(text);

            if (tokenizationResult.FoundMatch && tokenizationResult.Annotations != null)
            {
                if (ContainsTokenErrors(tokenizationResult.Annotations,out var tokenizerErrors))
                {
                    throw new TokenizeException(
                        "input contains characters which could not be mapped to a token.",
                        tokenizerErrors,
                        text
                    );
                }

                return tokenizationResult.Annotations!;
            }

            throw new TokenizeException("input contains no valid tokens.");
        }

        private Func<Annotation, bool> SetupErrorPredicate(bool failOnWarning)
        {
            var errorLevel = FailOnWarning
                    ? LogLevel.Warning | LogLevel.Error
                    : LogLevel.Error;

            return new Func<Annotation, bool>(
                a => a.Rule is LogRule<int> logRule && (logRule.Level & errorLevel) > 0
            );
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

                if (ContainsParseErrors(astNodes, FailOnWarning, out var grammarErrors))
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

        private bool ContainsTokenErrors(List<Annotation> annotations, out List<Annotation> errors)
        {
            var unknownTokenId = Tokenizer.FindRule(CommonTokenNames.UnknownToken);

            var errorPredicate = new Func<Annotation, bool>(a => a.Rule == unknownTokenId);

            errors = [];

            foreach (var annotation in annotations)
            {
                annotation.Collect(errorPredicate, errors);
            }

            return errors.Count > 0;
        }

        private bool ContainsParseErrors(List<Annotation> annotations, bool failOnWarning, out List<Annotation> errors)
        {
            var errorPredicate = SetupErrorPredicate(FailOnWarning);
            errors = new List<Annotation>();

            foreach (var annotation in annotations)
            {
                annotation.Collect(errorPredicate, errors);
            }

            return errors.Count > 0;
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

        private MatchOneOfFunction<int>/*MatchFunctionCount<int>*/ CreateMatchHeaderAnnotationProduction()
        {
            MatchTransitiveSelector = Token("TransitiveSelector", AnnotationProduct.Annotation, CommonTokenNames.TransitiveSelector);
            MatchNoProductSelector = Token("NoProductSelector", AnnotationProduct.Annotation, CommonTokenNames.NoProductSelector);

            InvalidProductInHeaderError = error(
                "InvalidProductInHeaderError",
                $"Expected either '{AnnotationProduct.None.GetPrefix()}' or '{AnnotationProduct.Transitive.GetPrefix()}' but found something else entirely.",
                any()
            );

            return oneOf(
                "#RuleProduction",
                // meaning no production
                ifMatches(IdentifierToken),
                MatchTransitiveSelector,
                MatchNoProductSelector,
                InvalidProductInHeaderError
            );
        }

        private MatchFunctionCount<int> CreateMatchAnnotationProduction()
        {
            return this.ZeroOrOne("#RuleProduction", AnnotationProduct.Transitive,
                this.OneOf("ProductionSelection", AnnotationProduct.Transitive,
                    MatchTransitiveSelector,
                    MatchNoProductSelector
                )
            );
        }
    }
}