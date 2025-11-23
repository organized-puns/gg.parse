// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using System.Collections;
using System.Collections.Concurrent;

namespace gg.parse.util
{
    // xxx implement icollection
    public class LogCollection : IEnumerable<LogEntry>
    {
        private readonly ConcurrentQueue<LogEntry> _logEntries = [];
        private readonly int _maxEntryCount;
        private readonly int _minRemoval;
        private readonly Lock _drainLock = new();

        private LogLevel _logMask = 0;

        public int Count => _logEntries.Count;  

        public LogEntry this[int index] => _logEntries.ElementAt(index);

        public IEnumerable<LogEntry> GetEntries(LogLevel level = LogLevel.Any) =>
            _logEntries.Where(entry => (entry.Level & level) > 0);

        public bool Contains(LogLevel level) =>
            (_logMask & level) > 0;

        public LogCollection(int max = 512, int minRemoval = 128)
        {
            Assertions.Requires(minRemoval >= 1);

            _maxEntryCount = max;
            _minRemoval = minRemoval;
        }

        public void Add(LogEntry entry)
        {
            _logMask |= entry.Level;

            // create new space for logs. 
            // this is heuristic as new entries may come in
            // while removing
            if (_maxEntryCount > 0)
            {
                if (_logEntries.Count > _maxEntryCount)
                {
                    if (_minRemoval >= _logEntries.Count)
                    {
                        _logEntries.Clear();
                        _logMask = 0;
                    }
                    else
                    {
                        for (var i = 0; i < _minRemoval; i++)
                        {
                            _logEntries.TryDequeue(out var _);
                        }
                    }
                }
            }

            _logEntries.Enqueue(entry);
        }

        public void Log(
            LogLevel level,
            string message,
            (int line, int column)? position = null,
            Exception? exception = null
        )
        {
            Add(new LogEntry(level, message, position, exception));
        }

        public void Clear()
        {
            RemoveLogs();
        }


        public void RemoveLogs(int count = 0)
        {
            lock (_drainLock)
            {
                if (count <= 0 || count >= _logEntries.Count)
                {
                    _logEntries.Clear();
                    _logMask = 0;
                }
                else
                { 
                    for (var i = 0; i < count; i++)
                    {
                        _logEntries.TryDequeue(out var _);
                    }
                }
            }
        }

        public IEnumerator<LogEntry> GetEnumerator() =>
            _logEntries.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}