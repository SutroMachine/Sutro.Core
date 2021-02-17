using Sutro.Core.Generators;
using Sutro.Core.Logging;
using Sutro.Core.Settings;

namespace Sutro.Core.Test
{
    public static class TestRunnerFactoryFFF
    {
        public static PrintTestRunner CreateTestRunner(string caseName, PrintProfileFFF settings, ILogger _logger = null)
        {
            var logger = _logger ?? new ConsoleLogger();
            var resultGenerator = CreateResultGenerator(settings, logger);
            var resultAnalyzer = new ResultAnalyzer<FeatureInfo>(new FeatureInfoFactoryFFF(), logger);
            return new PrintTestRunner(caseName, resultGenerator, resultAnalyzer);
        }

        public static ResultGenerator<SingleMaterialFFFPrintGenerator, PrintProfileFFF> CreateResultGenerator(
            PrintProfileFFF settings, ILogger _logger = null)
        {
            var logger = _logger ?? new ConsoleLogger();
            return new ResultGenerator<SingleMaterialFFFPrintGenerator, PrintProfileFFF>(
                new PrintGeneratorManager<SingleMaterialFFFPrintGenerator, PrintProfileFFF>(settings, "", "", logger), logger);
        }
    }
}