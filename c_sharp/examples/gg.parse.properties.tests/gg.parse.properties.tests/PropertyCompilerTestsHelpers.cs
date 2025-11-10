// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun
using System.Collections.Immutable;

using gg.parse.core;
using gg.parse.tests;

using Range = gg.parse.util.Range;

namespace gg.parse.properties.tests
{
    public static class PropertyCompilerTestsHelpers
    {
        // --- Private / util methods ---------------------------------------------------------------------------------

        public static (Annotation grammarAnnotation, ImmutableList<Annotation> tokens, string text)
            SetupSingleTokenTest(string text, string tokenName)
        {
            var token = new EmptyRule(tokenName);
            var tokens = ImmutableList<Annotation>.Empty.Add(new Annotation(token, new Range(0, text.Length)));
            var grammarAnnotation = new Annotation(new EmptyRule(0, tokenName), new Range(0, 1));

            return (grammarAnnotation, tokens, text);
        }
    }
}