
using gg.parse.rules;

using gg.parse.script.parser;
using gg.parse.script.compiler;

using static gg.parse.Assertions;

namespace gg.parse.script.pipeline
{
    public static class ScriptPipeline
    {
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

            (session.Tokens, session.AstNodes) = ParseSessionText(session);

            // merge the sessions' rulegraph with all the included files
            MergeIncludes(session);

            // send logs created by parsing to handler.out in a curated format
            session.LogHandler!.ProcessAstAnnotations(session.Text!, session.Tokens, session.AstNodes);
            
            // remove logs from the annotations
            session.AstNodes = session.AstNodes.Filter(a => a.Rule is not LogRule<int>);

            // combine the rule graph from the includes with the rulegraph with the one based on the current
            // parse results
            var compileSession = new CompileSession(session.Text, session.Tokens, session.AstNodes);
            session.RuleGraph = session.Compiler!.Compile(compileSession, session.RuleGraph);
            
            return session;
        }

        public static PipelineSession<T> InitializeSession<T>(string script, PipelineLog? logger = null)
            where T : IComparable<T>
        {
            var tokenizer = new ScriptTokenizer();
            var pipelineLogger = logger ?? new PipelineLog();
            var parser = new ScriptParser(tokenizer)
            {
                FailOnWarning = pipelineLogger.FailOnWarning
            };

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

        public static RuleCompiler<char> CreateTokenizerCompiler(ScriptParser parser)
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
                    throw new ScriptPipelineException($"Compiler is missing a function for rule with id={nce.RuleId}, and no corresponding rule was found in the parser. Please check the compiler configuration.");
                }
                else
                {
                    throw new ScriptPipelineException($"Compiler is missing a function for rule with id={nce.RuleId}({rule.Name}).", nce);
                }
            }
        }

        public static RuleCompiler<int> CreateParserCompiler(ScriptParser parser)
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
                    throw new ScriptPipelineException($"Compiler is missing a function for rule with id={nce.RuleId}, and no corresponding rule was found in the parser. Please check the compiler configuration.");
                }
                else
                {
                    throw new ScriptPipelineException($"Compiler is missing a function for rule with id={nce.RuleId}({rule.Name}).", nce);
                }
            }
        }


        private static (List<Annotation> tokens, List<Annotation> astNodes) ParseSessionText<T>(PipelineSession<T> session)
            where T : IComparable<T>
        {
            RequiresNotNull(session.Tokenizer!);
            RequiresNotNull(session.Parser!);
            RequiresNotNull(session.LogHandler!);
            RequiresNotNullOrEmpty(session.Text!);

            try
            {
                return session.Parser!.Parse(session.Text!);
            }
            catch (TokenizeException te)
            {
                session.LogHandler!.ProcessException(te);
                throw new ScriptPipelineException("Exception in tokens while tokenizing text.", te);
            }
            catch (ParseException pe)
            {
                session.LogHandler!.ProcessException(pe);
                throw new ScriptPipelineException("Exception in grammar while parsing tokens.", pe);
            } 
        }

        private static void MergeIncludes<T>(PipelineSession<T> session)
            where T : IComparable<T>
        {
            RequiresNotNull(session);
            RequiresNotNullOrEmpty(session.Text!);
            RequiresNotNull(session.Parser!);
            RequiresNotNull(session.AstNodes!);
            RequiresNotNull(session.Tokens!);
            RequiresNotNull(session.RuleGraph!);

            for (var i = 0; i < session.AstNodes!.Count;)
            {
                var statement = session.AstNodes[i];

                if (statement.Rule == session.Parser!.Include)
                {
                    Requires(statement.Children != null && statement.Children.Count > 0);

                    var filename = statement.Children![0].GetText(session.Text!, session.Tokens!);
                    var filePath = ResolveFile(filename, session.IncludePaths);

                    if (session.IncludedFiles.ContainsKey(filePath))
                    {
                        if (session.IncludedFiles[filePath] == null)
                        {
                            // xxx do this more insightful, doesn't tell where the error occured 
                            throw new ScriptPipelineException($"Circular include detected {filePath}.");
                        }
                        // cache should already be merged with the result
                        
                    }
                    else
                    {
                        // make sure the cache contains an entry in order to detect circular dependencies
                        session.IncludedFiles[filePath] = null;

                        session.LogHandler!.Log(LogLevel.Debug, $"including: {filename}({filePath}).");

                        var includeSession = RunPipeline(new PipelineSession<T>()
                        {
                           Text = File.ReadAllText(filePath),
                           IncludePaths = [Path.GetDirectoryName(filePath), AppContext.BaseDirectory],
                           Tokenizer = session.Tokenizer,
                           Parser = session.Parser,
                           LogHandler = session.LogHandler,
                           IncludedFiles = session.IncludedFiles,
                           Compiler = session.Compiler,
                           RuleGraph = session.RuleGraph 
                        });

                        session.IncludedFiles[filePath] = includeSession.RuleGraph;
                    }

                    session.AstNodes.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
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
            
            throw new ScriptPipelineException($"Trying to include {fileName} but doesn't seem to exist.");
        }
    }
}
