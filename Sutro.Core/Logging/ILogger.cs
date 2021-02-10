namespace Sutro.Core.Logging
{
    public interface ILogger
    {
        void LogMessage(string s);

        void LogWarning(string s);

        void LogInfo(string s);

        void LogError(string s);
    }
}