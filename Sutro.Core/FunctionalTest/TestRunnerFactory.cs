using gs;
using Sutro.Core.Logging;

namespace Sutro.Core.FunctionalTest
{
    public static class TestRunnerFactoryFFF
    {
        public static PrintTestRunner CreateTestRunner(string caseName, SingleMaterialFFFSettings settings)
        {
            var resultGenerator = CreateResultGenerator(settings);
            var resultAnalyzer = new ResultAnalyzer<FeatureInfo>(new FeatureInfoFactoryFFF(), new ConsoleLogger());
            return new PrintTestRunner(caseName, resultGenerator, resultAnalyzer);
        }

        public static ResultGenerator<SingleMaterialFFFPrintGenerator, SingleMaterialFFFSettings> CreateResultGenerator(SingleMaterialFFFSettings settings)
        {
            var logger = new ConsoleLogger();
            return new ResultGenerator<SingleMaterialFFFPrintGenerator, SingleMaterialFFFSettings>(
                new PrintGeneratorManager<SingleMaterialFFFPrintGenerator, SingleMaterialFFFSettings>(settings, "", "", logger), logger);
        }
    }
}