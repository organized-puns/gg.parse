
using gg.parse.rules;
using gg.parse.script.compiler;
using gg.parse.script.parser;
using System.ComponentModel.DataAnnotations;
using static gg.parse.Assertions;
using static gg.parse.script.compiler.CompilerFunctions;
using static System.Net.Mime.MediaTypeNames;

namespace gg.parse.script.pipeline
{
    public static class ScriptPipeline
    {
        public static PipelineSession<char> RunTokenPipeline(string tokenizerDefinition, PipelineLogger? logger = null)
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

            (session.Tokens, session.SyntaxTree) = ParseSessionText(session);

            // merge the sessions' rulegraph with all the included files
            MergeIncludes(session);

            // send logs created by parsing to handler.out in a curated format
            session.LogHandler!.ProcessAstAnnotations(session.Text!, session.Tokens, session.SyntaxTree);
            
            // remove logs from the annotations
            session.SyntaxTree = session.SyntaxTree.Filter(a => a.Rule is not LogRule<int>);

            // combine the rule graph from the includes with the rulegraph with the one based on the current
            // parse results
            try
            {
                session.RuleGraph =
                    session
                        .Compiler!
                        .Compile(
                            session.Text!,
                            session.Tokens,
                            session.SyntaxTree,
                            session.RuleGraph
                );
            }
            catch (AggregateException ae)
            {
                if (session.LogHandler != null)
                {
                    session.LogHandler.ProcessExceptions(
                        ae.InnerExceptions,
                        session.Text!,
                        session.Tokens
                    );
                }

                throw new ScriptPipelineException("Compliation exception(s) raised", ae);
            }
            
            return session;
        }

