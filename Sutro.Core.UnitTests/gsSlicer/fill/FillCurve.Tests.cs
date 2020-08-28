using g3;
using gs.FillTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.Settings;
using Sutro.Core.Settings.Info;
using System;

namespace gs.UnitTests.Fill
{
    [TestClass]
    public class FillCurveTests
    {
        private const double tolerance = 1e-6;

        private static Vector2d[] CreateSimpleVector2dArray()
        {
            return new Vector2d[]
            {
                new Vector2d(0,0),
                new Vector2d(1,0),
                new Vector2d(1,2),
            };
        }

        private static FillCurve<FillSegment> CreateSimpleFillCurve()
        {
            var vertices = new Vector2d[]
            {
                new Vector2d(0, 0),
                new Vector2d(1, 0),
                new Vector2d(3, 0),
                new Vector2d(4, 0),
            };
            var curve = new FillCurve<FillSegment>(vertices);
            return curve;
        }

        [TestMethod]
        public void Constructor_FromVertices()
        {
            // Act
            var curve = new FillCurve<FillSegment>(
                CreateSimpleVector2dArray());

            // Assert
            Assert.AreEqual(2, curve.Elements.Count);
        }

        [TestMethod]
        public void Constructor_FromPolyLine()
        {
            // Arrange
            var polyline = new PolyLine2d(CreateSimpleVector2dArray());

            // Act
            var curve = new FillCurve<FillSegment>(polyline);

            // Assert
            Assert.AreEqual(2, curve.Elements.Count);
        }

        [TestMethod]
        public void Constructor_FromPoints()
        {
            // Arrange
            var curve = new FillCurve<FillSegment>();

            // Act
            curve.BeginCurve(new Vector2d(0, 0));
            curve.AddToCurve(new Vector3d(1, 0, 0));
            curve.AddToCurve(new Vector2d(1, 2), new FillSegment(true));

            // Assert
            Assert.AreEqual(2, curve.Elements.Count);
            Assert.IsTrue(curve.Elements[1].Edge.IsConnector);
        }

        [TestMethod]
        public void Constructor_FromPoints_ExceptionOnAddBeforeBegin()
        {
            // Arrange
            var curve = new FillCurve<FillSegment>();

            // Act & Assert
            Assert.ThrowsException<MethodAccessException>(() =>
            {
                curve.AddToCurve(new Vector2d(1, 0));
            });
        }

        [TestMethod]
        public void Constructor_FromPoints_ExceptionOnBeginTwice()
        {
            // Arrange
            var curve = new FillCurve<FillSegment>();

            curve.BeginCurve(new Vector2d(0, 0));

            // Act & Assert
            Assert.ThrowsException<MethodAccessException>(() =>
            {
                curve.BeginCurve(new Vector2d(1, 0));
            });
        }

        [TestMethod]
        public void CloneBare()
        {
            // Arrange
            var settings = new GenericRepRapSettings();
            var curve = new FillCurve<FillSegment>()
            {
                FillType = settings.FillTypeFactory.OuterPerimeter(),
                PerimeterOrder = 100,
                IsHoleShell = true,
                FillThickness = 3
            };

            // Act
            var clone = curve.CloneBare();

            // Assert
            Assert.AreEqual(100, clone.PerimeterOrder);
            Assert.AreEqual(3, clone.FillThickness);
            Assert.IsTrue(clone.IsHoleShell);
            Assert.IsInstanceOfType(clone.FillType, typeof(OuterPerimeterFillType));
        }

        [TestMethod]
        public void CloseCurve()
        {
            // Arrange
            var curve = new FillCurve<FillSegment>(
                CreateSimpleVector2dArray());

            // Act
            var loop = curve.CloseCurve();

            // Assert
            Assert.AreEqual(3, loop.Elements.Count);
            Assert.AreEqual(curve.Entry, loop.Entry);
        }

        [TestMethod]
        public void Reversed()
        {
            // Arrange
            var curve = new FillCurve<FillSegment>(
                CreateSimpleVector2dArray());

            // Act
            var reversed = curve.Reversed();

            // Assert
            Assert.AreEqual(2, curve.Elements.Count);
            Assert.AreEqual(curve.Entry, reversed.Exit);
            Assert.AreEqual(curve.Exit, reversed.Entry);
        }

        [TestMethod]
        public void Extend_Success()
        {
            // Arrange
            var curve1 = new FillCurve<FillSegment>(new Vector2d[]
            {
                new Vector2d(0,0),
                new Vector2d(1,0),
            });
            var curve2 = new FillCurve<FillSegment>(new Vector2d[]
            {
                new Vector2d(1,0),
                new Vector2d(2,1),
                new Vector2d(3,3),
            });

            // Act
            curve1.Extend(curve2.Elements);

            // Assert
            Assert.AreEqual(3, curve1.Elements.Count);
            Assert.AreEqual(new Vector2d(0, 0), curve1.Entry);
            Assert.AreEqual(curve2.Exit, curve1.Exit);
        }

        [TestMethod]
        public void Extend_ExceptionOnDiscontinuity()
        {
            // Arrange
            var curve1 = new FillCurve<FillSegment>(new Vector2d[]
            {
                new Vector2d(0,0),
                new Vector2d(1,0),
            });
            var curve2 = new FillCurve<FillSegment>(new Vector2d[]
            {
                new Vector2d(2,0),
                new Vector2d(3,0),
            });

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() =>
            {
                curve1.Extend(curve2.Elements);
            });
        }

