// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.script.parser
{
    public class ScriptException : Exception
    {
        public List<Annotation>? Errors { get; init; }

        /// <summary>
        /// Text in which the errors occurred.
        /// </summary>
        public string? Text { get; init; }

        /// <summary>
        /// Tokens which were being parsed when the error occurred.
        /// </summary>
        public List<Annotation>? Tokens { get; init; }

        public ScriptException(string message)
            : base(message)
        {
        }

        public ScriptException(string message, List<Annotation> errors, string text)
            : base(message)
        {
            Errors = errors;
            Text = text;
        }

        public ScriptException(string message, List<Annotation> errors, string text, List<Annotation> tokens)
            : base(message)
        {
            Errors = errors;
            Text = text;
            Tokens = tokens;
        }
    }
}

