namespace Sutro.Core.FunctionalTest
{
    public readonly struct Comparison
    {
        public Comparison(bool match, string message)
        {
            Match = match;
            Message = message;
        }

        public bool Match { get; }
        public string Message { get; }
    }
}