// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Globalization;

using gg.parse.core;
using gg.parse.script;
using gg.parse.script.compiler;

namespace gg.parse.calculator
{
    public class CalculatorCompiler : CompilerTemplate<string, CompileContext>
    {
        private readonly Parser _parser;

        public IRuleGraph<int>? Grammar => _parser.Grammar;

        public IRuleGraph<char> Tokenizer => _parser.Tokens;

        public CalculatorCompiler(string tokensSpec, string grammarSpec)
        {
            RegisterDefaultFunctions();

            _parser = new ParserBuilder().From(tokensSpec, grammarSpec).Build();
        }

        public double Interpret(string text)
        {
            var (tokensResult, treeResult) = _parser.Parse(text);

            return (tokensResult && treeResult && tokensResult.Annotations != null && treeResult.Annotations != null)
                ? Compile<double>(treeResult!.Annotations![0], 
                    new CompileContext(text, tokensResult.Annotations, treeResult.Annotations))
            
                : throw new ArgumentException("Failed to parse text.");
        }

        public override ICompilerTemplate<CompileContext> RegisterDefaultFunctions()
        {
            Register(CalculatorNames.Group, (t, a, c) => Compile(t, a[0]!, c));
            
            Register(CalculatorNames.Number, (t, a, c) => double.Parse(c.GetText(a), CultureInfo.InvariantCulture));

            Register(CalculatorNames.UnaryOperation, (t, a, c) =>
            {
                var sign = a[0]! == CalculatorNames.Plus ? 1.0 : -1.0;
                return sign * Compile<double>(a[1]!, c);
            });

            Register(CalculatorNames.Multiplication, (t, a, c) => 
                Compile<double>(a[0]!, c) * Compile<double>(a[2]!, c));

            Register(CalculatorNames.Addition, (t, a, c) =>
                Compile<double>(a[0]!, c) + Compile<double>(a[2]!, c));

            Register(CalculatorNames.Subtraction, (t, a, c) =>
                Compile<double>(a[0]!, c) - Compile<double>(a[2]!, c));

            Register(CalculatorNames.Division, (t, a, c) =>
                Compile<double>(a[0]!, c) / Compile<double>(a[2]!, c));

            return this;
        }

        protected override string SelectKey(Type? targetType, Annotation annotation, CompileContext context) =>
            annotation.Name;
    }
}
