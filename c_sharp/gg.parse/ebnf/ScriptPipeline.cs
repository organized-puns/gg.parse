using System.Text;
using System.Text.RegularExpressions;

using gg.parse.compiler;
using gg.parse.rulefunctions;
using gg.parse.rulefunctions.datafunctions;

using static gg.core.util.Assertions;

namespace gg.parse.ebnf
{
    public class PipelineSession<T> where T : IComparable<T>
    {
        // -- config -------------------------------------------------
        public string? WorkingFile { get; set; }

        public string? Text { get; set; }

        public string[] IncludePaths { get; set; } = [];

        public Dictionary<string, RuleGraph<T>?> IncludedFiles { get; set; } = [];

        // -- services -----------------------------------------------
        public EbnfTokenizer? Tokenizer { get; set; }

        public EbnfTokenParser? Parser { get; set; }

        public PipelineLog? LogHandler { get; set; }

        public RuleCompiler<T>? Compiler { get; set; }

        // -- output -------------------------------------------------
        
        public RuleGraph<T>? RuleGraph { get; set; }

        public List<Annotation>? Tokens { get; set; }

        public List<Annotation>? AstNodes { get; set; }
    }

    public class ScriptPipeline
    {
        // replace large parameter lists with this...
        

        private RuleGraph<char> _ebnfTokenizer;
        private RuleGraph<int>? _ebnfParser;

        public RuleGraph<char> EbnfTokenizer => _ebnfTokenizer;

        public RuleGraph<int>? EbnfGrammarParser => _ebnfParser;

        /// <summary>
        /// Handler which will receive all logs (warnings/info/debug) after parsing is complete.
        /// Can be null
        /// </summary>
        public PipelineLog? LogHandler { get; init; }

        public ScriptPipeline()
        {
        }

        /// <summary>
        /// Deprecated
        /// </summary>
        /// 
        /// <param name="tokenizerDefinition"></param>
        /// <param name="grammarDefinition"></param>
        /// <param name="logger"></param>
        [Obsolete]
        public ScriptPipeline(string tokenizerDefinition, string? grammarDefinition, PipelineLog? logger = null)
        {
            var tokenizer = new EbnfTokenizer();

            _ebnfTokenizer = CreateTokenizerFromEbnfFile(tokenizerDefinition, tokenizer, [], logHandler: logger);
            
            if (!string.IsNullOrEmpty(grammarDefinition))
            {
                _ebnfParser = CreateParserFromEbnfFile(grammarDefinition, tokenizer, _ebnfTokenizer, logger);
            }
        }

        public static PipelineSession<char> RunTokenPipeline(string tokenizerDefinition, PipelineLog? logger = null)
        {
            RequiresNotNullOrEmpty(tokenizerDefinition);

            var session = InitializeSession<char>(tokenizerDefinition, logger);
            
            session.Compiler = CreateTokenizerCompiler(session.Parser!);

            return RunPipeline(session);
        }

        public static PipelineSession<int> RunGrammarPipeline(string grammarDefinition, PipelineSession<char> tokenSession)
        {
            RequiresNotNullOrEmpty(grammarDefinition);
            RequiresNotNull(tokenSession);
            RequiresNotNull(tokenSession.RuleGraph!);

            var session = InitializeSession<int>(grammarDefinition, tokenSession.LogHandler);

            session.RuleGraph = RegisterTokens(tokenSession.RuleGraph!, session.RuleGraph!);
            session.Compiler = CreateParserCompiler(session.Parser!);

            return RunPipeline(session);
        }

        public static PipelineSession<T> RunPipeline<T>(PipelineSession<T> session)
            where T : IComparable<T>
        {
            RequiresNotNull(session);
            RequiresNotNull(session.LogHandler!);
            RequiresNotNull(session.Compiler!);
            RequiresNotNullOrEmpty(session.Text!);

            (session.Tokens, session.AstNodes) = ParseText(session);

            // create a rulegraph from all the included files
            session.RuleGraph = MergeIncludes(session);

            // send logs created by parsing to handler.out in a curated format
            session.LogHandler!.ProcessAstLogs(session.Text!, session.Tokens, session.AstNodes);
            
            // remove logs from the annotations
            session.AstNodes = session.AstNodes.Filter(a => session.Parser.FindRule(a.RuleId) is not LogRule<int>);

            // combine the rule graph from the includes with the rulegraph with the one based on the current
            // parse results
            var compileSession = new CompileSession<T>(session.Text, session.Tokens, session.AstNodes);
            session.RuleGraph = session.Compiler!.Compile(compileSession, session.RuleGraph);
            
            return session;
        }

