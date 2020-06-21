namespace Sutro.Core.FunctionalTest
{
    public interface IResultGenerator
    {
        public GenerationResult GenerateResultFile(string meshFilePath, string outputFilePath);
    }
}