// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun
#nullable disable

using gg.parse.rules;
using gg.parse.script.common;

namespace gg.parse.script.parser
{
    public class ScriptParser : CommonParser
    {
        // short hands for annotation pruning tokens
        private const string pa = AnnotationPruningToken.All;
        private const string pr = AnnotationPruningToken.Root;

        public static class Names
        {
            public const string Any = "any";

            public const string CharacterRange = "char_range";
            public const string CharacterSet = "char_set";

            public const string Find = "find";

            public const string Evaluation = "eval";

            public const string If = "if";

            public const string Literal = "lit";

            public const string Log = "log";
            public const string LogLevel = "log_lvl";
            public const string LogLevelDebug = "log_lvl_debug";
            public const string LogLevelError = "log_lvl_error";
            public const string LogLevelFatal = "log_lvl_fatal";            
            public const string LogLevelInfo = "log_lvl_info";
            public const string LogLevelWarning = "log_lvl_warning";

            public const string Not = "not";

            public const string OneOrMore = "one_or_more";
            public const string Option = "option";

            public const string Reference = "ref";

            public const string Sequence = "sequence";
            public const string Skip = "skip";

            public const string ZeroOrOne = "zero_or_one";
            public const string ZeroOrMore = "zero_or_more";
        }

        // xxx put in alphabetical order
        public MatchOneOf<int> MatchLiteral { get; private set; }

        public MatchSingleData<int> MatchAnyToken { get; private set; }

        public MatchSingleData<int> MatchPruneRootToken { get; private set; }

        public MatchSingleData<int> MatchPruneChildrenToken { get; private set; }

        public MatchSingleData<int> MatchPruneAllToken { get; private set; }

        public MatchRuleSequence<int> MatchRule { get; private set; }

        public MatchSingleData<int> MatchRuleName { get; private set; }

        public MatchSingleData<int> MatchPrecedence { get; private set; }

        public MatchRuleSequence<int> MatchReference { get; private set; }

        public MatchRuleSequence<int> MatchSequence { get; private set; }

        public MatchRuleSequence<int> MatchOneOf { get; private set; }

        public MatchRuleSequence<int> MatchEval { get; private set; }

        public MatchRuleSequence<int> MatchCharacterSet { get; private set; }

        public MatchRuleSequence<int> MatchCharacterRange { get; private set; }

        public MatchRuleSequence<int> MatchGroup { get; private set; }

        public MatchRuleSequence<int> MatchZeroOrMoreOperator { get; private set; }

        public MatchRuleSequence<int> MatchZeroOrOneOperator { get; private set; }

        public MatchRuleSequence<int> MatchOneOrMoreOperator { get; private set; }

        public MatchRuleSequence<int> MatchNotOperator { get; private set; }

        public MatchRuleSequence<int> IfMatchOperator { get; private set; }

        public MatchRuleSequence<int> MatchSkipOperator { get; private set; }

        public MatchRuleSequence<int> MatchFindOperator { get; private set; }

        public MatchOneOf<int> MatchRuleBody { get; private set; }

        public MatchRuleSequence<int> MatchRuleHeader{ get; private set; }

        public MatchRuleSequence<int> MatchLog { get; private set; }

        public MatchRuleSequence<int> Include { get; private set; }

        public MatchRuleSequence<int> MatchUnexpectedPruneTokenInBodyError { get; private set; }

        public LogRule<int> UnexpectedPrunetokenInBodyError { get; private set; }

        public LogRule<int> RuleBodyError { get; private set; }

        public LogRule<int> UnknownInputError { get; private set; }

        public LogRule<int> ExpectedOperatorError { get; private set; }

        public LogRule<int> MissingRuleEndError { get; private set; }

