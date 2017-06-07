using Protoinject;

namespace Protogame
{
    using System;
    using System.Collections.Generic;

    public class EditorConsole : IConsole
    {
        private readonly List<ConsoleEntry> _log = new List<ConsoleEntry>();
        private readonly object _logLock = new object();

        public ConsoleState State { get; private set; }

        public ConsoleEntry[] Entries
        {
            get
            {
                lock (_logLock)
                {
                    return _log.ToArray();
                }
            }
        }

        public long EntryCount => _log.Count;
        
        public void Toggle()
        {
        }

        public void Render(IGameContext gameContext, IRenderContext renderContext)
        {
        }

        public void Update(IGameContext gameContext, IUpdateContext updateContext)
        {
        }

        public void Log(string message)
        {
            foreach (var m in message.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                LogInternal(new ConsoleEntry { Count = 1, Message = m?.TrimEnd(), Name = string.Empty });
            }
        }

        public void LogStructured(INode node, string format, object[] args)
        {
            var name = string.IsNullOrWhiteSpace(node.Name) ? node.Type.Name : node.Name;

            if (name.Length > 20)
            {
                name = name.Substring(0, 17) + "...";
            }

            var message = args == null ? format : string.Format(format, args);
            foreach (var m in message.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                LogInternal(new ConsoleEntry { Count = 1, LogLevel = ConsoleLogLevel.Debug, Message = m?.TrimEnd(), Name = name });
            }
        }

        public void LogStructured(INode node, ConsoleLogLevel logLevel, string format, object[] args)
        {
            var name = string.IsNullOrWhiteSpace(node.Name) ? node.Type.Name : node.Name;

            if (name.Length > 20)
            {
                name = name.Substring(0, 17) + "...";
            }

            var message = args == null ? format : string.Format(format, args);
            foreach (var m in message.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                LogInternal(new ConsoleEntry { Count = 1, LogLevel = logLevel, Message = m?.TrimEnd(), Name = name });
            }
        }

        private void LogInternal(ConsoleEntry consoleEntry)
        {
            lock (_logLock)
            {
                if (_log.Count > 0)
                {
                    var last = _log[_log.Count - 1];

                    if (last.Name != string.Empty && last.Name == consoleEntry.Name && last.Message == consoleEntry.Message)
                    {
                        last.Count++;
                    }
                    else
                    {
                        _log.Add(consoleEntry);
                    }
                }
                else
                {
                    _log.Add(consoleEntry);
                }
            }
        }

        public class ConsoleEntry
        {
            public string Name { get; set; }

            public int Count { get; set; }

            public string Message { get; set; }

            public ConsoleLogLevel LogLevel { get; set; }
        }
    }
}