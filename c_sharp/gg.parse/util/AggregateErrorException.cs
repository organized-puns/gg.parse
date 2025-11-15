// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.util
{
    /// <summary>
    /// Exception capturing multiple errors (Fatal/Error/Warning) 
    /// </summary>
    public class AggregateErrorException : Exception
    {
        public IEnumerable<LogEntry> Errors { get; private set; }

        public AggregateErrorException(string message, IEnumerable<LogEntry> errors)
            : base(message)
        {
            Errors = errors;
        }

        public AggregateErrorException(IEnumerable<LogEntry> errors)
            : base("Multiple errors reported.")
        {
            Errors = errors;
        }
    }
}