        public LogRule<int> InvalidPruneTokenInHeaderError { get; private set; }

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
            : base(tokenizer)
        {
            RegisterTokens();

            MatchRuleHeader = RegisterRuleHeaderMatchers();
            MatchRuleBody = RegisterRuleBodyMatchers();

            MatchRule = RegisterRule(MatchRuleHeader, MatchRuleBody);

            Include = Sequence("include", IncludeToken!, MatchLiteral!, RuleEndToken!);

            var validStatement = OneOf($"{pr}validStatement", Include, MatchRule);

            // fallback in case nothing matches
            UnknownInputError = Error(
                "unknown_input",
                "Can't match token(s) to a grammar expression.",
                Skip($"{pa}skip_until_next_valid_statement", stopCondition: validStatement, failOnEoF: false)
            );            

            Root = ZeroOrMore($"{pr}root", OneOf($"{pr}statement", validStatement, UnknownInputError));
        }

        private MatchRuleSequence<int> RegisterRuleHeaderMatchers()
        {
            MatchRuleName = MatchSingle("rule_name", Tokenizer.FindRule(CommonTokenNames.Identifier)!.Id);
            MatchPrecedence = MatchSingle("rule_precedence", Tokenizer.FindRule(CommonTokenNames.Integer)!.Id);

            InvalidPrecedenceError = Error(
                "precedenceNotFoundError",
                "Expecting precedence number.",
                // xxx this is rather weak test eg as it will fail rule () = .; because () are two tokens
                Sequence(Any(), IfMatch(AssignmentToken))
            );

            return Sequence(
                $"{pr}rule_header",
                CreateMatchHeaderAnnotationProduction(),
                // xxx should handle incorrect rulename / error
                MatchRuleName,
                ZeroOrOne($"{pr}precedence",
                    OneOf($"{pr}rule_precedence_options",
                        // if we match an assingment there's no a precedence value
                        IfMatch(AssignmentToken),
                        // handle the precedence value
                        MatchPrecedence,
                        // didn't find a precedence or assignment (ie the start of the rulebody), so 
                        // log an error
                        InvalidPrecedenceError
                    )
                )
            );
        }

        /// <summary>
        /// Register all the matchers which can be used in the body of a rule and
        /// return a matcher which can match a rule body term.
        /// </summary>
        /// <returns></returns>
        private MatchOneOf<int> RegisterRuleBodyMatchers()
        {
            // register matching rules like literals, identifiers, character sets/ranges, any character
            var dataMatchersArray = RegisterDataMatchers();

            // unary terms are single terms, ie everything but binary operators
            var unaryTerms = OneOf($"{pr}unaryTerms", dataMatchersArray);

            // operators like not(x), if(x), *(x), +(x), ?(x)
            var unaryOperators = RegisterUnaryOperatorMatchers(unaryTerms);

            // operators like a | b | c , a , b , c , a / b / c
            var binaryOperators = OneOf($"{pr}binaryRuleTerms", RegisterBinaryOperatorMatchers(unaryTerms));
            
            var ruleBody = OneOf($"{pr}ruleBody", binaryOperators, unaryTerms);

            // ( a, b, c )
            MatchGroup = Sequence($"{pr}group", GroupStartToken!, ruleBody, GroupEndToken!);

            // create all various instances of logs (errors,warnings,infos...)
            MatchLog = CreateMatchLog(ruleBody);

            RuleBase<int>[] recoveryRules = [.. dataMatchersArray, .. unaryOperators, MatchGroup];
            var ruleBodyErrorHandler = RegisterRuleBodyErrorHandlers(recoveryRules);

            unaryTerms.RuleOptions = [
                ..unaryTerms.RuleOptions,
                ..unaryOperators,
                MatchGroup,
                MatchLog,
                ruleBodyErrorHandler
            ];

            return ruleBody;
        }

