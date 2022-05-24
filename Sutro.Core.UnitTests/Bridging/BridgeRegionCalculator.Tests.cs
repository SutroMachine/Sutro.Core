using FluentAssertions;
using g3;
using gs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.Bridging;
using Sutro.Core.Parallel;

namespace Sutro.Core.UnitTests.Bridging
{
    [TestClass()]
    public class BridgeRegionCalculatorTests
    {
        private readonly PlanarSlice _currentSlice;
        private readonly PlanarSlice _nextSlice;

        public BridgeRegionCalculatorTests()
        {
            _currentSlice = new PlanarSlice();
            _currentSlice.Solids.Add(new GeneralPolygon2d(Polygon2d.MakeRectangle(
                new Vector2d(0, 0), new Vector2d(10, 10))));
            _currentSlice.Solids.Add(new GeneralPolygon2d(Polygon2d.MakeRectangle(
                new Vector2d(20, 0), new Vector2d(30, 10))));
            _currentSlice.BuildSpatialCaches();

            _nextSlice = new PlanarSlice();
            _nextSlice.Solids.Add(new GeneralPolygon2d(Polygon2d.MakeRectangle(
                new Vector2d(0, 0), new Vector2d(30, 10))));
            _nextSlice.BuildSpatialCaches();
        }

        [TestMethod()]
        public void JustBelowThreshold()
        {
            // Arrange
            var calculator = new BridgeRegionCalculator(new ParallelActorSerial(),
                maxBridgeDistance: 9.99,
                bridgeTolerance: 5,
                minArea: -1,
                extraExpansion: .025);

            // Act
            var bridgeRegions = calculator.CalculateLayer(_currentSlice, _nextSlice);

            // Assert
            bridgeRegions.Should().BeEmpty();
        }

        [TestMethod()]
        public void JustAboveThreshold()
        {
            // Arrange
            var calculator = new BridgeRegionCalculator(new ParallelActorSerial(),
                maxBridgeDistance: 10.01,
                bridgeTolerance: 5,
                minArea: -1,
                extraExpansion: .025);

            // Act
            var bridgeRegions = calculator.CalculateLayer(_currentSlice, _nextSlice);

            // Assert
            bridgeRegions.Should().HaveCount(1);
        }
    }
}