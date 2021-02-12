using Sutro.Core.Generators;
using System.IO;

namespace Sutro.Core.FunctionalTest
{
    public class PrintTestRunner
    {
        protected readonly IResultGenerator resultGenerator;
        private readonly IResultAnalyzer resultAnalyzer;

        protected DirectoryInfo directory;

        public PrintTestRunner(string name, IResultGenerator resultGenerator, IResultAnalyzer resultAnalyzer)
        {
            this.resultGenerator = resultGenerator;
            this.resultAnalyzer = resultAnalyzer;

            directory = TestDataPaths.GetTestDataDirectory(name);
        }

        public void CompareResults()
        {
            resultAnalyzer.CompareResults(
                TestDataPaths.GetExpectedFilePath(directory),
                TestDataPaths.GetResultFilePath(directory));
        }

        public GenerationResult GenerateFile()
        {
            return resultGenerator.GenerateResultFile(
                TestDataPaths.GetMeshFilePath(directory),
                TestDataPaths.GetResultFilePath(directory));
        }
    }
}