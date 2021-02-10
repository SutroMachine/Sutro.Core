using System;

namespace Sutro.Core.Logging
{
    public class NullLogger : ILogger
    {
        public void LogError(string s)
        {
        }

        public void LogInfo(string s)
        {
        }

        public void LogMessage(string s)
        {
        }

        public void LogWarning(string s)
        {
        }
    }
}