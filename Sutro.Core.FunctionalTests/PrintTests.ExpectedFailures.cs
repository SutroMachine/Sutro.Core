using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.Logging;
using Sutro.Core.Settings;
using Sutro.Core.Test;
using Sutro.Core.Test.Exceptions;
using System;

namespace Sutro.Core.FunctionalTests
{
    [TestClass]
    public class FFF_PrintTests_ExpectedFailures : TestBase
    {
        public FFF_PrintTests_ExpectedFailures() : base()
        {
        }

        private const string CaseName = "Cube.Failures";

        [ClassInitialize]
        public static void CreateExpectedResult(TestContext context)
        {
            var generator = TestRunnerFactoryFFF.CreateResultGenerator(new PrintProfileFFF());

            var directory = TestDataPaths.GetTestDataDirectory(CaseName);

            generator.GenerateResultFile(
                TestDataPaths.GetMeshFilePath(directory),
                TestDataPaths.GetExpectedFilePath(directory));
        }

        [TestMethod]
        public void WrongLayerHeight()
        {
            ExpectFailure<LayerCountException>(new PrintProfileFFF() { Part = { LayerHeightMM = 0.3 } });
        }

        [TestMethod]
        public void WrongShells()
        {
            ExpectFailure<CumulativeExtrusionException>(new PrintProfileFFF() { Part = { Shells = 3 } });
        }

        [TestMethod]
        public void WrongFloorLayers()
        {
            ExpectFailure<MissingFeatureException>(new PrintProfileFFF() { Part = { FloorLayers = 0 } });
        }

        [TestMethod]
        public void WrongRoofLayers()
        {
            ExpectFailure<MissingFeatureException>(new PrintProfileFFF() { Part = { FloorLayers = 3 } });
        }

        [TestMethod]
        public void WrongLocation()
        {
            var settings = new PrintProfileFFF();
            settings.Machine.OriginX = Sutro.Core.Models.Profiles.MachineBedOriginLocationX.Center;
            settings.Machine.OriginY = Sutro.Core.Models.Profiles.MachineBedOriginLocationY.Center;
            ExpectFailure<BoundingBoxException>(settings);
        }

        public void ExpectFailure<ExceptionType>(PrintProfileFFF settings) where ExceptionType : Exception
        {
            // Arrange
            var resultGenerator = TestRunnerFactoryFFF.CreateResultGenerator(settings);
            var resultAnalyzer = new ResultAnalyzer<FeatureInfo>(new FeatureInfoFactoryFFF(), new ConsoleLogger());
            var print = new PrintTestRunner(CaseName, resultGenerator, resultAnalyzer);

            // Act
            var result = print.GenerateFile();

            // Assert
            Assert.ThrowsException<ExceptionType>(() =>
            {
                print.CompareResults();
            });
        }
    }
}