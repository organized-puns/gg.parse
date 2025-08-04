using gg.parse.rulefunctions;
using Microsoft.VisualBasic.FileIO;
using System.Data;

namespace gg.parse.examples
{
    
    public class EbnfTokenizerParser : RuleTable<int>
    {
        public EbnfTokenizer Tokenizer { get; init; }

        public RuleBase<int> Root { get; private set; }

        public EbnfTokenizerParser()
            : this(new EbnfTokenizer())
        {
        }

        private MatchOneOfFunction<int> _literal;
        private MatchSingleData<int> _transitiveSelector;
        private MatchSingleData<int> _noProductSelector;
        private MatchFunctionCount<int> _ruleProduction;
        private MatchSingleData<int> _ruleName;
        private MatchSingleData<int> _identifier;
        private MatchFunctionSequence<int> _sequence;
        private MatchFunctionSequence<int> _option;
        private MatchFunctionSequence<int> _characterSet;
        private MatchFunctionSequence<int> _characterRange;
        private MatchFunctionSequence<int> _group;
        private MatchFunctionSequence<int> _zeroOrMoreOperator;
        private MatchFunctionSequence<int> _zeroOrOneOperator;
        private MatchFunctionSequence<int> _oneOrMoreOperator;
        private MatchFunctionSequence<int> _notOperator;


        public EbnfTokenizerParser(EbnfTokenizer tokenizer)
        {
            Tokenizer = tokenizer;

            // "abc" or 'abc'
            _literal = OneOf("Literal", AnnotationProduct.Annotation,
                    Token(TokenNames.SingleQuotedString),
                    Token(TokenNames.DoubleQuotedString)
            );

            // { "abcf" }
            _characterSet = Sequence("CharacterSet", AnnotationProduct.Annotation,
                    Token(TokenNames.ScopeStart),
                    _literal,
                    Token(TokenNames.ScopeEnd)
            );

            // { 'a' .. 'z' }
            _characterRange = Sequence("CharacterRange", AnnotationProduct.Annotation,
                    Token(TokenNames.ScopeStart),
                    _literal,
                    Token(TokenNames.Elipsis),
                    _literal,
                    Token(TokenNames.ScopeEnd)
            );

            _identifier = Token("Identifier", AnnotationProduct.Annotation, TokenNames.Identifier);
            
            // literal | set
            var unaryRuleTerms = OneOf("#UnaryRuleTerms", AnnotationProduct.Transitive, _literal, _characterSet, _characterRange, _identifier);

            var nextSequenceElement = Sequence("#NextSequenceElement", AnnotationProduct.Transitive,
                    Token(TokenNames.CollectionSeparator),
                    unaryRuleTerms);

            // a, b, c
            _sequence = Sequence("Sequence", AnnotationProduct.Annotation,
                    unaryRuleTerms,
                    Token(TokenNames.CollectionSeparator),
                    unaryRuleTerms,
                    ZeroOrMore("#SequenceRest", AnnotationProduct.Transitive, nextSequenceElement));

            var nextOptionElement = Sequence("#NextOptionElement", AnnotationProduct.Transitive,
                    Token(TokenNames.Option),
                    unaryRuleTerms);

            // a | b | c
            _option = Sequence("Option", AnnotationProduct.Annotation,
                    unaryRuleTerms,
                    Token(TokenNames.Option),
                    unaryRuleTerms,
                    ZeroOrMore("#OptionRest", AnnotationProduct.Transitive, nextOptionElement));

            var binaryRuleTerms = OneOf("#BinaryRuleTerms", AnnotationProduct.Transitive, _sequence, _option);

            var ruleDefinition = OneOf("#RuleDefinition", AnnotationProduct.Transitive, binaryRuleTerms, unaryRuleTerms);

            // ( a, b, c )
            _group = Sequence("#Group", AnnotationProduct.Transitive,
                Token(TokenNames.GroupStart),
                ruleDefinition,
                Token(TokenNames.GroupEnd));

            // *(a | b | c)
            _zeroOrMoreOperator = Sequence("ZeroOrMore", AnnotationProduct.Annotation,
                Token(TokenNames.ZeroOrMoreOperator),
                unaryRuleTerms);

            // ?(a | b | c)
            _zeroOrOneOperator = Sequence("ZeroOrOne", AnnotationProduct.Annotation,
                Token(TokenNames.ZeroOrOneOperator),
                unaryRuleTerms);

            // +(a | b | c)
            _oneOrMoreOperator = Sequence("OneOrMore", AnnotationProduct.Annotation,
                Token(TokenNames.OneOrMoreOperator),
                unaryRuleTerms);

            // !(a | b | c)
            _notOperator = Sequence("Not", AnnotationProduct.Annotation,
                Token(TokenNames.NotOperator),
                unaryRuleTerms);

            unaryRuleTerms.Options = [.. unaryRuleTerms.Options, _group, _zeroOrMoreOperator, _zeroOrOneOperator, _oneOrMoreOperator, _notOperator];

            _transitiveSelector = Token("TransitiveSelector", AnnotationProduct.Annotation, TokenNames.TransitiveSelector);
            _noProductSelector = Token("NoProductSelector", AnnotationProduct.Annotation, TokenNames.NoProductSelector);

            var ruleProduction = ZeroOrOne("#RuleProduction", AnnotationProduct.Transitive,
                OneOf("ProductionSelection", AnnotationProduct.Transitive,
                    _transitiveSelector,
                    _noProductSelector
                )
            );

            _ruleName = Token("RuleName", AnnotationProduct.Annotation, TokenNames.Identifier);
            
            var rule = Sequence("Rule", AnnotationProduct.Annotation,
                    ruleProduction,
                    _ruleName,
                    Token(TokenNames.Assignment),
                    ruleDefinition,
                    Token(TokenNames.EndStatement));

            Root = ZeroOrMore("#Root", AnnotationProduct.Transitive, rule);
        }

