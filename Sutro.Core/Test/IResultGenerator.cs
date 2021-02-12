using Sutro.Core.Generators;

namespace Sutro.Core.Test
{
    public interface IResultGenerator
    {
        public GenerationResult GenerateResultFile(string meshFilePath, string outputFilePath);
    }
}