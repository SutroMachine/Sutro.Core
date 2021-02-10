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
        public void Write(string s, ConsoleColor? color = null)
        {
            UseConsoleWithColorOverride(() => Console.Write(s), color);
        }

        public void WriteLine(string s, ConsoleColor? color = null)
        {
            UseConsoleWithColorOverride(() => Console.WriteLine(s), color);
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }
    }
}