        private MatchRuleSequence<int> RegisterRule(RuleBase<int> ruleHeader, RuleBase<int> ruleBody)
        {
            RuleBodyError = Error(
                "rule_body_error",
                "Unexpected token(s) in the rule's body.",
                Skip(stopCondition: RuleEndToken, failOnEoF: false)
            );

            var emptyBodyWarning = Warning("no_rule_body_warning", "Rule has no body.", IfMatch(RuleEndToken));

            var ruleBodyOptions = OneOf($"{pr}rule_body_options", ruleBody, emptyBodyWarning, RuleBodyError);

            MissingRuleEndError = Error(
                "missing_end_rule",
                "Missing end of rule (;) at the given position.",
                // skip until the start of the next rule, if any
                Skip($"{pa}skip_until_next_header_or_eof", ruleHeader, failOnEoF: false)
            );

            var endStatementOptions = OneOf(
                $"{pr}end_statement_options",
                RuleEndToken,
                MissingRuleEndError
            );

            MissingAssignmentError = Error(
                "missing_assignment_Error",
                "Assignment token '=', expected but encountered something different."
            );

            MatchRule = Sequence(
                "rule",
                ruleHeader,
                OneOf($"{pr}rule_assignment_token", AssignmentToken, MissingAssignmentError),
                ruleBodyOptions,
                endStatementOptions
            );

            return MatchRule;
        }

        private MatchOneOf<int> RegisterRuleBodyErrorHandlers(RuleBase<int>[] recoveryRules)
        {
            // A stray output modifier found, output modifier can only appear in front of references
            // because they don't make any sense elsewhere (or at least I'm not aware of a valid use case).
            // Match -r, -c or -a inside the rule, if found, raise an error and skip until the next token,
            // in script: (-a|-r|-c), error "unexpected product modifier" .
            UnexpectedPrunetokenInBodyError = Error(
                    "unexpected_pruning_token",
                    "Found an unexpected annotation output modifier. These can only appear in front of references to other rules or rule declarations."
            );

            // error for cases where a product is not followed by a valid term, eg #&, -r; or -a followed by EOF
            MatchUnexpectedPruneTokenInBodyError = Sequence(
                "unexpected_prune_token_error_match",
                CreateMatchBodyAnnotationProduction(),
                OneOf($"{pr}UnexpectedProductErrorMatchTerm", recoveryRules),
                UnexpectedPrunetokenInBodyError
            );

            // xxx add more error conditions here

            return OneOf($"{pr}rulebody_error_handler", MatchUnexpectedPruneTokenInBodyError);
        }

        private RuleBase<int>[] RegisterDataMatchers()
        {
            // .
            // MatchAnyToken = Token(CommonTokenNames.AnyCharacter, CommonTokenNames.AnyCharacter);
            MatchAnyToken = Token(Names.Any, CommonTokenNames.AnyCharacter);

            // "abc" or 'abc'
            MatchLiteral = OneOf(
                Names.Literal,
                Token(CommonTokenNames.SingleQuotedString),
                Token(CommonTokenNames.DoubleQuotedString)
            );

            // { "abcf" }
            MatchCharacterSet = Sequence(
                Names.CharacterSet,
                Token(CommonTokenNames.ScopeStart),
                MatchLiteral,
                Token(CommonTokenNames.ScopeEnd)
            );

            // { 'a' .. 'z' }
            MatchCharacterRange = Sequence(
                    Names.CharacterRange,
                    Token(CommonTokenNames.ScopeStart),
                    MatchLiteral,
                    Token(CommonTokenNames.Elipsis),
                    MatchLiteral,
                    Token(CommonTokenNames.ScopeEnd)
            );

            // reference
            MatchReference = Sequence(
                Names.Reference,
                CreateMatchBodyAnnotationProduction(),
                IdentifierToken!
            );

            return [
                MatchLiteral,
                MatchAnyToken!,
                MatchCharacterSet,
                MatchCharacterRange,
                MatchReference
            ];
        }

