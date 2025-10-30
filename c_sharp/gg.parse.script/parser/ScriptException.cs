// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections.Immutable;

namespace gg.parse.script.parser
{
    public class ScriptException : Exception
    {
        public ImmutableList<Annotation>? Errors { get; init; }

        /// <summary>
        /// Text in which the errors occurred.
        /// </summary>
        public string? Text { get; init; }

        /// <summary>
        /// Tokens which were being parsed when the error occurred.
        /// </summary>
        public ImmutableList<Annotation>? Tokens { get; init; }

        public ScriptException(string message)
            : base(message)
        {
        }

        public ScriptException(string message, ImmutableList<Annotation> errors, string text)
            : base(message)
        {
            Errors = errors;
            Text = text;
        }

        public ScriptException(string message, ImmutableList<Annotation> errors, string text, ImmutableList<Annotation> tokens)
            : base(message)
        {
            Errors = errors;
            Text = text;
            Tokens = tokens;
        }
    }
}

