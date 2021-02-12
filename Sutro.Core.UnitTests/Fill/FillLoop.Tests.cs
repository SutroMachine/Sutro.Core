using g3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.Fill;
using Sutro.Core.FillTypes;
using Sutro.Core.Settings;
using System.Collections.Generic;
using System.Linq;

namespace Sutro.Core.UnitTests.Fill
{
    [TestClass]
    public class FillLoopTests
    {
        private static double delta = 1e-4;

        [TestMethod]
        public void EnumerableVector2dConstructor()
        {
            // Arrange
            var vertices = new List<Vector2d>()
            {
                new Vector2d(2,0),
                new Vector2d(2,4),
                new Vector2d(0,4)
            };

            // Act
            var loop = new FillLoop<FillSegment>(vertices);

            // Assert
            Assert.AreEqual(3, loop.Elements.Count);
            Assert.AreEqual(2, loop.Elements[0].NodeStart.x);
        }

        [TestMethod]
        public void Vertices_RepeatFirstFalse()
        {
            // Act
            var vertices = FillFactory.CreateTriangleCCW().Vertices(false).ToList();

            // Assert
            Assert.AreEqual(3, vertices.Count);

            Assert.AreEqual(0, vertices[0].x, delta);
            Assert.AreEqual(0, vertices[0].y, delta);

            Assert.AreEqual(4, vertices[1].x, delta);
            Assert.AreEqual(0, vertices[1].y, delta);

            Assert.AreEqual(4, vertices[2].x, delta);
            Assert.AreEqual(3, vertices[2].y, delta);
        }

        [TestMethod]
        public void Vertices_RepeatFirstTrue()
        {
            // Act
            var vertices = FillFactory.CreateTriangleCCW().Vertices(true).ToList();

            // Assert
            Assert.AreEqual(4, vertices.Count);

            Assert.AreEqual(0, vertices[0].x, delta);
            Assert.AreEqual(0, vertices[0].y, delta);

            Assert.AreEqual(4, vertices[1].x, delta);
            Assert.AreEqual(0, vertices[1].y, delta);

            Assert.AreEqual(4, vertices[2].x, delta);
            Assert.AreEqual(3, vertices[2].y, delta);

            Assert.AreEqual(0, vertices[3].x, delta);
            Assert.AreEqual(0, vertices[0].y, delta);
        }

        [TestMethod]
        public void RollNoEffectCCW()
        {
            // Arrange
            var triangle = FillFactory.CreateTriangleCCW();

            // Act
            var result = (FillLoop<FillSegment>)triangle.RollBetweenVertices(new ElementLocation(0, 0));

            // Assert
            Assert.AreEqual(3, result.Elements.Count);

            Assert.AreEqual(0, result.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[0].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[1].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[1].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[2].NodeStart.x, delta);
            Assert.AreEqual(3, result.Elements[2].NodeStart.y, delta);

            Assert.AreEqual(0, result.Elements[2].NodeEnd.x, delta);
            Assert.AreEqual(0, result.Elements[2].NodeEnd.y, delta);
        }

