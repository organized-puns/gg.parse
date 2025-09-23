using gg.parse.rules;
using gg.parse.script.common;

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

        public MatchFunctionSequence<int> IfMatchOperator { get; private set; }

        public MatchOneOfFunction<int> MatchRuleBody { get; private set; }

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

        public MatchSingleData<int> AssignmentToken { get; set; }

        public MatchSingleData<int> GroupStartToken { get; set; }

        public MatchSingleData<int> GroupEndToken { get; set; }

        public MatchSingleData<int> IdentifierToken { get; set; }

        public MatchSingleData<int> IncludeToken { get; set; }

        public MatchSingleData<int> RuleEndToken { get; set; }

        public ScriptParser()
            : this(new ScriptTokenizer())
        {
        }
        
        public ScriptParser(ScriptTokenizer tokenizer)
        {
            Tokenizer = tokenizer;

            RegisterTokens();

            MatchRuleHeader = RegisterRuleHeader();
            MatchRuleBody = RegisterRuleBody();

            MatchRule = RegisterRule(MatchRuleHeader, MatchRuleBody);

            Include = Sequence("include", IncludeToken!, MatchLiteral!, RuleEndToken!);

            var validStatement = OneOf("#validStatement", Include, MatchRule);

            // fallback in case nothing matches
            UnknownInputError = Error(
                "UnknownInput",
                "Can't match token(s) to a grammar expression.",
                Skip("~skipUntilNextValidStatement", stopCondition: validStatement, failOnEoF: false)
            );            

            Root = ZeroOrMore("#root", OneOf("#statement", validStatement, UnknownInputError));
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

        private MatchSingleData<int> Token(string tokenName) =>
            MatchSingle($"{AnnotationProduct.None.GetPrefix()}Token({tokenName})", Tokenizer.FindRule(tokenName)!.Id);

        private MatchSingleData<int> Token(string ruleName, string tokenName) =>
            MatchSingle($"Token({ruleName})", Tokenizer.FindRule(tokenName)!.Id);


        private MatchFunctionSequence<int> RegisterRuleHeader()
        {
            MatchRuleName = MatchSingle("RuleName", Tokenizer.FindRule(CommonTokenNames.Identifier)!.Id);
            MatchPrecedence = MatchSingle("RulePrecedence", Tokenizer.FindRule(CommonTokenNames.Integer)!.Id);

            InvalidPrecedenceError = Error(
                "PrecedenceNotFoundError",
                "Expecting precedence number.",
                // xxx this is rather weak test eg as it will fail rule () = .; because () are two tokens
                Sequence(Any(), IfMatch(AssignmentToken))
            );

            MatchRuleHeader = Sequence(
                "#RuleDeclaration",
                CreateMatchHeaderAnnotationProduction(),
                MatchRuleName,
                ZeroOrOne("#Precedence",
                    OneOf("#RulePrecedenceOptions",
                        // ie no a precedence
                        IfMatch(AssignmentToken),
                        MatchPrecedence,
                        InvalidPrecedenceError
                    )
                )
                );

            return MatchRuleHeader;
        }

        private MatchOneOfFunction<int> RegisterRuleBody()
        {
            var dataMatchers = OneOf("#DataMatchers", RegisterDataMatchers());
            
            var ruleBody = OneOf(
                "#RuleBody",
                // match this before unary terms
                OneOf("#BinaryRuleTerms", RegisterBinaryOperators(dataMatchers)),
                dataMatchers
            );
            
            var unaryOperators = RegisterUnaryOperators(dataMatchers);

            // ( a, b, c )
            MatchGroup = Sequence("#Group", GroupStartToken!, ruleBody, GroupEndToken!);

            // A stray production modifier found, production modifier can only appear in front of references
            // because they don't make any sense elsewhere (or at least I'm not aware of a valid use case).
            // Match ~ or # inside the rule, if found, raise an error and skip until the next token,
            // in script: (~|#), error "unexpected product modifier" .
            UnexpectedProductInBodyError = Error(
                    "UnexpectedProductionModifier",
                    "Found an unexpected annotation production modifier. These can only appear in front of references to other rules or rule declarations."
            );

            // error for cases where a product is not followed by a valid term, eg #&, ~; or # followed by EOF
            MatchUnexpectedProductInBodyError = Sequence(
                "UnexpectedProductErrorMatch",
                CreateMatchBodyAnnotationProduction(),
                OneOf("#UnexpectedProductErrorMatchTerm", [.. RegisterDataMatchers(), .. unaryOperators, MatchGroup]),
                UnexpectedProductInBodyError
            );

            // create all various instances of logs (errors,warnings,infos...)
            MatchLog = CreateMatchLog(ruleBody);

            dataMatchers.RuleOptions = [
                ..dataMatchers.RuleOptions,
                ..unaryOperators,
                MatchGroup,
                MatchLog,
                MatchUnexpectedProductInBodyError
            ];

            return ruleBody;
        }

        private MatchFunctionSequence<int> RegisterRule(RuleBase<int> ruleHeader, RuleBase<int> ruleBody)
        {
            RuleBodyError = Error(
                "RuleBodyError",
                "Unexpected token(s) in the rule's body.",
                Skip(stopCondition: RuleEndToken, failOnEoF: false)
            );

            var emptyBodyWarning = Warning("NoRuleBodyWarning", "Rule has no body.", IfMatch(RuleEndToken));

            var ruleBodyOptions = OneOf("#RuleBodyOptions", ruleBody, emptyBodyWarning, RuleBodyError);

            MissingRuleEndError = Error(
                "MissingEndRule",
                "Missing end of rule (;) at the given position.",
                // skip until the start of the next rule, if any
                Skip("~skipUntilNextHeaderOrEof", ruleHeader, failOnEoF: false)
            );

            var endStatementOptions = OneOf(
                "#EndStatementOptions",
                RuleEndToken,
                MissingRuleEndError
            );

            MissingAssignmentError = Error(
                "MissingAssignmentError",
                "Assignment token '=', expected but encountered something different."
            );

            MatchRule = Sequence(
                "Rule",
                ruleHeader,
                OneOf("#RuleAssignmentToken", AssignmentToken, MissingAssignmentError),
                ruleBodyOptions,
                endStatementOptions
            );

            return MatchRule;
        }


        private RuleBase<int>[] RegisterDataMatchers()
        {
            // .
            MatchAnyToken = Token(CommonTokenNames.AnyCharacter, CommonTokenNames.AnyCharacter);

            // "abc" or 'abc'
            MatchLiteral = OneOf(
                "Literal",
                Token(CommonTokenNames.SingleQuotedString),
                Token(CommonTokenNames.DoubleQuotedString)
            );

            // { "abcf" }
            MatchCharacterSet = Sequence(
                "CharacterSet",
                Token(CommonTokenNames.ScopeStart),
                MatchLiteral,
                Token(CommonTokenNames.ScopeEnd)
            );

            // { 'a' .. 'z' }
            MatchCharacterRange = Sequence(
                    "CharacterRange",
                    Token(CommonTokenNames.ScopeStart),
                    MatchLiteral,
                    Token(CommonTokenNames.Elipsis),
                    MatchLiteral,
                    Token(CommonTokenNames.ScopeEnd)
            );

            // foo or bar
            MatchIdentifier = Sequence(
                "Identifier",
                CreateMatchBodyAnnotationProduction(),
                IdentifierToken!
            );

            return [
                MatchLiteral,
                MatchAnyToken!,
                MatchCharacterSet,
                MatchCharacterRange,
                MatchIdentifier
            ];
        }

        private RuleBase<int>[] RegisterUnaryOperators(MatchOneOfFunction<int> unaryTerms)
        {
            MissingUnaryOperatorTerm = Error("MissingUnaryOperatorTerm", "Expecting term after an unary operator (try, !,?,+, or *).");

            var unaryDataTermsOptions =
                OneOf(
                    "#UnaryDataTermsOptions",
                    unaryTerms,
                    MissingUnaryOperatorTerm
                );

            // *(a | b | c)
            MatchZeroOrMoreOperator = Sequence(
                "ZeroOrMore",
                Token(CommonTokenNames.ZeroOrMoreOperator),
                unaryDataTermsOptions
            );

            // ?(a | b | c)
            MatchZeroOrOneOperator = Sequence(
                "ZeroOrOne",
                Token(CommonTokenNames.ZeroOrOneOperator),
                unaryDataTermsOptions
            );

            // +(a | b | c)
            MatchOneOrMoreOperator = Sequence(
                "OneOrMore",
                Token(CommonTokenNames.OneOrMoreOperator),
                unaryDataTermsOptions
            );

            // !(a | b | c)
            MatchNotOperator = Sequence(
                "Not",
                Token(CommonTokenNames.NotOperator),
                unaryDataTermsOptions
            );

            // if ( a | b | c)
            IfMatchOperator = Sequence(
                "IfMatch",
                Token(CommonTokenNames.If),
                unaryDataTermsOptions
            );

            return
            [
                MatchZeroOrMoreOperator,
                MatchZeroOrOneOperator,
                MatchOneOrMoreOperator,
                MatchNotOperator,
                IfMatchOperator
            ];
        }

        private void RegisterTokens()
        {
            RuleEndToken = Token(CommonTokenNames.EndStatement);
            GroupStartToken = Token(CommonTokenNames.GroupStart);
            GroupEndToken = Token(CommonTokenNames.GroupEnd);
            AssignmentToken = Token(CommonTokenNames.Assignment);
            IncludeToken = Token(CommonTokenNames.Include);

            IdentifierToken = Token(CommonTokenNames.Identifier, CommonTokenNames.Identifier);
            MatchTransitiveSelector = Token(CommonTokenNames.TransitiveSelector, CommonTokenNames.TransitiveSelector);
            MatchNoProductSelector = Token(CommonTokenNames.NoProductSelector, CommonTokenNames.NoProductSelector);
        }

        private RuleBase<int>[] RegisterBinaryOperators(MatchOneOfFunction<int> unaryTerms)
        {
            // mainSequence contains both the match and error handling
            (var mainSequence, MatchSequence) = CreateBinaryOperator("Sequence", CommonTokenNames.CollectionSeparator, unaryTerms);

            // a | b | c
            // mainOption contains both the match and error handling
            (var mainOption, MatchOption) = CreateBinaryOperator("Option", CommonTokenNames.Option, unaryTerms);

            // a / b / c
            // mainEval contains both the match and error handling
            (var mainEval, MatchEval) = CreateBinaryOperator("Evaluation", CommonTokenNames.OptionWithPrecedence, unaryTerms);

            return [mainSequence, mainOption, mainEval];
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

            var nextElement = Sequence(
                    $"#Next{name}Element",
                    Token(operatorTokenName),
                    ruleTerms);

            var eof = Not(Any());
            var notEndOfOperator = Not(OneOf(eof, RuleEndToken, GroupEndToken));

            // user forgot an operator eg: a, b  c;
            var matchMissingOperatorError = Error(
                $"MissingOperatorError({operatorTokenName})",
                $"Expected an operator ({operatorTokenName}) but did not find any.",
                ruleTerms
            );

            MissingOperatorError[operatorTokenName] = matchMissingOperatorError;

            // user forgot an term after the operator eg: a, b, ;
            var matchMissingTermError = Error(
                $"MissingTermError({operatorTokenName})",
                $"Expected an rule term after operator ({operatorTokenName}) but did not find any.",
                Sequence(Token(operatorTokenName), Not(ruleTerms))
            );

            MissingTermAfterOperatorInRemainderError[operatorTokenName] = matchMissingTermError;

            // user used a different operator by mistake (?) eg: a, b | c, d or a wrong token altogether
            // eg a, b, . c
            var matchWrongOperatorError = Error(
                $"WrongOperatorError({operatorTokenName})",
                $"Expected an operator ({operatorTokenName}) but found something else.",
                Sequence(Not(operatorToken), Any(), ruleTerms)
            );

            WrongOperatorTokenError[operatorTokenName] = matchWrongOperatorError;

            var matchOperatorError = Sequence(
                $"#MatchOperatorErrors({operatorTokenName})",
                notEndOfOperator,
                OneOf(
                    $"#OneOfOperatorError({operatorTokenName})",
                    matchMissingTermError,
                    matchMissingOperatorError,
                    matchWrongOperatorError
                )
            );

            var remainder = OneOf(
                $"#OperatorRemainder({operatorTokenName})",
                nextElement,
                matchOperatorError
            );

            var matchOperatorSequence = Sequence(
                name,
                ruleTerms,
                operatorToken,
                ruleTerms,
                ZeroOrMore($"#{name}Remainder", remainder)
            );

            var matchOperatorSequenceError = Error(
                $"MatchOperatorSequenceError({operatorTokenName})",
                $"Expected a rule term after operator ({operatorTokenName}) but found something else.",
                Sequence(ruleTerms, operatorToken, Not(ruleTerms))
            );

            MissingTermAfterOperatorError[operatorTokenName] = matchOperatorSequenceError;

            return (
                OneOf(
                    $"#OneOfBinaryOperator(({operatorTokenName})",
                    matchOperatorSequence,
                    matchOperatorSequenceError
                ),
                matchOperatorSequence
            );
        }

        private MatchFunctionSequence<int> CreateMatchLog(MatchOneOfFunction<int> condition)
        {
            var matchLogLevel = OneOf(
                "LogLevel",
                Token(CommonTokenNames.LogFatal, CommonTokenNames.LogFatal),
                Token(CommonTokenNames.LogError, CommonTokenNames.LogError),
                Token(CommonTokenNames.LogWarning, CommonTokenNames.LogWarning),
                Token(CommonTokenNames.LogInfo, CommonTokenNames.LogInfo),
                Token(CommonTokenNames.LogDebug, CommonTokenNames.LogDebug)
            );

            var matchText = MatchLiteral;

            var matchOptionalCondition = ZeroOrOne(
                "#OptionalLogCondition", 
                Sequence(
                    "#LogCondition",
                    Token(CommonTokenNames.If),
                    condition
                )
            );

            return Sequence(
                "MatchLog",
                matchLogLevel,
                matchText,
                matchOptionalCondition
            );
        }

        private MatchOneOfFunction<int> CreateMatchHeaderAnnotationProduction()
        {
            InvalidProductInHeaderError = Error(
                "InvalidProductInHeaderError",
                $"Expected either '{AnnotationProduct.None.GetPrefix()}' or '{AnnotationProduct.Transitive.GetPrefix()}' but found something else entirely.",
                Any()
            );

            return OneOf(
                "#HeaderRuleProduction",
                // if an indentifier token is found, it means there is no production
                IfMatch(IdentifierToken),
                MatchTransitiveSelector,
                MatchNoProductSelector,
                InvalidProductInHeaderError
            );
        }

        /// <summary>
        /// Annotation production matching for references in the rule's body
        /// </summary>
        /// <returns></returns>
        private MatchFunctionCount<int> CreateMatchBodyAnnotationProduction()
        {
            return ZeroOrOne(
                "#ruleBodyProduction", 
                OneOf(
                    "#productionSelection", 
                    MatchTransitiveSelector,
                    MatchNoProductSelector
                )
            );
        }
    }
}