// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.rules;
using gg.parse.script.parser;
using gg.parse.util;

namespace gg.parse.script.common
{
    public class CommonParser : CommonGraphWrapper<int>
    {
        public CommonGraphWrapper<char> Tokenizer { get; init; }

        public CommonParser(CommonGraphWrapper<char> tokenizer)
        {
            Assertions.RequiresNotNull(tokenizer, nameof(tokenizer));
            Tokenizer = tokenizer;
        }

        /// <summary>
        /// Parse and validate the results of the various steps involved.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <exception cref="TokenizeException">Thrown when the tokenization step results in errors.</exception>
        /// <exception cref="ScriptException">Thrown when parsing reports error.</exception>
        public (List<Annotation>? tokens, List<Annotation>? astNodes) Parse(string text, bool failOnWarning = false)
        { 
            var (tokeninzeResult, parseResult) = this.Parse(Tokenizer, text, failOnWarning);
            return (tokeninzeResult.Annotations, parseResult.Annotations);
        }            

        public MatchSingleData<int> Token(string tokenName) =>
            MatchSingle($"{RuleOutput.Void.GetToken()}{tokenName}", Tokenizer.FindRule(tokenName)!.Id);

        public MatchSingleData<int> Token(string ruleName, string tokenName) =>
            MatchSingle(ruleName, Tokenizer.FindRule(tokenName)!.Id);
    }
}
