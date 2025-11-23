// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Reflection;

using gg.parse.core;
using gg.parse.util;

namespace gg.parse.script.compiler
{
    public class NameCompilerBase<TContext> : CompilerTemplate<string, TContext>
        where TContext : CompileContext
    {
        public const string CompileMethodPrefix = "Compile";

        public NameCompilerBase() 
        {
            RegisterDefaultFunctions();
        }

        public override ICompilerTemplate<TContext> RegisterDefaultFunctions()
        {
            var type = GetType();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance );

            foreach (var method in methods.Where(m => m.Name.StartsWith(CompileMethodPrefix) && m.Name != CompileMethodPrefix))
            {
                var parameters = method.GetParameters();

                if (parameters.Length >= 3
                    && parameters[0].ParameterType == typeof(Type)
                    && parameters[1].ParameterType == typeof(Annotation)
                    && parameters[2].ParameterType == typeof(TContext))
                {
                    var ruleName = method.Name[CompileMethodPrefix.Length..].SplitOnCapitals(toLowerCase: true);
                    var compileDelegate = (CompileFunc<TContext>)Delegate.CreateDelegate(typeof(CompileFunc<TContext>), this, method);
                    Register(ruleName, compileDelegate);
                }
            }

            return this;
        }

        protected override string SelectKey(Type? targetType, Annotation annotation, TContext context) =>
            annotation.Name;

    }
}