        private RuleBase<int>[] RegisterUnaryOperatorMatchers(MatchOneOf<int> unaryTerms)
        {
            MissingUnaryOperatorTerm = Error("missing_unary_operator_term", "Expecting term after an unary operator (try, !,?,+, or *).");

            var unaryDataTermsOptions =
                OneOf(
                    $"{pr}unary_data_terms_options",
                    unaryTerms,
                    MissingUnaryOperatorTerm
                );

            // *(a | b | c)
            MatchZeroOrMoreOperator = Sequence(
                Names.ZeroOrMore,
                Token(CommonTokenNames.ZeroOrMoreOperator),
                unaryDataTermsOptions
            );

            // ?(a | b | c)
            MatchZeroOrOneOperator = Sequence(
                Names.ZeroOrOne,
                Token(CommonTokenNames.ZeroOrOneOperator),
                unaryDataTermsOptions
            );

            // +(a | b | c)
            MatchOneOrMoreOperator = Sequence(
                Names.OneOrMore,
                Token(CommonTokenNames.OneOrMoreOperator),
                unaryDataTermsOptions
            );

            // !(a | b | c)
            MatchNotOperator = Sequence(
                Names.Not,
                Token(CommonTokenNames.NotOperator),
                unaryDataTermsOptions
            );

            // if ( a | b | c)
            IfMatchOperator = Sequence(
                Names.If,
                Token(CommonTokenNames.If),
                unaryDataTermsOptions
            );

            // >> a
            MatchFindOperator = Sequence(
                Names.Find,
                Token(CommonTokenNames.FindOperator),
                unaryDataTermsOptions
            );

            // >>> a
            MatchSkipOperator = Sequence(
                Names.Skip,
                Token(CommonTokenNames.SkipOperator),
                unaryDataTermsOptions
            );

            return
            [
                MatchZeroOrMoreOperator,
                MatchZeroOrOneOperator,
                MatchOneOrMoreOperator,
                MatchNotOperator,
                IfMatchOperator,
                MatchFindOperator,
                MatchSkipOperator
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
            
            MatchPruneAllToken = Token(CommonTokenNames.PruneAll, CommonTokenNames.PruneAll);
            MatchPruneChildrenToken = Token(CommonTokenNames.PruneChildren, CommonTokenNames.PruneChildren);
            MatchPruneRootToken = Token(CommonTokenNames.PruneRoot, CommonTokenNames.PruneRoot);
        }

        private RuleBase<int>[] RegisterBinaryOperatorMatchers(MatchOneOf<int> unaryTerms)
        {
            // mainSequence contains both the match and error handling
            (var mainSequence, MatchSequence) = CreateBinaryOperator(Names.Sequence, CommonTokenNames.CollectionSeparator, unaryTerms);

            // a | b | c
            // mainOption contains both the match and error handling
            (var mainOption, MatchOneOf) = CreateBinaryOperator(Names.Option, CommonTokenNames.OneOf, unaryTerms);

            // a / b / c
            // mainEval contains both the match and error handling
            (var mainEval, MatchEval) = CreateBinaryOperator(Names.Evaluation, CommonTokenNames.OptionWithPrecedence, unaryTerms);

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
        private (MatchOneOf<int> mainFunction, MatchRuleSequence<int> matchOperationFunction)
            CreateBinaryOperator(string name, string operatorTokenName, MatchOneOf<int> ruleTerms)
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
                    $"{pr}next_{name}_element",
                    Token(operatorTokenName),
                    ruleTerms);

            var eof = Not(Any());
            var notEndOfOperator = Not(OneOf(eof, RuleEndToken, GroupEndToken));

            // user forgot an operator eg: a, b  c;
            var matchMissingOperatorError = Error(
                $"missing_operator_error({operatorTokenName})",
                $"Expected an operator ({operatorTokenName}) but did not find any.",
                ruleTerms
            );

            MissingOperatorError[operatorTokenName] = matchMissingOperatorError;

            // user forgot an term after the operator eg: a, b, ;
            var matchMissingTermError = Error(
                $"missing_term_error({operatorTokenName})",
                $"Expected an rule term after operator ({operatorTokenName}) but did not find any.",
                Sequence(Token(operatorTokenName), Not(ruleTerms))
            );

            MissingTermAfterOperatorInRemainderError[operatorTokenName] = matchMissingTermError;

            // user used a different operator by mistake (?) eg: a, b | c, d or a wrong token altogether
            // eg a, b, . c
            var matchWrongOperatorError = Error(
                $"wrong_operator_error({operatorTokenName})",
                $"Expected an operator ({operatorTokenName}) but found something else.",
                Sequence(Not(operatorToken), Any(), ruleTerms)
            );

            WrongOperatorTokenError[operatorTokenName] = matchWrongOperatorError;

            var matchOperatorError = Sequence(
                $"{pr}match_operator_errors({operatorTokenName})",
                notEndOfOperator,
                OneOf(
                    $"{pr}one_of_operator_error({operatorTokenName})",
                    matchMissingTermError,
                    matchMissingOperatorError,
                    matchWrongOperatorError
                )
            );

            var remainder = OneOf(
                $"{pr}operator_remainder({operatorTokenName})",
                nextElement,
                matchOperatorError
            );

            var matchOperatorSequence = Sequence(
                name,
                ruleTerms,
                operatorToken,
                ruleTerms,
                ZeroOrMore($"{pr}{name}_remainder", remainder)
            );

            var matchOperatorSequenceError = Error(
                $"match_operator_sequence_error({operatorTokenName})",
                $"Expected a rule term after operator ({operatorTokenName}) but found something else.",
                Sequence(ruleTerms, operatorToken, Not(ruleTerms))
            );

            MissingTermAfterOperatorError[operatorTokenName] = matchOperatorSequenceError;

            return (
                OneOf(
                    $"{pr}one_of_binary_operator(({operatorTokenName})",
                    matchOperatorSequence,
                    matchOperatorSequenceError
                ),
                matchOperatorSequence
            );
        }