        public static PipelineSession<T> InitializeSession<T>(string script, PipelineLogger? logger = null)
            where T : IComparable<T>
        {
            var tokenizer = new ScriptTokenizer();
            var pipelineLogger = logger ?? new PipelineLogger();
            var parser = new ScriptParser(tokenizer);

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

        public static RuleCompiler CreateTokenizerCompiler(ScriptParser parser)
        {
            try
            {
                return new RuleCompiler(CreateRuleOutputMapping(parser))
                        .RegisterTokenizerCompilerFunctions(parser);
            }
            catch (CompilationException ce)
            {
                var rule = parser.FindRule(ce.RuleId);

                // xxx not necessary to wrap these ?
                if (rule == null)
                {
                    throw new ScriptPipelineException($"Compiler is missing a function for rule with id={ce.RuleId},"
                        +" and no corresponding rule was found in the parser. Please check the compiler configuration.");
                }
                else
                {
                    throw new ScriptPipelineException($"Compiler is missing a function for rule with" 
                        + "i d={ce.RuleId}({rule.Name}).", ce);
                }
            }
        }

        public static (int functionId, IRule.Output product)[] CreateRuleOutputMapping(ScriptParser parser) =>
        [
            (parser.MatchTransitiveSelector.Id, IRule.Output.Children),
            (parser.MatchNoProductSelector.Id, IRule.Output.Void),
        ];

        public static RuleCompiler CreateParserCompiler(ScriptParser parser)
        {
            try
            {
                return new RuleCompiler(CreateRuleOutputMapping(parser))
                    .RegisterGrammarCompilerFunctions(parser);
            }
            catch (CompilationException ce)
            {
                var rule = parser.FindRule(ce.RuleId);

                // xxx not necessary to wrap these ?
                if (rule == null)
                {
                    throw new ScriptPipelineException($"Compiler is missing a function for rule with id={ce.RuleId}, and no corresponding rule was found in the parser. Please check the compiler configuration.");
                }
                else
                {
                    throw new ScriptPipelineException($"Compiler is missing a function for rule with id={ce.RuleId}({rule.Name}).", ce);
                }
            }
        }


        /// <summary>
        /// Registers all the compiler functions needed to compile a tokenizer script.
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static RuleCompiler RegisterTokenizerCompilerFunctions(this RuleCompiler compiler, ScriptParser parser)
        {
            return compiler
                    .RegisterFunction(parser.MatchAnyToken, CompileAny<char>)
                    .RegisterFunction(parser.MatchCharacterRange, CompileCharacterRange)
                    .RegisterFunction(parser.MatchCharacterSet, CompileCharacterSet)
                    .RegisterFunction(parser.MatchLog, CompileLog<char>)
                    .RegisterFunction(parser.MatchGroup, CompileGroup<char>)
                    .RegisterFunction(parser.MatchIdentifier, CompileIdentifier<char>)
                    .RegisterFunction(parser.MatchLiteral, CompileLiteral)
                    .RegisterFunction(parser.MatchNotOperator, CompileNot<char>)
                    .RegisterFunction(parser.IfMatchOperator, CompileTryMatch<char>)
                    .RegisterFunction(parser.MatchOneOrMoreOperator, CompileOneOrMore<char>)
                    .RegisterFunction(parser.MatchOption, CompileOption<char>)
                    .RegisterFunction(parser.MatchSequence, CompileSequence<char>)
                    .RegisterFunction(parser.MatchZeroOrMoreOperator, CompileZeroOrMore<char>)
                    .RegisterFunction(parser.MatchZeroOrOneOperator, CompileZeroOrOne<char>)
                    .RegisterFunction(parser.MatchFindOperator, CompileFind<char>)
                    .RegisterFunction(parser.MatchSkipOperator, CompileSkip<char>);
        }

        /// <summary>
        /// Registers all the compiler functions needed to compile a grammar script.
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static RuleCompiler RegisterGrammarCompilerFunctions(this RuleCompiler compiler, ScriptParser parser)
        {
            return compiler
                    .RegisterFunction(parser.MatchAnyToken, CompileAny<int>)
                    .RegisterFunction(parser.MatchGroup, CompileGroup<int>)
                    .RegisterFunction(parser.MatchIdentifier, CompileIdentifier<int>)
                    .RegisterFunction(parser.MatchNotOperator, CompileNot<int>)
                    .RegisterFunction(parser.IfMatchOperator, CompileTryMatch<int>)
                    .RegisterFunction(parser.MatchOneOrMoreOperator, CompileOneOrMore<int>)
                    .RegisterFunction(parser.MatchOption, CompileOption<int>)
                    .RegisterFunction(parser.MatchSequence, CompileSequence<int>)
                    .RegisterFunction(parser.MatchZeroOrMoreOperator, CompileZeroOrMore<int>)
                    .RegisterFunction(parser.MatchZeroOrOneOperator, CompileZeroOrOne<int>)
                    .RegisterFunction(parser.MatchEval, CompileEvaluation<int>)
                    .RegisterFunction(parser.MatchLog, CompileLog<int>)
                    .RegisterFunction(parser.MatchFindOperator, CompileFind<int>)
                    .RegisterFunction(parser.MatchSkipOperator, CompileSkip<int>);
        }

        private static (List<Annotation>? tokens, List<Annotation>? astNodes) ParseSessionText<T>(PipelineSession<T> session)
            where T : IComparable<T>
        {
            RequiresNotNull(session);
            RequiresNotNull(session.Tokenizer!);
            RequiresNotNull(session.Parser!);
            RequiresNotNull(session.LogHandler!);
            RequiresNotNullOrEmpty(session.Text!);

            try
            {
                return session!.Parser!.Parse(session.Text!, session.LogHandler!.FailOnWarning);
            }
            catch (ScriptException pe)
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
            RequiresNotNull(session.SyntaxTree!);
            RequiresNotNull(session.Tokens!);
            RequiresNotNull(session.RuleGraph!);

            for (var i = 0; i < session.SyntaxTree!.Count;)
            {
                var statement = session.SyntaxTree[i];

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

                    session.SyntaxTree.RemoveAt(i);
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
            foreach (var tokenFunctionName in tokenSource.RuleNames)
            {
                var tokenFunction = tokenSource.FindRule(tokenFunctionName);

                if (tokenFunction.Production == IRule.Output.Self)
                {
                    target.RegisterRule(new MatchSingleData<int>($"{tokenFunctionName}", tokenFunction.Id, IRule.Output.Self));
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
