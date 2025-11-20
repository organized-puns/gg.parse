// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Globalization;

using gg.parse.rules;
using gg.parse.script.parser;
using gg.parse.script.pipeline;
using gg.parse.util;

namespace gg.parse.calculator
{
    internal class Program
    {
        static void Main(string[] _)
        {
            // write a welcome message
            Console.WriteLine("Welcome to the gg.parse.calculator example.");
            Console.WriteLine("Type 'exit' or 'x' to quit, or enter a simple equation like 1+2.\n");

            // load the tokenizer and grammar from file
            var interpreter = new CalculatorCompiler(
                File.ReadAllText("assets/calculator.tokens"),
                File.ReadAllText("assets/calculator.grammar")
            );

            // set up a logger to handle errors and warnings
            var logger = new ScriptLogger()
            {
                Out = (level, message) =>
                {
                    if (level == LogLevel.Warning)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }
                    else if (level == LogLevel.Error)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    Console.WriteLine($"{level}: {message}");
                    Console.ResetColor();
                }
            };

            // loop until the user wants to exit
            while (true) 
            {
                try
                {
                    Console.Write(">");

                    var input = Console.ReadLine();

                    if (!string.IsNullOrEmpty(input))
                    {
                        if (IsExitCommand(input)) 
                        {
                            break;
                        }

                        Console.WriteLine(
                            interpreter
                                .Interpret(input)
                                .ToString(CultureInfo.InvariantCulture)
                        );
                    }

                }
                catch (ScriptException e)
                {
                    logger.ProcessScriptException(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine("unexpected exception occured... " + e);
                }
            }         
        }

        private static bool IsExitCommand(string input) =>
            input == "x" || input == "exit" || input == "quit" || input == "q";
    }
}
