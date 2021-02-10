namespace Sutro.Core.Logging
{
    public interface ILogger
    {
        void Write(string s, System.ConsoleColor? color = null);

        void WriteLine(string s, System.ConsoleColor? color = null);

        void WriteLine();

        void WriteLine(object o)
        {
            WriteLine(o.ToString());
        }
    }
}