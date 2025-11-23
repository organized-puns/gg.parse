// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;
using System.Diagnostics;

using gg.parse.core;
using gg.parse.util;

namespace gg.parse.script.compiler
{    
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
        /// <param name="collection"></param>
        /// <returns></returns>
        public ICollection<T> Compile<T>(Type? targetType, TContext context, ICollection<T> collection) =>
        
            context.SyntaxTree == null
                ? Compile(targetType, context.Tokens, context, collection)
                : Compile(targetType, context.SyntaxTree, context, collection);

        public virtual ICollection<T> Compile<T>(
            Type? targetType,
            ImmutableList<Annotation> annotations,
            TContext context,
            ICollection<T> container
        ) 
        {
            foreach (var node in annotations)
            {
                try
                {
                    var output = Compile(targetType, node, context);

                    if (output != null )
                    {
                        if (output is T typedOutput)
                        {
                            AddOutput(typedOutput, container, context);
                        }
                        else
                        {
                            context.ReportError(
                                $"Mismatched type, expecing {typeof(T)}, the result was of type '{output.GetType()}'.",
                                node
                            );
                        }
                    }
                    else
                    {
                        context.Log(
                            LogLevel.Warning,
                            $"The result of compiling '{node.Name}' was null. It's not included in the result.",
                            node
                        );
                    }
                }
                catch (Exception ex)
                {
                    // add the exception and continue with the other rules
                    context.ReportError(ex.Message, node, ex);
                }
            }

            return !context.Logs.Contains(LogLevel.Error | LogLevel.Fatal)
                    ? container
                    : throw new AggregateErrorException("Failed compilation, see the 'Errors' for more information.",
                        context.Logs.GetEntries(LogLevel.Error | LogLevel.Fatal));
        }

        protected virtual void AddOutput<T>(
            T output, 
            ICollection<T> targetCollection,
            TContext context)
        {
            targetCollection.Add(output);
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

        protected abstract TKey SelectKey(Type? targetType, Annotation annotation, TContext context);
    }
}
