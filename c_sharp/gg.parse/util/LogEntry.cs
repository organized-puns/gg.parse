// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.util
{
    [Flags]
    public enum LogLevel
    {
        Fatal = 1,
        Error = 2,
        Warning = 4,
        Info = 8,
        Debug = 16,
        Any = Fatal | Error | Warning | Info | Debug
    }

    public sealed class LogEntry
    {
        public LogLevel Level { get; init; }

        public string Message { get; init; }

        public (int line, int column)? Position { get; init; }

        public Exception? Exception { get; init; }

        public LogEntry(LogLevel level, string message, (int line, int column)? position = null, Exception? exception = null)
        {
            Level = level;
            Message = message;
            Position = position;
            Exception = exception;
        }

        public override string ToString()
        {
            return $"[{Level}] {Message}";
        }
    }
}
