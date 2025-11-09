// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.util;
using System.Collections;
using System.Collections.Immutable;

namespace gg.parse.script.compiler
{    
    public interface ICompileOutputCollection : IEnumerable
    {
        void AddOutput(object output);
    }

    public interface ICompilerTemplate
    {
        ICompileOutputCollection Compile(
            Type targetType,
            ImmutableList<Annotation> annotations,
            CompileContext context,
            ICompileOutputCollection container
        );

        ICompilerTemplate RegisterDefaultFunctions();

        T? Compile<T>(Annotation annotation, CompileContext context);

        object? Compile(Type? targetType, Annotation annotation, CompileContext context);
    }


    public class CompileContext
    {
        public string Text { get; init; }

        // xxx keep this out of the context
        // public ICompilerTemplate Compiler { get; init; }

        public ImmutableList<Annotation> Tokens { get; init; }

        public ImmutableList<Annotation>? SyntaxTree { get; init; }

        public List<Exception> Exceptions { get; init; }

        public CompileContext(
            string text,
            //ICompilerTemplate compiler, 
            ImmutableList<Annotation> tokens
        )
        {
            Text = text;
            //Compiler = compiler;
            Tokens = tokens;
            SyntaxTree = null;
            Exceptions = [];
        }

        public CompileContext(
            string text,
            //ICompilerTemplate compiler, 
            ImmutableList<Annotation> tokens, 
            ImmutableList<Annotation> syntaxTree
        )
        {
            Text = text;
            //Compiler = compiler;
            Tokens = tokens;
            SyntaxTree = syntaxTree;
            Exceptions = [];
        }

        public string GetText(Annotation annotation) =>
            Text.Substring(Tokens.CombinedRange(annotation.Range));


        /*public T? Compile<T>(Annotation annotation) =>
            (T?) Compiler.Compile(typeof(T), annotation, this);

        public object? Compile(Annotation annotation) =>
            Compiler.Compile(null, annotation, this);

        public object? Compile(Type? targetType, Annotation annotation) =>
            Compiler.Compile(targetType, annotation, this);*/
    }

    public delegate object? CompileFunc(Type? targetType, Annotation annotation, CompileContext context);

    
    public abstract class CompilerTemplate<TKey> : ICompilerTemplate where TKey : notnull
    {
        private readonly Dictionary<TKey, CompileFunc> _functionLookup = [];

        public CompilerTemplate() 
        { 
        }

        public CompilerTemplate(Dictionary<TKey, CompileFunc> functions) => 
            functions.ForEach(kvp => _functionLookup.Add(kvp.Key, kvp.Value));


        public abstract ICompilerTemplate RegisterDefaultFunctions();

        public void Register(TKey key, CompileFunc function)
        {
            Assertions.RequiresNotNull(key);
            Assertions.RequiresNotNull(function);
            Assertions.Requires(!_functionLookup.ContainsKey(key));

            _functionLookup[key] = function;
        }

        public T? Compile<T>(Annotation annotation, CompileContext context) =>
            (T?)Compile(typeof(T), annotation, context);


        /// <summary>
        /// Compile the syntax tree if provided in the context else the tokens in the context
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public ICompileOutputCollection Compile(Type targetType, CompileContext context, ICompileOutputCollection result) =>
        
            context.SyntaxTree == null
                ? Compile(targetType, context.Tokens, context, result)
                : Compile(targetType, context.SyntaxTree, context, result);

        public ICompileOutputCollection Compile(
            Type targetType,
            ImmutableList<Annotation> annotations, 
            CompileContext context, 
            ICompileOutputCollection container
        )
        {
            foreach (var node in annotations)
            {
                try
                {
                    var output = Compile(targetType, node, context);

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

        
        public object? Compile(Type? targetType, Annotation annotation, CompileContext context)
        {
            object? result = default;
            
            if (_functionLookup.TryGetValue(SelectKey(targetType, annotation, context), out var func))
            {
                result = func(targetType,annotation, context);
            }
            else
            {
                context.Exceptions.Add(new CompilationException($"Can't find a function matching {annotation.Rule.Name}."));
            }

            return result;
        }

        public virtual ICompileOutputCollection PostCompile(CompileContext context, ICompileOutputCollection result)
        {
            return result;
        }

        protected abstract TKey SelectKey(Type? targetType, Annotation annotation, CompileContext context);
    }
}
