// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.script.pipeline
{
    public class ScriptPipelineException : Exception
    {
        public ScriptPipelineException() { }

        public ScriptPipelineException(string message) : base(message) { }

        public ScriptPipelineException(string message, Exception inner) : base(message, inner) { }
    }
}
