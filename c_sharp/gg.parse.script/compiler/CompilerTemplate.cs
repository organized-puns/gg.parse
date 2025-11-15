// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

using gg.parse.core;
using gg.parse.util;

namespace gg.parse.script.compiler
{    
    public interface ICompileOutputCollection : IEnumerable
    {
        void AddOutput(object output);
    }

    public delegate object? CompileFunc<TContext>(Type? targetType, Annotation annotation, TContext context)
        where TContext : CompileContext;

    public abstract class CompilerTemplate<TKey, TContext> : ICompilerTemplate<TContext>
        where TKey : notnull
        where TContext: CompileContext
    {
        private readonly Dictionary<TKey, CompileFunc<TContext>> _functionLookup = [];

        [DebuggerStepThrough]
        public CompilerTemplate() 
        { 
        }

        public CompilerTemplate(Dictionary<TKey, CompileFunc<TContext>> functions) => 
            functions.ForEach(kvp => _functionLookup.Add(kvp.Key, kvp.Value));

        public abstract ICompilerTemplate<TContext> RegisterDefaultFunctions();

        public void Register(TKey key, CompileFunc<TContext> function)
        {
            Assertions.RequiresNotNull(key);
            Assertions.RequiresNotNull(function);
            Assertions.Requires(!_functionLookup.ContainsKey(key));

            _functionLookup[key] = function;
        }

        [DebuggerStepThrough]
        public T? Compile<T>(Annotation annotation, TContext context) =>
            (T?) Compile(typeof(T), annotation, context);


        /// <summary>
        /// Compile the syntax tree if provided in the context else the tokens in the context
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public ICompileOutputCollection Compile(Type targetType, TContext context, ICompileOutputCollection result) =>
        
            context.SyntaxTree == null
                ? Compile(targetType, context.Tokens, context, result)
                : Compile(targetType, context.SyntaxTree, context, result);

        public ICompileOutputCollection Compile(
            Type targetType,
            ImmutableList<Annotation> annotations,
            TContext context, 
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
                    context.ReportError(ex.Message, node, ex);
                }
            }

            return context.Logs.Contains(LogLevel.Error | LogLevel.Fatal)
                ? PostCompile(context, container)
                : throw new AggregateErrorException("Failed compilation, see the 'Errors' for more information.",
                    context.Logs.GetEntries(LogLevel.Error | LogLevel.Fatal));
        }

        
        public virtual object? Compile(Type? targetType, Annotation annotation, TContext context)
        {
            object? result = default;

            try
            {
                if (_functionLookup.TryGetValue(SelectKey(targetType, annotation, context), out var func))
                {
                    result = func(targetType, annotation, context);
                }
                else
                {
                    context.ReportError(
                        $"Can't find a compile function matching '{annotation.Rule.Name}'.",
                        annotation
                    );
                }
            }
            catch (Exception e)
            {
                context.ReportException(e, annotation);
            }

            return result;
        }

        public virtual ICompileOutputCollection PostCompile(TContext context, ICompileOutputCollection result)
        {
            return result;
        }

        protected abstract TKey SelectKey(Type? targetType, Annotation annotation, TContext context);
    }
}
