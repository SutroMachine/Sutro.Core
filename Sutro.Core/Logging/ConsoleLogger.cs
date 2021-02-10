using System;

namespace Sutro.Core.Logging
{
    public class ConsoleLogger : ILogger
    {
        protected void UseConsoleWithColorOverride(Action action, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            var previousForeground = Console.ForegroundColor;
            var previousBackground = Console.BackgroundColor;

            if (foreground != null) Console.ForegroundColor = foreground.Value;
            if (background != null) Console.BackgroundColor = background.Value;

            action.Invoke();

            Console.ForegroundColor = previousForeground;
            Console.BackgroundColor = previousBackground;

        }

        private void WriteLine(string s, ConsoleColor? color = null)
        {
            UseConsoleWithColorOverride(() => Console.WriteLine(s), color);
        }

        private void ErrorWriteLine(string s, ConsoleColor? color = null)
        {
            UseConsoleWithColorOverride(() => Console.Error.WriteLine(s), color);
        }

        public void LogError(string s)
        {
            ErrorWriteLine($"error: {s}", ConsoleColor.Red);
        }

        public void LogMessage(string s)
        {
            WriteLine($"{s}");
        }

        public void LogWarning(string s)
        {
            WriteLine($"warning: {s}", ConsoleColor.Yellow);
        }

        public void LogInfo(string s)
        {
            WriteLine($"info: {s}", ConsoleColor.Gray);
        }
    }
}