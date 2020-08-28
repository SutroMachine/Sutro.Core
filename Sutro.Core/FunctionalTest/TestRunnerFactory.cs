using gs;
using Sutro.Core.Settings;
using Sutro.Core.Settings.Machine;
using Sutro.Core.Settings.Part;

namespace Sutro.Core.FunctionalTest
{
    public static class TestRunnerFactoryFFF
    {
        public static PrintTestRunner CreateTestRunner(string caseName, PrintProfileFFF settings)
        {
            var resultGenerator = CreateResultGenerator(settings);
            var resultAnalyzer = new ResultAnalyzer<FeatureInfo>(new FeatureInfoFactoryFFF(), new ConsoleLogger());
            return new PrintTestRunner(caseName, resultGenerator, resultAnalyzer);
        }

        public static ResultGenerator<SingleMaterialFFFPrintGenerator, PrintProfileFFF> CreateResultGenerator(PrintProfileFFF settings)
        {
            var logger = new ConsoleLogger();
            return new ResultGenerator<SingleMaterialFFFPrintGenerator, PrintProfileFFF>(
                new PrintGeneratorManager<SingleMaterialFFFPrintGenerator, PrintProfileFFF>(settings, "", "", logger), logger);
        }
    }
}