        [TestMethod]
        public void SplitAtDistances_MiddleSegment()
        {
            // Arrange
            var curve = CreateSimpleFillCurve();

            // Act
            var splitCurves = curve.SplitAtDistances(new double[] { 2 });

            // Assert

            Assert.AreEqual(2, splitCurves.Count);

            var split1 = (FillCurve<FillSegment>)splitCurves[0];
            Assert.AreEqual(2, split1.Elements.Count);
            Assert.AreEqual(curve.Elements[0].NodeStart, split1.Elements[0].NodeStart);
            Assert.AreEqual(curve.Elements[0].NodeEnd, split1.Elements[1].NodeStart);
            Assert.AreEqual(new Vector3d(2, 0, 0), split1.Elements[1].NodeEnd);

            var split2 = (FillCurve<FillSegment>)splitCurves[1];
            Assert.AreEqual(2, split2.Elements.Count);
            Assert.AreEqual(new Vector3d(2, 0, 0), split2.Elements[0].NodeStart);
            Assert.AreEqual(curve.Elements[^1].NodeStart, split2.Elements[1].NodeStart);
            Assert.AreEqual(curve.Elements[^1].NodeEnd, split2.Elements[1].NodeEnd);
        }

        [TestMethod]
        public void SplitAtDistances_OnFirstVertex()
        {
            // Arrange
            var curve = CreateSimpleFillCurve();

            // Act
            var splitCurves = curve.SplitAtDistances(new double[] { 0 });

            // Assert

            Assert.AreEqual(1, splitCurves.Count);

            var split1 = (FillCurve<FillSegment>)splitCurves[0];
            Assert.AreEqual(3, split1.Elements.Count);
            Assert.AreEqual(curve.Elements[0].NodeStart, split1.Elements[0].NodeStart);
            Assert.AreEqual(curve.Elements[1].NodeStart, split1.Elements[1].NodeStart);
            Assert.AreEqual(curve.Elements[2].NodeStart, split1.Elements[2].NodeStart);
            Assert.AreEqual(curve.Elements[2].NodeEnd, split1.Elements[2].NodeEnd);
        }

        [TestMethod]
        public void SplitAtDistances_EmptySplitDistanceInput()
        {
            // Arrange
            var curve = CreateSimpleFillCurve();

            // Act
            var splitCurves = curve.SplitAtDistances(new double[] { });

            // Assert

            Assert.AreEqual(1, splitCurves.Count);

            var split1 = (FillCurve<FillSegment>)splitCurves[0];
            Assert.AreEqual(3, split1.Elements.Count);
            Assert.AreEqual(curve.Elements[0].NodeStart, split1.Elements[0].NodeStart);
            Assert.AreEqual(curve.Elements[1].NodeStart, split1.Elements[1].NodeStart);
            Assert.AreEqual(curve.Elements[2].NodeStart, split1.Elements[2].NodeStart);
            Assert.AreEqual(curve.Elements[2].NodeEnd, split1.Elements[2].NodeEnd);
        }

        [TestMethod]
        public void TrimFront()
        {
            // Arrange
            var curve = CreateSimpleFillCurve();

            // Act
            var trimmed = (FillCurve<FillSegment>)curve.TrimFront(2d);

            // Assert
            Assert.AreEqual(2, trimmed.Elements.Count);
            Assert.AreEqual(2, trimmed.Elements[0].NodeStart.x, tolerance);
            Assert.AreEqual(3, trimmed.Elements[1].NodeStart.x, tolerance);
            Assert.AreEqual(4, trimmed.Elements[1].NodeEnd.x, tolerance);
        }

        [TestMethod]
        public void TrimBack()
        {
            // Arrange
            var curve = CreateSimpleFillCurve();

            // Act
            var trimmed = (FillCurve<FillSegment>)curve.TrimBack(1.5d);

            // Assert
            Assert.AreEqual(2, trimmed.Elements.Count);
            Assert.AreEqual(0, trimmed.Elements[0].NodeStart.x, tolerance);
            Assert.AreEqual(1, trimmed.Elements[1].NodeStart.x, tolerance);
            Assert.AreEqual(2.5, trimmed.Elements[1].NodeEnd.x, tolerance);
        }

        [TestMethod]
        public void TrimFrontAndBack()
        {
            // Arrange
            var curve = CreateSimpleFillCurve();

            // Act
            var trimmed = (FillCurve<FillSegment>)curve.TrimFrontAndBack(0.5d, 0.75d);

            // Assert
            Assert.AreEqual(3, trimmed.Elements.Count);
            Assert.AreEqual(0.5, trimmed.Elements[0].NodeStart.x, tolerance);
            Assert.AreEqual(1, trimmed.Elements[1].NodeStart.x, tolerance);
            Assert.AreEqual(3, trimmed.Elements[2].NodeStart.x, tolerance);
            Assert.AreEqual(3.25, trimmed.Elements[2].NodeEnd.x, tolerance);
        }

        [TestMethod]
        public void TrimFront_ExceptionOnTrimDistanceExceedsCurveLength()
        {
            // Arrange
            var curve = CreateSimpleFillCurve();

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() =>
            {
                curve.TrimFront(5);
            });
        }

        [TestMethod]
        public void TrimFront_NegativeDistance()
        {
            // Arrange
            var curve = CreateSimpleFillCurve();

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() =>
            {
                curve.TrimFront(-1);
            });
        }

        [TestMethod]
        public void TrimFrontAndBack_ExceptionOnTrimDistanceExceedsCurveLength()
        {
            // Arrange
            var curve = CreateSimpleFillCurve();

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() =>
            {
                curve.TrimFrontAndBack(3);
            });
        }
    }
}