        private MatchRuleSequence<int> CreateMatchLog(MatchOneOf<int> condition)
        {
            var matchLogLevel = OneOf(
                Names.LogLevel,
                Token(Names.LogLevelFatal, CommonTokenNames.LogFatal),
                Token(Names.LogLevelError, CommonTokenNames.LogError),
                Token(Names.LogLevelWarning, CommonTokenNames.LogWarning),
                Token(Names.LogLevelInfo, CommonTokenNames.LogInfo),
                Token(Names.LogLevelDebug, CommonTokenNames.LogDebug)
            );

            var matchText = MatchLiteral;

            var matchOptionalCondition = ZeroOrOne(
                $"{pr}optional_log_vondition", 
                Sequence(
                    $"{pr}log_condition",
                    Token(CommonTokenNames.If),
                    condition
                )
            );

            return Sequence(
                Names.Log,
                matchLogLevel,
                matchText,
                matchOptionalCondition
            );
        }

        private MatchOneOf<int> CreateMatchHeaderAnnotationProduction()
        {
            InvalidPruneTokenInHeaderError = Error(
                "invalid_prune_token_in_rule_header_error",
                $"Expected either '{AnnotationPruning.All.GetTokenString()}' or '{AnnotationPruning.Root.GetTokenString()}' but found something else entirely.",
                Any()
            );

            return OneOf(
                $"{pr}header_rule_pruning",
                // if an indentifier token is found, it means there is no output
                IfMatch(IdentifierToken),
                MatchPruneAllToken,
                MatchPruneChildrenToken,
                MatchPruneRootToken,
                InvalidPruneTokenInHeaderError
            );
        }

        /// <summary>
        /// Annotation output matching for references in the rule's body
        /// </summary>
        /// <returns></returns>
        private MatchCount<int> CreateMatchBodyAnnotationProduction()
        {
            return ZeroOrOne(
                $"{pr}ruleBodyProduction", 
                OneOf(
                    $"{pr}productionSelection",
                    MatchPruneAllToken,
                    MatchPruneChildrenToken,
                    MatchPruneRootToken
                )
            );
        }
    }
}
#nullable enable