        // xxx left off here
        public RuleTable<char> CompileFile(string path) =>
            Compile(File.ReadAllText(path));
        

        public RuleTable<char> Compile(string text)
        {
            var (tokens, ruleNodes) = Parse(text);
            var tokenTable = new BasicTokensTable();
            
            foreach (var rule in ruleNodes)
            {
                var production = AnnotationProduct.Annotation;
                var idx = 0;
                
                if (rule.Children[idx].FunctionId == _transitiveSelector.Id)
                {
                    production = AnnotationProduct.Transitive;
                    idx++;
                }
                else if (rule.Children[idx].FunctionId == _noProductSelector.Id)
                {
                    production = AnnotationProduct.None;
                    idx++;
                }

                var name = GetText(text, rule.Children[idx], tokens);
                idx++;

                CompileRuleDefinition(tokenTable, production, name, text, rule.Children[idx], tokens);
            }

            tokenTable.ResolveReferences();

            return tokenTable;
        }

        private RuleBase<char> CompileRuleDefinition(
            BasicTokensTable table, 
            AnnotationProduct product, 
            string name, 
            string text, 
            Annotation ruleDefinition, 
            List<Annotation> tokens)
        {
            if (ruleDefinition.FunctionId == _literal.Id)
            {
                var literalText = GetText(text, ruleDefinition, tokens);
                return table.Literal(literalText.Substring(1, literalText.Length - 2), name, product);
            }
            else if (ruleDefinition.FunctionId == _sequence.Id)
            {
                return CompileSequence(table, product, name, text, ruleDefinition, tokens);
            }
            else if (ruleDefinition.FunctionId == _option.Id)
            {
                return CompileOption(table, product, name, text, ruleDefinition, tokens);
            }
            else if (ruleDefinition.FunctionId == _identifier.Id)
            {
                var referenceName = GetText(text, ruleDefinition, tokens);
                var ruleName = $"ref_to_{referenceName}";

                return table.TryFindRule(ruleName, out RuleBase<char> rule)
                    ? rule
                    : table.RegisterRule(new RuleReference<char>(ruleName, referenceName));
            }
            else if (ruleDefinition.FunctionId == _characterSet.Id)
            {
                var setText = GetText(text, ruleDefinition.Children[0], tokens);
                return table.InSet(name, product, setText.Substring(1, setText.Length - 2).ToArray());
            }
            else if (ruleDefinition.FunctionId == _characterRange.Id)
            {
                var lowerRange = GetText(text, ruleDefinition.Children[0], tokens);
                var upperRange = GetText(text, ruleDefinition.Children[1], tokens);

                return table.TryFindRule(name, out MatchDataRange<char> rule)
                    ? rule
                    : table.RegisterRule(new MatchDataRange<char>(name, lowerRange[1], upperRange[1], product));
            }
            else if (ruleDefinition.FunctionId == _group.Id)
            {
                return CompileRuleDefinition(table, product, name, text, ruleDefinition.Children[0], tokens);
            }
            else if (ruleDefinition.FunctionId == _zeroOrMoreOperator.Id)
            {
                var function = CompileRuleDefinition(table, AnnotationProduct.None, $"{name}(function)", text, ruleDefinition.Children[0], tokens);
                return table.ZeroOrMore(name, product, function);
            }

            throw new NotImplementedException();
        }

        private RuleBase<char> CompileSequence(BasicTokensTable table, AnnotationProduct product, string name, string text, Annotation ruleDefinition, List<Annotation> tokens)
        {
            var sequenceElements = new List<RuleBase<char>>();
            
            for (var  i = 0; i < ruleDefinition.Children.Count; i++)
            {
                var child = ruleDefinition.Children[i];
                sequenceElements.Add(CompileRuleDefinition(table, AnnotationProduct.None, $"{name}[{i}]", text, child, tokens));
            }

            return table.Sequence(name, product, sequenceElements.ToArray());
        }

        private RuleBase<char> CompileOption(BasicTokensTable table, AnnotationProduct product, string name, string text, Annotation ruleDefinition, List<Annotation> tokens)
        {
            var options = new List<RuleBase<char>>();

            for (var i = 0; i < ruleDefinition.Children.Count; i++)
            {
                var child = ruleDefinition.Children[i];
                options.Add(CompileRuleDefinition(table, AnnotationProduct.None, $"{name}[{i}]", text, child, tokens));
            }

            return table.OneOf(name, product, options.ToArray());
        }

        private string GetText(string text, Annotation astNode, List<Annotation> tokens)
        {
            var start = tokens[astNode.Start].Start;
            var length = 0;
            for (var i = 0; i < astNode.Length; i++)
            {
                length += tokens[astNode.Start + i].Length;
            }
            return text.Substring(start, length);
        }

        public ParseResult Parse(List<Annotation> tokens)
        {
            return Root.Parse(tokens.Select(t => t.FunctionId).ToArray(), 0);
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

