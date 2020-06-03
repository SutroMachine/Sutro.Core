﻿using gs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.FunctionalTest;
using Sutro.Core.FunctionalTest.FeatureMismatchExceptions;
using System;

namespace gsCore.FunctionalTests
{
    [TestClass]
    public class FFF_PrintTests_ExpectedFailures
    {
        private const string CaseName = "Cube.Failures";

        [ClassInitialize]
        public static void CreateExpectedResult(TestContext context)
        {
            var generator = TestRunnerFactoryFFF.CreateResultGenerator(new GenericRepRapSettings());

            var directory = TestDataPaths.GetTestDataDirectory(CaseName);

            generator.GenerateResultFile(
                TestDataPaths.GetMeshFilePath(directory),
                TestDataPaths.GetExpectedFilePath(directory));
        }

        [TestMethod]
        public void WrongLayerHeight()
        {
            ExpectFailure<LayerCountException>(new GenericRepRapSettings() { LayerHeightMM = 0.3 });
        }

        [TestMethod]
        public void WrongShells()
        {
            ExpectFailure<CumulativeExtrusionException>(new GenericRepRapSettings() { Shells = 3 });
        }

        [TestMethod]
        public void WrongFloorLayers()
        {
            ExpectFailure<MissingFeatureException>(new GenericRepRapSettings() { FloorLayers = 0 });
        }

        [TestMethod]
        public void WrongRoofLayers()
        {
            ExpectFailure<MissingFeatureException>(new GenericRepRapSettings() { FloorLayers = 3 });
        }

        [TestMethod]
        public void WrongLocation()
        {
            var settings = new GenericRepRapSettings();
            settings.Machine.BedOriginFactorX = 0.5;
            settings.Machine.BedOriginFactorY = 0.5;
            ExpectFailure<BoundingBoxException>(settings);
        }

        public void ExpectFailure<ExceptionType>(GenericRepRapSettings settings) where ExceptionType : Exception
        {
            // Arrange
            var resultGenerator = TestRunnerFactoryFFF.CreateResultGenerator(settings);
            var resultAnalyzer = new ResultAnalyzer<FeatureInfo>(new FeatureInfoFactoryFFF(), new ConsoleLogger());
            var print = new PrintTestRunner(CaseName, resultGenerator, resultAnalyzer);

            // Act
            print.GenerateFile();

            // Assert
            Assert.ThrowsException<ExceptionType>(() =>
            {
                print.CompareResults();
            });
        }
    }
}