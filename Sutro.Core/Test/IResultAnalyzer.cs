namespace Sutro.Core.Test
{
    public interface IResultAnalyzer
    {
        void CompareResults(string pathExpected, string pathActual);
    }
}