        [TestMethod]
        public void RollNoEffectCW()
        {
            // Arrange
            var triangle = FillFactory.CreateTriangleCW();

            // Act
            var result = (FillLoop<FillSegment>)triangle.RollBetweenVertices(new ElementLocation(0, 0));

            // Assert
            Assert.AreEqual(3, result.Elements.Count);

            Assert.AreEqual(4, result.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(3, result.Elements[0].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[1].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[1].NodeStart.y, delta);

            Assert.AreEqual(0, result.Elements[2].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[2].NodeStart.y, delta);
        }

        [TestMethod]
        public void RollFirstSegmentHalfwayCCW()
        {
            // Arrange
            var triangle = FillFactory.CreateTriangleCCW();

            // Act
            var result = (FillLoop<FillSegment>)triangle.RollBetweenVertices(new ElementLocation(0, 0.5));

            // Assert
            Assert.AreEqual(4, result.Elements.Count);

            Assert.AreEqual(2, result.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[0].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[1].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[1].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[2].NodeStart.x, delta);
            Assert.AreEqual(3, result.Elements[2].NodeStart.y, delta);

            Assert.AreEqual(0, result.Elements[3].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[3].NodeStart.y, delta);
        }

        [TestMethod]
        public void RollFirstSegmentHalfwayCW()
        {
            // Arrange
            var triangle = FillFactory.CreateTriangleCW();

            // Act
            var result = (FillLoop<FillSegment>)triangle.RollBetweenVertices(new ElementLocation(0, 0.5));

            // Assert
            Assert.AreEqual(4, result.Elements.Count);

            Assert.AreEqual(4, result.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(1.5, result.Elements[0].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[1].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[1].NodeStart.y, delta);

            Assert.AreEqual(0, result.Elements[2].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[2].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[3].NodeStart.x, delta);
            Assert.AreEqual(3, result.Elements[3].NodeStart.y, delta);
        }

        [TestMethod]
        public void RollSecondSegmentHalfwayCCW()
        {
            // Arrange
            var triangle = FillFactory.CreateTriangleCCW();

            // Act
            var result = (FillLoop<FillSegment>)triangle.RollBetweenVertices(new ElementLocation(1, 1d / 3d));

            // Assert
            Assert.AreEqual(4, result.Elements.Count);

            Assert.AreEqual(4, result.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(1, result.Elements[0].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[1].NodeStart.x, delta);
            Assert.AreEqual(3, result.Elements[1].NodeStart.y, delta);

            Assert.AreEqual(0, result.Elements[2].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[2].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[3].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[3].NodeStart.y, delta);
        }

        [TestMethod]
        public void RollLastSegmentAtEnd()
        {
            // Arrange
            var triangle = FillFactory.CreateTriangleCCW();

            // Act
            var result = (FillLoop<FillSegment>)triangle.RollBetweenVertices(new ElementLocation(2, 1));

            // Assert
            Assert.AreEqual(3, result.Elements.Count);

            Assert.AreEqual(0, result.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[0].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[1].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[1].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[2].NodeStart.x, delta);
            Assert.AreEqual(3, result.Elements[2].NodeStart.y, delta);
        }

        [TestMethod]
        public void RollSecondSegmentHalfwayCW()
        {
            // Arrange
            var triangle = FillFactory.CreateTriangleCW();

            // Act
            var result = (FillLoop<FillSegment>)triangle.RollBetweenVertices(new ElementLocation(1, 0.5));

            // Assert
            Assert.AreEqual(4, result.Elements.Count);

            Assert.AreEqual(2, result.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[0].NodeStart.y, delta);

            Assert.AreEqual(0, result.Elements[1].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[1].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[2].NodeStart.x, delta);
            Assert.AreEqual(3, result.Elements[2].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[3].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[3].NodeStart.y, delta);
        }

        [TestMethod]
        public void RollFirstSegmentWithinToleranceLowSideCCW()
        {
            // Arrange
            var triangle = FillFactory.CreateTriangleCCW();

            // Act
            var result = (FillLoop<FillSegment>)triangle.RollBetweenVertices(new ElementLocation(0, 0.004 / 4), 0.005);

            // Assert
            Assert.AreEqual(3, result.Elements.Count);

            Assert.AreEqual(0, result.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[0].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[1].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[1].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[2].NodeStart.x, delta);
            Assert.AreEqual(3, result.Elements[2].NodeStart.y, delta);
        }

        [TestMethod]
        public void RollFirstSegmentWithinToleranceLowSideCW()
        {
            // Arrange
            var triangle = FillFactory.CreateTriangleCW();

            // Act
            var result = (FillLoop<FillSegment>)triangle.RollBetweenVertices(new ElementLocation(1, 0.004 / 4), 0.005);

            // Assert
            Assert.AreEqual(3, result.Elements.Count);

            Assert.AreEqual(4, result.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[0].NodeStart.y, delta);

            Assert.AreEqual(0, result.Elements[1].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[1].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[2].NodeStart.x, delta);
            Assert.AreEqual(3, result.Elements[2].NodeStart.y, delta);
        }

        [TestMethod]
        public void RollFirstSegmentWithinToleranceHighSideCCW()
        {
            // Arrange
            var triangle = FillFactory.CreateTriangleCCW();

            // Act
            var result = (FillLoop<FillSegment>)triangle.RollBetweenVertices(new ElementLocation(0, 1 - 0.004 / 4), 0.005);

            // Assert
            Assert.AreEqual(3, result.Elements.Count);

            Assert.AreEqual(4, result.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[0].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[1].NodeStart.x, delta);
            Assert.AreEqual(3, result.Elements[1].NodeStart.y, delta);

            Assert.AreEqual(0, result.Elements[2].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[2].NodeStart.y, delta);
        }

        [TestMethod]
        public void RollFirstSegmentWithinToleranceHighSideCW()
        {
            // Arrange
            var triangle = FillFactory.CreateTriangleCW();

            // Act
            var result = (FillLoop<FillSegment>)triangle.RollBetweenVertices(new ElementLocation(1, 1 - 0.004 / 4), 0.005);

            // Assert
            Assert.AreEqual(3, result.Elements.Count);

            Assert.AreEqual(0, result.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[0].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[1].NodeStart.x, delta);
            Assert.AreEqual(3, result.Elements[1].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[2].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[2].NodeStart.y, delta);
        }

        [TestMethod]
        public void ConvertToCurve()
        {
            // Act
            var result = FillFactory.CreateTriangleCCW().ConvertToCurve();

            // Assert
            Assert.AreEqual(3, result.Elements.Count);

            Assert.AreEqual(0, result.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[0].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[1].NodeStart.x, delta);
            Assert.AreEqual(0, result.Elements[1].NodeStart.y, delta);

            Assert.AreEqual(4, result.Elements[2].NodeStart.x, delta);
            Assert.AreEqual(3, result.Elements[2].NodeStart.y, delta);

            Assert.AreEqual(0, result.Elements[2].NodeEnd.x, delta);
            Assert.AreEqual(0, result.Elements[2].NodeEnd.y, delta);
        }

        [TestMethod]
        public void CloneBare()
        {
            // Arrange
            var settings = new PrintProfileFFF();
            var loop = FillFactory.CreateTriangleCCW();
            loop.FillType = settings.FillTypeFactory.OuterPerimeter();
            loop.PerimeterOrder = 100;
            loop.IsHoleShell = true;
            loop.FillThickness = 3;

            // Act
            var clone = loop.CloneBare();

            // Assert
            Assert.AreEqual(100, clone.PerimeterOrder);
            Assert.AreEqual(3, clone.FillThickness);
            Assert.IsTrue(clone.IsHoleShell);
            Assert.IsInstanceOfType(clone.FillType, typeof(OuterPerimeterFillType));
        }

        [TestMethod]
        public void CloneBareAsCurve()
        {
            // Arrange
            var settings = new PrintProfileFFF();
            var loop = FillFactory.CreateTriangleCCW();
            loop.FillType = settings.FillTypeFactory.OuterPerimeter();
            loop.PerimeterOrder = 100;
            loop.IsHoleShell = true;
            loop.FillThickness = 3;

            // Act
            var clone = loop.CloneBareAsCurve();

            // Assert
            Assert.AreEqual(100, clone.PerimeterOrder);
            Assert.AreEqual(3, clone.FillThickness);
            Assert.IsTrue(clone.IsHoleShell);
            Assert.IsInstanceOfType(clone.FillType, typeof(OuterPerimeterFillType));
        }

        [TestMethod]
        public void SplitOnceFirstSegment()
        {
            // Act
            var result = FillFactory.CreateTriangleCCW().SplitAtDistances(new double[] { 3 }, false);

            // Assert
            Assert.AreEqual(2, result.Count);

            var curve0 = result[0];
            Assert.AreEqual(1, curve0.Elements.Count);
            Assert.AreEqual(0, curve0.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(0, curve0.Elements[0].NodeStart.y, delta);
            Assert.AreEqual(3, curve0.Elements[0].NodeEnd.x, delta);
            Assert.AreEqual(0, curve0.Elements[0].NodeEnd.y, delta);

            var curve1 = result[1];
            Assert.AreEqual(3, curve1.Elements.Count);
            Assert.AreEqual(3, curve1.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(0, curve1.Elements[0].NodeStart.y, delta);
            Assert.AreEqual(4, curve1.Elements[1].NodeStart.x, delta);
            Assert.AreEqual(0, curve1.Elements[1].NodeStart.y, delta);
            Assert.AreEqual(4, curve1.Elements[2].NodeStart.x, delta);
            Assert.AreEqual(3, curve1.Elements[2].NodeStart.y, delta);
            Assert.AreEqual(0, curve1.Elements[2].NodeEnd.x, delta);
            Assert.AreEqual(0, curve1.Elements[2].NodeEnd.y, delta);
        }

        [TestMethod]
        public void SplitOnceSecondSegment()
        {
            // Act
            var result = FillFactory.CreateTriangleCCW().SplitAtDistances(new double[] { 5 });

            // Assert
            Assert.AreEqual(2, result.Count);

            var curve0 = result[0];
            Assert.AreEqual(2, curve0.Elements.Count);
            Assert.AreEqual(0, curve0.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(0, curve0.Elements[0].NodeStart.y, delta);
            Assert.AreEqual(4, curve0.Elements[1].NodeStart.x, delta);
            Assert.AreEqual(0, curve0.Elements[1].NodeStart.y, delta);
            Assert.AreEqual(4, curve0.Elements[1].NodeEnd.x, delta);
            Assert.AreEqual(1, curve0.Elements[1].NodeEnd.y, delta);

            var curve1 = result[1];
            Assert.AreEqual(2, curve1.Elements.Count);
            Assert.AreEqual(4, curve1.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(1, curve1.Elements[0].NodeStart.y, delta);
            Assert.AreEqual(4, curve1.Elements[1].NodeStart.x, delta);
            Assert.AreEqual(3, curve1.Elements[1].NodeStart.y, delta);
            Assert.AreEqual(0, curve1.Elements[1].NodeEnd.x, delta);
            Assert.AreEqual(0, curve1.Elements[1].NodeEnd.y, delta);
        }

        [TestMethod]
        public void SplitTwiceFirstSegment()
        {
            // Act
            var result = FillFactory.CreateTriangleCCW().SplitAtDistances(new double[] { 1, 3 });

            // Assert
            Assert.AreEqual(3, result.Count);

            var curve0 = result[0];
            Assert.AreEqual(1, curve0.Elements.Count);
            Assert.AreEqual(0, curve0.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(0, curve0.Elements[0].NodeStart.y, delta);
            Assert.AreEqual(1, curve0.Elements[0].NodeEnd.x, delta);
            Assert.AreEqual(0, curve0.Elements[0].NodeEnd.y, delta);

            var curve1 = result[1];
            Assert.AreEqual(1, curve1.Elements.Count);
            Assert.AreEqual(1, curve1.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(0, curve1.Elements[0].NodeStart.y, delta);
            Assert.AreEqual(3, curve1.Elements[0].NodeEnd.x, delta);
            Assert.AreEqual(0, curve1.Elements[0].NodeEnd.y, delta);

            var curve2 = result[2];
            Assert.AreEqual(3, curve2.Elements.Count);
            Assert.AreEqual(3, curve2.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(0, curve2.Elements[0].NodeStart.y, delta);
            Assert.AreEqual(4, curve2.Elements[1].NodeStart.x, delta);
            Assert.AreEqual(0, curve2.Elements[1].NodeStart.y, delta);
            Assert.AreEqual(4, curve2.Elements[2].NodeStart.x, delta);
            Assert.AreEqual(3, curve2.Elements[2].NodeStart.y, delta);
            Assert.AreEqual(0, curve2.Elements[2].NodeEnd.x, delta);
            Assert.AreEqual(0, curve2.Elements[2].NodeEnd.y, delta);
        }

        [TestMethod]
        public void SplitTwiceFirstSegmentConnect()
        {
            // Act
            var result = FillFactory.CreateTriangleCCW().SplitAtDistances(new double[] { 1, 3 }, true);

            // Assert
            Assert.AreEqual(2, result.Count);

            var curve0 = result[0];
            Assert.AreEqual(1, curve0.Elements.Count);
            Assert.AreEqual(1, curve0.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(0, curve0.Elements[0].NodeStart.y, delta);
            Assert.AreEqual(3, curve0.Elements[0].NodeEnd.x, delta);
            Assert.AreEqual(0, curve0.Elements[0].NodeEnd.y, delta);

            var curve1 = result[1];
            Assert.AreEqual(4, curve1.Elements.Count);
            Assert.AreEqual(3, curve1.Elements[0].NodeStart.x, delta);
            Assert.AreEqual(0, curve1.Elements[0].NodeStart.y, delta);
            Assert.AreEqual(4, curve1.Elements[1].NodeStart.x, delta);
            Assert.AreEqual(0, curve1.Elements[1].NodeStart.y, delta);
            Assert.AreEqual(4, curve1.Elements[2].NodeStart.x, delta);
            Assert.AreEqual(3, curve1.Elements[2].NodeStart.y, delta);
            Assert.AreEqual(0, curve1.Elements[3].NodeStart.x, delta);
            Assert.AreEqual(0, curve1.Elements[3].NodeStart.y, delta);
            Assert.AreEqual(1, curve1.Elements[3].NodeEnd.x, delta);
            Assert.AreEqual(0, curve1.Elements[3].NodeEnd.y, delta);
        }

        [TestMethod]
        public void IsClockwise_True()
        {
            // Arrange
            var loop = FillFactory.CreateTriangleCW();

            // Act
            bool isClockwise = loop.IsClockwise();

            // Assert
            Assert.IsTrue(isClockwise);
        }

        [TestMethod]
        public void IsClockwise_False()
        {
            // Arrange
            var loop = FillFactory.CreateTriangleCCW();

            // Act
            bool isClockwise = loop.IsClockwise();

            // Assert
            Assert.IsFalse(isClockwise);
        }

        [TestMethod]
        public void RollToVertex()
        {
            // Arrange
            var loop = FillFactory.CreateTriangleCCW();

            // Act
            var rolled = (FillLoop<FillSegment>)loop.RollToVertex(1);

            // Assert
            Assert.AreEqual(loop.Elements.Count, rolled.Elements.Count);
            Assert.AreEqual(loop.Elements[0].NodeEnd, rolled.Elements[0].NodeStart);
        }

        [TestMethod]
        public void Reversed()
        {
            // Arrange
            var loop = FillFactory.CreateTriangleCCW();

            // Act
            var reversed = (FillLoop<FillSegment>)loop.Reversed();

            // Assert
            Assert.AreEqual(loop.Elements.Count, reversed.Elements.Count);
            Assert.AreEqual(loop.Elements[0].NodeStart, reversed.Elements[0].NodeStart);
            Assert.AreEqual(loop.Elements[^1].NodeEnd, reversed.Elements[^1].NodeEnd);
            Assert.IsTrue(reversed.IsClockwise());
        }
    }
}