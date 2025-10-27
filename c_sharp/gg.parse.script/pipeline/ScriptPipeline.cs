// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.rules;
using gg.parse.util;
using gg.parse.script.compiler;
using gg.parse.script.parser;

using static gg.parse.util.Assertions;
using static gg.parse.script.compiler.CompilerFunctions;
using System.Diagnostics.CodeAnalysis;

namespace gg.parse.script.pipeline
{
    public static class ScriptPipeline
    {
        public static PipelineSession<char> RunTokenPipeline(
            string tokenizerDefinition, 
            ScriptLogger? logger = null,
            HashSet<string>? includedPaths = null)
        {
            RequiresNotNullOrEmpty(tokenizerDefinition);

            var session = InitializeSession<char>(
                tokenizerDefinition, 
                logger,
                includedPaths == null ? [] : [.. includedPaths!]);
            
            session.Compiler = CreateTokenizerCompiler(session.Parser!);

            return RunPipeline(session);
        }

        public static PipelineSession<int> RunGrammarPipeline(
            string grammarDefinition, 
            PipelineSession<char> tokenSession,
            HashSet<string>? includedPaths = null)
        {
            RequiresNotNullOrEmpty(grammarDefinition);
            RequiresNotNull(tokenSession);
            RequiresNotNull(tokenSession.RuleGraph);

            var session = InitializeSession<int>(
                grammarDefinition, 
                tokenSession.LogHandler, 
                includedPaths == null ? [] : [..includedPaths!]);

            session.RuleGraph!.RegisterTokens(tokenSession.RuleGraph!);
            session.Compiler = CreateParserCompiler(session.Parser!);

            return RunPipeline(session);
        }

        public static PipelineSession<T> RunPipeline<T>(PipelineSession<T> session)
            where T : IComparable<T>
        {
            RequiresNotNull(session);
            RequiresNotNull(session.LogHandler);
            RequiresNotNull(session.Compiler);
            RequiresNotNull(session.RuleGraph);
            RequiresNotNullOrEmpty(session.Text!);

            (session.Tokens, session.SyntaxTree) = ParseSessionText(session);

            RequiresNotNull(session.Tokens);
            RequiresNotNull(session.SyntaxTree);

            // merge the sessions' rulegraph with all the included files
            MergeIncludes(session);

            // send logs created by parsing to handler.out in a curated format
            session.LogHandler!.ProcessSyntaxTree(session.Text!, session.Tokens, session.SyntaxTree);
            
            // remove logs from the annotations
            session.SyntaxTree = session.SyntaxTree.Prune(a => a.Rule is LogRule<int>);

            // combine the rule graph from the includes with the rulegraph with the one based on the current
            // parse results
            try
            {
                // reset the root. Included files may have set a root but the root needs to be the 
                // one of the topmost file included. The compiler will set it for us
                session.RuleGraph.Root = null;

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
                session.LogHandler?.ProcessExceptions(
                        ae.InnerExceptions,
                        session.Text!,
                        session.Tokens
                    );

                throw new ScriptPipelineException("Compliation exception(s) raised", ae);
            }
            
            return session;
        }
        
        public static PipelineSession<T> InitializeSession<T>(
            string script, 
            ScriptLogger? logger = null,
            HashSet<string>? includePaths = null)
            where T : IComparable<T>
        {
            var tokenizer = new ScriptTokenizer();
            var pipelineLogger = logger ?? new ScriptLogger();
            var parser = new ScriptParser(tokenizer);

            var sessionIncludePaths = includePaths ?? [];

            sessionIncludePaths.Add(AppContext.BaseDirectory);

            var session = new PipelineSession<T>()
            {
                Tokenizer = tokenizer,
                Parser = parser,
                LogHandler = pipelineLogger,
                Text = script,
                RuleGraph = new RuleGraph<T>(),
                IncludePaths = sessionIncludePaths
            };

            return session;
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
                if (ce.Rule == null)
                {
                    throw new ScriptPipelineException($"Unknown compiler exception while registering compiler functions for the tokens.", ce);
                }
                else
                {
                    throw new ScriptPipelineException($"Compiler is missing a function for token rule '{ce.Rule.Name}'.", ce);
                }
            }
        }

