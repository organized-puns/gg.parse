using gg.core.util;
using gg.parse.compiler;
using gg.parse.rulefunctions;
using System.Text;
using System.Text.RegularExpressions;

namespace gg.parse.ebnf
{
    public class EbnfParser
    {
        private RuleTable<char> _ebnfTokenizer;
        private RuleTable<int>? _ebnfParser;

        public RuleTable<char> EbnfTokenizer => _ebnfTokenizer;

        public RuleTable<int>? EbnfGrammarParser => _ebnfParser;

        public EbnfParser(string tokenizerDefinition, string? grammarDefinition)
        {
            var tokenizer = new EbnfTokenizer();
            _ebnfTokenizer = CreateTokenizerFromEbnfFile(tokenizerDefinition, tokenizer);
            
            if (!string.IsNullOrEmpty(grammarDefinition))
            {
                _ebnfParser = CreateParserFromEbnfFile(grammarDefinition, tokenizer, _ebnfTokenizer);
            }
        }

        public RuleBase<int>? FindParserRule(string name) => _ebnfParser.FindRule(name);

        public RuleBase<int>? FindParserRule(int id) => _ebnfParser.FindRule(id);

        public bool TryMatch(string text) => TryMatch(text, out var result);

        public bool TryMatch(string text, out Range? result)
        {
            if (TryBuildAstTree(text, out var tokens, out var astTree))
            {
                result = new(0, tokens.MatchedLength);
                return true;
            }

            result = null;
            return false;
        }

        public ParseResult Parse(string text) =>
            TryBuildAstTree(text, out var tokens, out var astTree)
                ? astTree
                : ParseResult.Failure;

                
        public bool TryBuildAstTree(string text, out ParseResult tokens, out ParseResult astTree)
        {
            astTree = ParseResult.Failure;
            tokens = _ebnfParser != null  && _ebnfParser.Root != null
                    ? _ebnfTokenizer!.Root!.Parse(text.ToArray(), 0)
                    : ParseResult.Failure;

            if (tokens.FoundMatch)
            {
                if (tokens.Annotations != null && tokens.Annotations.Count > 0)
                {
                    astTree = tokens.FoundMatch
                            ? _ebnfParser!.Root!.Parse(tokens.Annotations.Select(t => t.FunctionId).ToArray(), 0)
                            : ParseResult.Failure;
                }
                else
                {
                    // empty file
                    astTree = new ParseResult(true, 0, []);
                }

                return astTree.FoundMatch;
            }
            
            return false;            
        }

        public string Dump(string text, ParseResult tokens, ParseResult astTree, string indentStr = "   ")
        {
            var builder = new StringBuilder();
            var indent = 0;

            if (astTree.Annotations != null && astTree.Annotations.Count > 0)
            {
                foreach (var astNode in astTree.Annotations)
                {
                    Dump(builder, indent, indentStr, astNode, text, tokens.Annotations);
                }
            }

            return builder.ToString();
        }

        public void Dump(StringBuilder builder, int indentCount, string indentStr, Annotation node, string text, List<Annotation> tokens)
        {
            var function = FindParserRule(node.FunctionId);

            for (var i = 0; i < indentCount; i++)
            {
                builder.Append(indentStr);
            }

            var nodeText = Regex.Escape(GetText(text, node, tokens));

            if (nodeText.Length > 20)
            {
                nodeText = $"{nodeText.Substring(0, 17)}...";
            }

            builder.AppendLine($"[{node.Range.Start},{node.Range.End}]{function.Name}({function.Id}): {nodeText}");

            if (node.Children != null && node.Children.Count > 0)
            {
                foreach(var child in node.Children)
                {
                    Dump(builder, indentCount+1, indentStr, child, text, tokens);
                }
            }
        }



        public static string GetText(string text, Annotation grammarAnnotation, List<Annotation> tokens)
        {
            var range = GetTextRange(grammarAnnotation, tokens);
            return text.Substring(range.Start, range.Length);
        }

        public static string GetText(string text, Annotation grammarAnnotation, ParseResult tokens)
            => GetText(text, grammarAnnotation, tokens.Annotations);
        

        public static ReadOnlySpan<char> GetSpan(string text, Annotation grammarAnnotation, ParseResult tokens)
        {
            var range = GetTextRange(grammarAnnotation, tokens);
            return text.AsSpan(range.Start, range.Length);
        }

        public static Range GetTextRange(Annotation grammarAnnotation, ParseResult tokens) =>
            GetTextRange(grammarAnnotation.Range, tokens.Annotations);

        public static Range GetTextRange(Annotation grammarAnnotation, List<Annotation> tokens) =>
            GetTextRange(grammarAnnotation.Range, tokens);

        public static Range GetTextRange(Range tokenRange, List<Annotation> tokens)
        {
            Contract.RequiresNotNull(tokens);

            var startIndex = tokens[tokenRange.Start].Start;
            var start = startIndex;
            var length = 0;

            for (var i = 0; i < tokenRange.Length; i++)
            {
                // need to take in account possible white space
                var token = tokens[tokenRange.Start + i];
                length += (token.Start - (startIndex + length)) + token.Length;
            }

            return new Range(start, length);
        }


        public static RuleTable<char> CreateTokenizerFromEbnfFile(
            string tokenizerText,
            EbnfTokenizer tokenizer)
        {
            var tokenizerParser = new EbnfTokenParser(tokenizer);

            var tokenizerTokens  = tokenizer.Tokenize(tokenizerText).Annotations;
            var tokenizerAstTree = tokenizerParser.Parse(tokenizerTokens).Annotations;

            var tokenContext = CompilerUtils
                                .CreateContext<char>(tokenizerText, tokenizerTokens, tokenizerAstTree)
                                .SetAnnotationProductMapping(tokenizerParser);

            return new RuleCompiler<char>()
                    .RegisterTokenizerCompilerFunctions(tokenizerParser)
                    .Compile(tokenContext);
        }

        public static RuleTable<int> CreateParserFromEbnfFile(
            string grammarText,
            EbnfTokenizer tokenizer,
            RuleTable<char> tokenSource)
        {
            var grammarParser = new EbnfTokenParser(tokenizer);
            var (grammarTokens, grammarAstNodes) = grammarParser.Parse(grammarText);

            var grammarcontext = CompilerUtils
                                    .CreateContext<int>(grammarText, grammarTokens, grammarAstNodes)
                                    .SetAnnotationProductMapping(grammarParser);

            return new RuleCompiler<int>()
                    .RegisterGrammarCompilerFunctions(grammarParser)
                    .Compile(grammarcontext, RegisterTokens(tokenSource, new RuleTable<int>()));
        }

        private static RuleTable<int> RegisterTokens(RuleTable<char> tokenSource, RuleTable<int> target)
        {
            // register the tokens found in the interpreted ebnf tokenizer with the grammar compiler
            foreach (var tokenFunctionName in tokenSource.FunctionNames)
            {
                var tokenFunction = tokenSource.FindRule(tokenFunctionName);

                if (tokenFunction.Production == AnnotationProduct.Annotation)
                {
                    target.RegisterRule(new MatchSingleData<int>($"{tokenFunctionName}", tokenFunction.Id, AnnotationProduct.Annotation));
                }
            }

            return target;
        }
    }
    
}
