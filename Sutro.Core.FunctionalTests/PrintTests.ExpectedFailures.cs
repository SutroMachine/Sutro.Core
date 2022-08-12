using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.FunctionalTest;
using Sutro.Core.Settings;
using System;
using System.Collections.Generic;

namespace gsCore.FunctionalTests
{
    [TestClass]
    public class FFF_PrintTests_ExpectedFailures
    {
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
            ExpectFailure(new PrintProfileFFF() { Part = { LayerHeightMM = 0.3 } }, new[] {
                "Expected 15 layers but the result has 10"
            });
        }

        [TestMethod]
        public void WrongShells()
        {
            ExpectFailure(new PrintProfileFFF() { Part = { Shells = 3 } }, new[] {
                "Cumulative extrusion amounts aren't equal",
                "Cumulative durations aren't equal",
                "Cumulative distances aren't equal"});
        }

        [TestMethod]
        public void WrongFloorLayers()
        {
            ExpectFailure(new PrintProfileFFF() { Part = { FloorLayers = 0 } }, new[] {
                "Cumulative extrusion amounts aren't equal",
                "Cumulative distances aren't equal"});
        }

        [TestMethod]
        public void WrongRoofLayers()
        {
            ExpectFailure(new PrintProfileFFF() { Part = { RoofLayers = 3 } }, new[] {
                "Cumulative extrusion amounts aren't equal",
                "Cumulative distances aren't equal"});
        }

        [TestMethod]
        public void WrongLocation()
        {
            var settings = new PrintProfileFFF();
            settings.Machine.OriginX = Sutro.Core.Models.Profiles.MachineBedOriginLocationX.Center;
            settings.Machine.OriginY = Sutro.Core.Models.Profiles.MachineBedOriginLocationY.Center;
            ExpectFailure(settings, new[] { "Centers of mass aren't equal" });
        }

        public void ExpectFailure(PrintProfileFFF settings, IEnumerable<string> messages)
        {
            // Arrange
            var resultGenerator = TestRunnerFactoryFFF.CreateResultGenerator(settings);
            var resultAnalyzer = new ResultAnalyzer<FeatureInfo>(new FeatureInfoFactoryFFF());
            var print = new PrintTestRunner(CaseName, resultGenerator, resultAnalyzer);

            // Act
            print.GenerateFile();

            // Assert
            var comparison = print.CompareResults();
            var report = comparison.GetReport();
            Console.WriteLine(report);
            comparison.AreEquivalent.Should().BeFalse();
            foreach (var message in messages)
            {
                report.Should().Contain(message);
            }
        }
    }
}