        public static (int functionId, AnnotationPruning product)[] CreateRuleOutputMapping(ScriptParser parser) =>
        [
            (parser.MatchPruneRootToken.Id, AnnotationPruning.Root),
            (parser.MatchPruneAllToken.Id, AnnotationPruning.All),
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
                if (ce.Rule == null)
                {
                    throw new ScriptPipelineException($"Unknown compiler exception while registering compiler functions for the grammar.", ce);
                }
                else
                {
                    throw new ScriptPipelineException($"Compiler is missing a function for grammar rule '{ce.Rule.Name}'.", ce);
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
                    .RegisterFunction(parser.MatchReference, CompileIdentifier<char>)
                    .RegisterFunction(parser.MatchLiteral, CompileLiteral)
                    .RegisterFunction(parser.MatchNotOperator, CompileNot<char>)
                    .RegisterFunction(parser.IfMatchOperator, CompileTryMatch<char>)
                    .RegisterFunction(parser.MatchOneOrMoreOperator, CompileOneOrMore<char>)
                    .RegisterFunction(parser.MatchOneOf, CompileOption<char>)
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
                    .RegisterFunction(parser.MatchReference, CompileIdentifier<int>)
                    .RegisterFunction(parser.MatchNotOperator, CompileNot<int>)
                    .RegisterFunction(parser.IfMatchOperator, CompileTryMatch<int>)
                    .RegisterFunction(parser.MatchOneOrMoreOperator, CompileOneOrMore<int>)
                    .RegisterFunction(parser.MatchOneOf, CompileOption<int>)
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
            RequiresNotNull(session.Tokenizer);
            RequiresNotNull(session.Parser);
            RequiresNotNull(session.LogHandler);
            RequiresNotNullOrEmpty(session.Text!);

            try
            {
                return session.Parser.Parse(session.Text, failOnWarning: session.LogHandler.FailOnWarning);
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

            // xxx replace by collect rules
            for (var i = 0; i < session.SyntaxTree!.Count;)
            {
                var statement = session.SyntaxTree[i];

                if (statement.Rule == session.Parser!.Include)
                {
                    Requires(statement.Children != null && statement.Children.Count > 0);

                    var filename = statement.Children![0].GetText(session.Text!, session.Tokens!);
                    var filePath = filename.ResolveFile(session.IncludePaths);

                    if (session.IncludedFiles.ContainsKey(filePath))
                    {
                        // if the associated graph is null, we have a circular dependency
                        if (session.IncludedFiles[filePath] == null)
                        {
                            session.LogHandler!.Log(LogLevel.Warning, $"Circular include detected {filePath}.");
                        }
                        
                        // cache should already be merged with the result
                    }
                    else
                    {
                        // make sure the cache contains an entry in order to detect circular dependencies
                        session.IncludedFiles[filePath] = null;

                        session.LogHandler!.Log(LogLevel.Debug, $"including: {filename}({filePath}).");

                        session.IncludePaths = session.IncludePaths.AddFilePath(filePath);

                        var includeSession = RunPipeline(new PipelineSession<T>()
                        {
                           Text = File.ReadAllText(filePath),
                           IncludePaths = session.IncludePaths, 
                           Tokenizer = session.Tokenizer,
                           Parser = session.Parser,
                           LogHandler = session.LogHandler,
                           IncludedFiles = session.IncludedFiles,
                           Compiler = session.Compiler,
                           RuleGraph = session.RuleGraph 
                        });

                        session.IncludedFiles[filePath] = includeSession.RuleGraph;
                    }

                    // remove the include from the syntax tree
                    session.SyntaxTree.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        private static void RegisterTokens(this RuleGraph<int> target, RuleGraph<char> tokenSource)
        {
            // register the tokens found in the interpreted ebnf tokenizer with the grammar compiler
            tokenSource
                .Where( f => f.Prune == AnnotationPruning.None
                    && !f.Name.StartsWith(CompilerFunctionNameGenerator.UnnamedRulePrefix))
                .ForEach( f =>
                    target.RegisterRule(new MatchSingleData<int>($"{f.Name}", f.Id, AnnotationPruning.None)));
        }
    }
}
