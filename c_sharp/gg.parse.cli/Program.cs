// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.script;
using gg.parse.script.compiler;
using gg.parse.script.parser;
using gg.parse.util;

namespace gg.parse.cli
{
    internal class Program
    {
        private class CommandCompiler : NameCompilerBase<CompileContext> 
        {
            public string CompileExportGrammarNames(Type? _, Annotation annotation, CompileContext context)
            {
                var tokenFile = context.GetDelimitedStringValue(annotation[1]!);
                var grammarFile = context.GetDelimitedStringValue(annotation[2]!);
                var qualifiedClassName = context.GetText(annotation[3]!);

                var builder = new ParserBuilder().FromFile(tokenFile, grammarFile, new ScriptLogger(logs: context.Logs));
                var startClassName = qualifiedClassName.LastIndexOf('.');
                var className = qualifiedClassName[(startClassName + 1)..];
                var classNamespace = qualifiedClassName[..startClassName];

                return ScriptUtils.ExportNames(builder.TokenGraph, builder.GrammarGraph, classNamespace, className);
            }
        }

        static void Main(string[] args)
        {
            var logger = new ScriptLogger();

            try
            {
                var argsParser = new ParserBuilder()
                            .FromFile("assets/command_line.tokens", "assets/command_line.grammar", logger: logger)
                            .Build();

                var text = "export 'assets/command_line.tokens' 'assets/command_line.grammar' gg.parse.cli.ParseNames";// string.Join("", args);

                if (!string.IsNullOrEmpty(text))
                {
                    var (tokens, syntaxTree) = argsParser.Parse(text, logger: logger);
                    var compiler = new CommandCompiler();
                    var session = new CompileContext(text, tokens.Annotations!, syntaxTree.Annotations!, logger.ReceivedLogs);
                    var output = compiler.Compile<string>(syntaxTree[0]!, session);

                    Console.WriteLine(output);
                }
                else
                {
                    Console.WriteLine("Missing command line arguments, type 'parse_cli help' or 'parse_cli ?' for help.");
                }
            }
            catch (Exception e)
            {
                var errorEntries = logger.ReceivedLogs!.GetEntries(LogLevel.Error | LogLevel.Warning);

                // any reported errors ?

                if (errorEntries.Any())
                {
                    foreach (var message in logger.ReceivedLogs!.GetEntries(LogLevel.Error | LogLevel.Warning))
                    {
                        Console.WriteLine(message);

                        if (message.Exception != null)
                        {
                            Console.WriteLine("  " + message.Exception.Message);
                        }
                    }
                }
                else
                {
                    // some other exception was thrown
                    Console.WriteLine(e);
                }
            }
        }
    }
}
