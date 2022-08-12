namespace Sutro.Core.FunctionalTest
{
    public interface IResultAnalyzer
    {
        ComparisonReport CompareResults(string pathExpected, string pathActual);
    }
}