        public static PipelineSession<T> InitializeSession<T>(string script, PipelineLog? logger = null)
            where T : IComparable<T>
        {
            var tokenizer = new EbnfTokenizer();
            var pipelineLogger = logger ?? new PipelineLog();
            var parser = new EbnfTokenParser(tokenizer)
            {
                FailOnWarning = pipelineLogger.FailOnWarning
            };

            pipelineLogger.FindAstRule = id => parser.FindRule(id);

            var tokenizerSession = new PipelineSession<T>()
            {
                Tokenizer = tokenizer,
                Parser = parser,
                LogHandler = pipelineLogger,
                Text = script,
                RuleGraph = new RuleGraph<T>()
            };

            return tokenizerSession;
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
            tokens = _ebnfTokenizer != null && _ebnfTokenizer.Root != null && _ebnfParser != null  && _ebnfParser.Root != null
                    ? _ebnfTokenizer!.Root!.Parse(text.ToArray(), 0)
                    : ParseResult.Failure;

            // found match implies all tokens were accounted for. since invalid
            // tokens are matched against 'errors', we will need to check there are no 
            // error tokens in the matches
            if (tokens.FoundMatch)
            {
                if (tokens.Annotations != null && tokens.Annotations.Count > 0)
                {
                    astTree = _ebnfParser!.Root!.Parse(tokens.Annotations.Select(t => t.RuleId).ToArray(), 0);
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
            var function = FindParserRule(node.RuleId);

            for (var i = 0; i < indentCount; i++)
            {
                builder.Append(indentStr);
            }

            var nodeText = Regex.Escape(node.GetText(text, tokens));

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

        [Obsolete]
        public static RuleGraph<char> CreateTokenizerFromEbnfFile(
            string tokenizerText,
            EbnfTokenizer tokenizer,
            Dictionary<string, RuleGraph<char>> cache,
            string[]? paths = null,
            PipelineLog? logHandler = null)
        {
            List<Annotation> tokenizerTokens;
            List<Annotation> tokenizerAstNodes;

            var tokenizerParser = new EbnfTokenParser(tokenizer)
            {
                FailOnWarning = logHandler != null && logHandler.FailOnWarning
            };

            if (logHandler != null)
            {
                logHandler.FindAstRule = id => tokenizerParser.FindRule(id);
            }

            try
            {
                (tokenizerTokens, tokenizerAstNodes) = tokenizerParser.Parse(tokenizerText);
            }
            // xxx not necessary to wrap these ?
            catch (TokenizeException te)
            {             
                throw new EbnfException("Exception in token. Failed to build tokenizer. See inner exception for details.", te);
            }
            catch (ParseException pe) 
            {
                throw new EbnfException("Exception in grammar. Failed to build tokenizer. See inner exception for details.", pe);
            }

            var includedSources = ProcessIncludedFiles(
                                    tokenizerText, 
                                    tokenizer, 
                                    tokenizerParser.Include.Id, 
                                    tokenizerTokens, 
                                    tokenizerAstNodes, 
                                    cache,
                                    paths,
                                    (text, includePaths) => CreateTokenizerFromEbnfFile(text, tokenizer, cache, includePaths, logHandler));

            logHandler?.ProcessAstLogs(tokenizerText, tokenizerTokens, tokenizerAstNodes);

            // remove logs from the annotations
            var filteredNodes = tokenizerAstNodes.Filter(a => tokenizerParser.FindRule(a.RuleId) is not LogRule<int>);

            var tokenContext = new CompileSession<char>(tokenizerText, tokenizerTokens, filteredNodes);

            try
            {
                return new RuleCompiler<char>()
                        .WithAnnotationProductMapping(tokenizerParser.CreateAnnotationProductMapping())
                        .RegisterTokenizerCompilerFunctions(tokenizerParser)
                        .Compile(tokenContext, includedSources);
            }
            catch (NoCompilationFunctionException nce)
            {
                var rule = tokenizerParser.FindRule(nce.RuleId);

                // xxx not necessary to wrap these ?
                if (rule == null)
                {
                    throw new EbnfException($"Compiler is missing a function for rule with id={nce.RuleId}, and no corresponding rule was found in the parser. Please check the compiler configuration.");
                }
                else
                {
                    throw new EbnfException($"Compiler is missing a function for rule with id={nce.RuleId}({rule.Name}).", nce);
                }
            }
        }

        public static RuleCompiler<char> CreateTokenizerCompiler(EbnfTokenParser parser)
        {
            try
            {
                return new RuleCompiler<char>()
                        .WithAnnotationProductMapping(parser.CreateAnnotationProductMapping())
                        .RegisterTokenizerCompilerFunctions(parser);
            }
            catch (NoCompilationFunctionException nce)
            {
                var rule = parser.FindRule(nce.RuleId);

                // xxx not necessary to wrap these ?
                if (rule == null)
                {
                    throw new EbnfException($"Compiler is missing a function for rule with id={nce.RuleId}, and no corresponding rule was found in the parser. Please check the compiler configuration.");
                }
                else
                {
                    throw new EbnfException($"Compiler is missing a function for rule with id={nce.RuleId}({rule.Name}).", nce);
                }
            }
        }

        public static RuleCompiler<int> CreateParserCompiler(EbnfTokenParser parser)
        {
            try
            {
                return new RuleCompiler<int>()
                    .WithAnnotationProductMapping(parser.CreateAnnotationProductMapping())
                    .RegisterGrammarCompilerFunctions(parser);
            }
            catch (NoCompilationFunctionException nce)
            {
                var rule = parser.FindRule(nce.RuleId);

                // xxx not necessary to wrap these ?
                if (rule == null)
                {
                    throw new EbnfException($"Compiler is missing a function for rule with id={nce.RuleId}, and no corresponding rule was found in the parser. Please check the compiler configuration.");
                }
                else
                {
                    throw new EbnfException($"Compiler is missing a function for rule with id={nce.RuleId}({rule.Name}).", nce);
                }
            }
        }


        private static (List<Annotation> tokens, List<Annotation> astNodes) ParseText<T>(PipelineSession<T> session)
            where T : IComparable<T>
        {
            RequiresNotNull(session.Tokenizer!);
            RequiresNotNull(session.Parser!);
            RequiresNotNullOrEmpty(session.Text!);

            try
            {
                return session.Parser!.Parse(session.Text!);
            }
            // xxx not necessary to wrap these ?
            catch (TokenizeException te)
            {
                throw new EbnfException("Exception in token. Failed to build tokenizer. See inner exception for details.", te);
            }
            catch (ParseException pe)
            {
                throw new EbnfException("Exception in grammar. Failed to build tokenizer. See inner exception for details.", pe);
            } 
        }

        private static RuleGraph<T> MergeIncludes<T>(PipelineSession<T> session)
            where T : IComparable<T>
        {
            RequiresNotNull(session);
            RequiresNotNullOrEmpty(session.Text!);
            RequiresNotNull(session.Parser!);
            RequiresNotNull(session.AstNodes!);
            RequiresNotNull(session.Tokens!);
            RequiresNotNull(session.RuleGraph!);

            var result = session.RuleGraph;

            for (var i = 0; i < session.AstNodes!.Count;)
            {
                var statement = session.AstNodes[i];

                if (statement.RuleId == session.Parser!.Include.Id)
                {
                    Requires(statement.Children != null && statement.Children.Count > 0);

                    var fileName = 
                        ResolveFile(
                            statement.Children![0].GetText(session.Text!, session.Tokens!), session.IncludePaths
                        );

                    if (session.IncludedFiles.ContainsKey(fileName))
                    {
                        if (session.IncludedFiles[fileName] == null)
                        {
                            // xxx do this more insightful, doesn't tell where the error occured 
                            throw new InvalidProgramException($"Circular file include detected: {fileName}.");
                        }
                        // cache should already be merged with the result
                        // xxx so there is no need to store the graph in the cache, make the cache a set ?
                    }
                    else
                    {
                       // make sure the cache contains an entry in order to detect circular dependencies
                       session.IncludedFiles[fileName] = null;

                       var includeSession = RunPipeline(new PipelineSession<T>()
                       {
                           Text = File.ReadAllText(fileName),
                           IncludePaths = [Path.GetDirectoryName(fileName), AppContext.BaseDirectory],
                           Tokenizer = session.Tokenizer,
                           Parser = session.Parser,
                           LogHandler = session.LogHandler,
                           IncludedFiles = session.IncludedFiles,
                       });

                        session.IncludedFiles[fileName] = includeSession.RuleGraph;
                        result.Merge(includeSession.RuleGraph);
                    }

                    session.AstNodes.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            return result;
        }

        /// <summary>
        /// Given grammar text, turns it into a rulegraph(int) for parsing purposes
        /// and turning other text into asttrees.
        /// </summary>
        /// <param name="grammarText"></param>
        /// <param name="tokenizer"></param>
        /// <param name="tokenSource"></param>
        /// <returns></returns>
        public static RuleGraph<int> CreateParserFromEbnfFile(
            string grammarText,
            EbnfTokenizer tokenizer,
            RuleGraph<char> tokenSource,
            PipelineLog? logHandler = null) =>

            CreateParserFromEbnfFile(
                grammarText, 
                tokenizer, 
                RegisterTokens(tokenSource, new RuleGraph<int>()), 
                [],
                logHandler: logHandler
            );

        private static RuleGraph<int> CreateParserFromEbnfFile(
            string grammarText,
            EbnfTokenizer tokenizer,
            RuleGraph<int> target,
            Dictionary<string, RuleGraph<int>> cache,
            string[]? paths = null,
            PipelineLog? logHandler = null)
        {
            List<Annotation>? grammarTokens = null;
            List<Annotation>? grammarAstNodes = null;

            var grammarParser = new EbnfTokenParser(tokenizer)
            {
                FailOnWarning = logHandler != null && logHandler.FailOnWarning
            };

            try
            {
                (grammarTokens, grammarAstNodes) = grammarParser.Parse(grammarText);
            }
            // xxx not necessary to wrap these ?
            catch (Exception e) when (e is ParseException || e is TokenizeException)
            {
                // add information where this went wrong
                throw new EbnfException("Failed to build grammar parser. See inner exception for details.", e);
            }

            // merge with the rules coming in from other included files
            target.Merge(
                ProcessIncludedFiles(
                    grammarText,
                    tokenizer,
                    grammarParser.Include.Id,
                    grammarTokens,
                    grammarAstNodes,
                    cache,
                    paths,
                    (text, includePaths) => 
                        CreateParserFromEbnfFile(text, tokenizer, target, cache, includePaths, logHandler)
                )
            );

            // redirect rule lookup if a logger is set
            if (logHandler != null)
            {
                logHandler.FindAstRule = id => grammarParser.FindRule(id);
                logHandler.ProcessAstLogs(grammarText, grammarTokens, grammarAstNodes);
            }

            // remove logs from the annotations
            var filteredNodes = grammarAstNodes.Filter(a => grammarParser.FindRule(a.RuleId) is not LogRule<int>);

            var grammarcontext = new CompileSession<int>(grammarText, grammarTokens, filteredNodes);

            return new RuleCompiler<int>()
                    .WithAnnotationProductMapping(grammarParser.CreateAnnotationProductMapping()) 
                    .RegisterGrammarCompilerFunctions(grammarParser)
                    .Compile(grammarcontext, target);
        }

        private static RuleGraph<int> RegisterTokens(RuleGraph<char> tokenSource, RuleGraph<int> target)
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

        /// <summary>
        /// Run pre-processor step to include other ebnf files
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="tokenizer"></param>
        /// <param name="includeId"></param>
        /// <param name="tokens"></param>
        /// <param name="astTree"></param>
        /// <returns></returns>
        private static RuleGraph<T> ProcessIncludedFiles<T>(
            string inputText,
            EbnfTokenizer tokenizer,
            int includeId,
            List<Annotation> tokens,
            List<Annotation> astTree,
            Dictionary<string, RuleGraph<T>> cache,
            string[]? paths,
            Func<string, string[], RuleGraph<T>> buildIncludedGraph
            ) where T : IComparable<T>
        {
            var result = new RuleGraph<T>();

            for (var i = 0; i < astTree.Count;)
            {
                var statement = astTree[i];

                if (statement.RuleId == includeId)
                {
                    var fileName = ResolveFile(statement.Children[0].GetText(inputText, tokens), paths);
                    
                    if (cache.ContainsKey(fileName))
                    {
                        if (cache[fileName] == null)
                        {
                            // xxx do this more insightful, doesn't tell where the error occured 
                            throw new InvalidProgramException($"Circular file include detected: {fileName}.");
                        }
                        // cache should already be merged with the result
                        // xxx so there is no need to store the graph in the cache, make the cache a set ?
                    }
                    else
                    {
                        // make sure the cache contains an entry in order to detect circular dependencies
                        cache[fileName] = null;

                        var includeGraph = buildIncludedGraph(File.ReadAllText(fileName), [Path.GetDirectoryName(fileName), AppContext.BaseDirectory]);

                        cache[fileName] = includeGraph;

                        result.Merge(includeGraph);
                    }

                    astTree.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            return result;
        }

        private static string ResolveFile(string fileName, string[] paths = null)
        {
            if (fileName[0] == '"' || fileName[0] == '\'')
            {
                fileName = fileName.Substring(1, fileName.Length - 2);
            }

            if (File.Exists(fileName))
            {
                return Path.GetFullPath(fileName);
            }
            else if (paths != null && paths.Length > 0)
            {
                foreach (var path in paths)
                {
                    var separator = path[^1] == '/' || path[^1] == '\\'
                        ? ""
                        : Path.DirectorySeparatorChar.ToString();

                    if (File.Exists(path + separator + fileName))
                    {
                        return Path.GetFullPath(path + separator + fileName);
                    }
                }
            }
            
            throw new ArgumentException($"Trying to include {fileName} but doesn't seem to exist.");
        }
    }
}
