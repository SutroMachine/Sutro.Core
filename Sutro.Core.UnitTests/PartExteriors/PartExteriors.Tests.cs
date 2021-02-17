using g3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.PartExteriors;
using Sutro.Core.Slicing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sutro.Core.UnitTests.PartExteriors
{
    [TestClass]
    public class PartExteriorsTests
    {
        private PlanarSliceStack CreateCircleStack()
        {
            var slices = new PlanarSliceStack();
            for (int i = 0; i < 1000; i++)
            {
                slices.Add(new PlanarSlice()
                {
                    LayerIndex = i,
                    LayerZSpan = new Interval1d(i * 0.2, (i + 1) * 0.2),
                    Solids = new List<GeneralPolygon2d>(){
                       new GeneralPolygon2d(Polygon2d.MakeCircle(10, 1000))
                    },
                });
            }
            return slices;
        }

        private List<GeneralPolygon2d> CreateSquare()
        {
            return new List<GeneralPolygon2d>()
            {
                new GeneralPolygon2d(Polygon2d.MakeRectangle(Vector2d.Zero, Vector2d.One)),
            };
        }

        [TestMethod]
        public void CancellationTest()
        {
            var slices = CreateCircleStack();

            var partExterior = new PartExteriorVerticalProjection(slices, -1, 0, 3, 3);
            var cts = new CancellationTokenSource();

            // Run a task so that we can cancel from another thread.
            var a = Task.Factory.StartNew(() =>
            {
                partExterior.Initialize(cts.Token);
            });

            cts.Cancel();

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                partExterior.GetExteriorRegions(0, new List<GeneralPolygon2d>());
            });
        }

        [TestMethod]
        public void FullyHollowTest()
        {
            var partExterior = new PartExteriorFullyHollow();
            partExterior.Initialize(CancellationToken.None);

            var exterior = partExterior.GetExteriorRegions(0, CreateSquare());
            Assert.AreEqual(0, exterior.Count);
        }

        [TestMethod]
        public void FullySolidTest()
        {
            var partExterior = new PartExteriorFullySolid();
            partExterior.Initialize(CancellationToken.None);

            var exterior = partExterior.GetExteriorRegions(0, CreateSquare());
            Assert.AreEqual(1, exterior.Count);
            Assert.AreEqual(1, exterior[0].Area);
        }

        private PlanarSlice SliceConstructor(int i, Polygon2d poly = null)
        {
            double halfWidth = 7;
            double halfHeight = 2;

            var gpoly = new GeneralPolygon2d(Polygon2d.MakeRectangle(
                new Vector2d(-halfWidth, -halfHeight), new Vector2d(halfWidth, halfHeight)));

            if (poly != null)
                gpoly.AddHole(poly, true, true);

            return new PlanarSlice()
            {
                LayerIndex = i,
                Solids = new List<GeneralPolygon2d>() { gpoly },
            };
        }

        private Polygon2d HoleConstructor(double xCenter)
        {
            double halfWidth = 1;
            double halfHeight = 1;

            var poly = Polygon2d.MakeRectangle(
                new Vector2d(xCenter - halfWidth, -halfHeight),
                new Vector2d(xCenter + halfWidth, halfHeight));

            poly.Reverse();
            return poly;
        }

        [TestMethod]
        public void VerticalProjectionTest()
        {
            var slices = new PlanarSliceStack();
            slices.Add(SliceConstructor(0, HoleConstructor(-5)));
            slices.Add(SliceConstructor(1, HoleConstructor(-2)));
            slices.Add(SliceConstructor(2));
            slices.Add(SliceConstructor(3, HoleConstructor(2)));
            slices.Add(SliceConstructor(4, HoleConstructor(5)));

            var partExterior = new PartExteriorVerticalProjection(slices, -1, 0.2, 2, 2);
            partExterior.Initialize(CancellationToken.None);

            var exterior = partExterior.GetExteriorRegions(2, slices[2].Solids);

            Assert.AreEqual(5, exterior.Count);

            double actualArea = exterior.Sum(gpoly => gpoly.Area);
            double expectedArea = 0;
            expectedArea += 4 * (7 * 2); // Add outer ring
            expectedArea -= 4 * ((7 - 0.2) * (2 - 0.2)); // Subtract inner ring
            expectedArea += 4 * 4 * (1.2 * 1.2); // Add hole coverage

            Assert.AreEqual(expectedArea, actualArea, 1e-6);
        }
    }
}