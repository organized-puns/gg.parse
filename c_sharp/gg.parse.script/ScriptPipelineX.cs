using gg.parse.compiler;
using gg.parse.rulefunctions;
using gg.parse.rulefunctions.datafunctions;

using static gg.core.util.Assertions;

namespace gg.parse.ebnf
{
    public static class ScriptPipelineX
    {
        public static PipelineSessionX<char> RunTokenPipeline(string tokenizerDefinition, PipelineLog? logger = null)
        {
            RequiresNotNullOrEmpty(tokenizerDefinition);

            var session = InitializeSession<char>(tokenizerDefinition, logger);
            
            session.Compiler = CreateTokenizerCompiler(session.Parser!);

            return RunPipeline(session);
        }

        public static PipelineSessionX<int> RunGrammarPipeline(string grammarDefinition, PipelineSessionX<char> tokenSession)
        {
            RequiresNotNullOrEmpty(grammarDefinition);
            RequiresNotNull(tokenSession);
            RequiresNotNull(tokenSession.RuleGraph!);

            var session = InitializeSession<int>(grammarDefinition, tokenSession.LogHandler);

            session.RuleGraph = RegisterTokens(tokenSession.RuleGraph!, session.RuleGraph!);
            session.Compiler = CreateParserCompiler(session.Parser!);

            return RunPipeline(session);
        }

        public static PipelineSessionX<T> RunPipeline<T>(PipelineSessionX<T> session)
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

        public static PipelineSessionX<T> InitializeSession<T>(string script, PipelineLog? logger = null)
            where T : IComparable<T>
        {
            var tokenizer = new EbnfTokenizer();
            var pipelineLogger = logger ?? new PipelineLog();
            var parser = new EbnfTokenParser(tokenizer)
            {
                FailOnWarning = pipelineLogger.FailOnWarning
            };

            pipelineLogger.FindAstRule = id => parser.FindRule(id);

            var tokenizerSession = new PipelineSessionX<T>()
            {
                Tokenizer = tokenizer,
                Parser = parser,
                LogHandler = pipelineLogger,
                Text = script,
                RuleGraph = new RuleGraph<T>()
            };

            return tokenizerSession;
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


        private static (List<Annotation> tokens, List<Annotation> astNodes) ParseText<T>(PipelineSessionX<T> session)
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

        private static RuleGraph<T> MergeIncludes<T>(PipelineSessionX<T> session)
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

                       var includeSession = RunPipeline(new PipelineSessionX<T>()
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
