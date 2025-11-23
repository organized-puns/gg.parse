// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;

using gg.parse.core;
using gg.parse.rules;
using gg.parse.script.compiler;
using gg.parse.script.parser;
using gg.parse.util;

using static gg.parse.util.Assertions;

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

            session.Compiler = new TokenizerCompiler();

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
            session.Compiler = new GrammarCompiler();

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
                var context = new RuleCompilationContext(
                    session.Text, 
                    session.Tokens, 
                    session.SyntaxTree, 
                    session.LogHandler.ReceivedLogs
                );

                var graph = session.RuleGraph;

                session.Compiler!.Compile(null, session.SyntaxTree, context, graph);

                graph.ResolveReferences(context);
            }
            catch (Exception e)
            {
                session.LogHandler?.ProcessException(e, session.Text, session.Tokens);

                throw new ScriptPipelineException("Exception(s) raised during compliation.", e);
            }

            var errorLevel = session.LogHandler.FailOnWarning
                                ? LogLevel.Error | LogLevel.Fatal | LogLevel.Warning
                                : LogLevel.Error | LogLevel.Fatal;

            if (session.LogHandler.ReceivedLogs!.Contains(errorLevel))
            {
                throw new ScriptPipelineException("Exception(s) raised during compliation.",
                    new AggregateErrorException("Errors encountered while compiling",
                        session.LogHandler.ReceivedLogs.GetEntries(errorLevel)));
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
                RuleGraph = [],
                IncludePaths = sessionIncludePaths
            };

            return session;
        }

        private static (ImmutableList<Annotation>? tokens, ImmutableList<Annotation>? syntaxTree) ParseSessionText<T>(PipelineSession<T> session)
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
                session.LogHandler!.ProcessScriptException(pe);
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
                    session.SyntaxTree = session.SyntaxTree.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        private static void RegisterTokens(this MutableRuleGraph<int> target, MutableRuleGraph<char> tokenSource)
        {
            // register the tokens found in the interpreted ebnf tokenizer with the grammar compiler
            tokenSource
                .Where( f => f.Prune == AnnotationPruning.None
                    && !f.Name.StartsWith(CompilerFunctionNameGenerator.UnnamedRulePrefix))
                .ForEach( f =>
                    target.Register(new MatchSingleData<int>($"{f.Name}", f.Id, AnnotationPruning.None)));
        }
    }
}
