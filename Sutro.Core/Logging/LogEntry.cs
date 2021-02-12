using Sutro.Core.Logging;

namespace Sutro.Core
{
    public readonly struct LogEntry
    {
        public LoggingLevel Level { get; }
        public string Message { get; }

        public LogEntry(LoggingLevel level, string message)
        {
            Level = level;
            Message = message;
        }
    }
}