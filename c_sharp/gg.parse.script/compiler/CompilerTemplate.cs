// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.util;
using System.Collections.Immutable;

namespace gg.parse.script.compiler
{    
    public interface ICompileOutputCollection<T> : IEnumerable<T>
    {
        void AddOutput(T output);
    }

    public class CompileContext<T>
    {
        public string Text { get; init; }

        public CompilerTemplate<T> Compiler { get; init; }

        public ImmutableList<Annotation> Tokens { get; init; }

        public ImmutableList<Annotation>? SyntaxTree { get; init; }

        public List<Exception> Exceptions { get; init; }

        public CompileContext(
            string text, 
            CompilerTemplate<T> compiler, 
            ImmutableList<Annotation> tokens
        )
        {
            Text = text;
            Compiler = compiler;
            Tokens = tokens;
            SyntaxTree = null;
            Exceptions = [];
        }

        public CompileContext(
            string text, 
            CompilerTemplate<T> compiler, 
            ImmutableList<Annotation> tokens, 
            ImmutableList<Annotation> syntaxTree
        )
        {
            Text = text;
            Compiler = compiler;
            Tokens = tokens;
            SyntaxTree = syntaxTree;
            Exceptions = [];
        }

        public string GetText(Annotation annotation) =>
            Text.Substring(Tokens.CombinedRange(annotation.Range));

        public object? Compile(Annotation annotation) =>
            Compiler.Compile(annotation, this);
    }

    public delegate T? CompileFunc<T>(Annotation annotation, CompileContext<T> context);

    
    public class CompilerTemplate<T>
    {
        private readonly Dictionary<string, CompileFunc<T>> _functionLookup = [];

        public void Register(IRule rule, CompileFunc<T> function)
        {
            Register(rule.Name, function);
        }

        public void Register(string ruleName, CompileFunc<T> function)
        {
            Assertions.RequiresNotNullOrEmpty(ruleName);
            Assertions.RequiresNotNull(function);
            Assertions.Requires(!_functionLookup.ContainsKey(ruleName));

            _functionLookup[ruleName] = function;
        }

        public ICompileOutputCollection<T> Compile(CompileContext<T> context, ICompileOutputCollection<T> result) =>
        
            context.SyntaxTree != null
                ? Compile(context.SyntaxTree, context, result)
                : Compile(context.Tokens, context, result);

        private ICompileOutputCollection<T> Compile(
            ImmutableList<Annotation> annotations, 
            CompileContext<T> context, 
            ICompileOutputCollection<T> container
        )
        {
            foreach (var node in annotations)
            {
                try
                {
                    var output = Compile(node, context);

                    if (output != null)
                    {
                        container.AddOutput(output);
                    }
                }
                catch (Exception ex)
                {
                    // add the exception and continue with the other rules
                    context.Exceptions.Add(ex);
                }
            }

            return context.Exceptions.Count == 0
                ? PostCompile(context, container)
                : throw new AggregateException("Failed compilation, see inner exceptions for more information", context.Exceptions);
        }

        public T? Compile(Annotation annotation, CompileContext<T> context)
        {
            T? result = default;
            
            if (_functionLookup.TryGetValue(annotation.Rule.Name, out var func))
            {
                result = func(annotation, context);
            }
            else
            {
                context.Exceptions.Add(new CompilationException($"Can't find a function matching {annotation.Rule.Name}."));
            }

            return result;
        }

        public virtual ICompileOutputCollection<T> PostCompile(CompileContext<T> context, ICompileOutputCollection<T> result)
        {
            return result;
        }
    }
}
