using g3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.Slicing;
using System;
using System.Collections.Generic;

namespace Sutro.Core.UnitTests.Slicing
{
    [TestClass()]
    public class PlanarSliceTests
    {
        [TestMethod()]
        public void AddPolygonTest()
        {
            PlanarSlice slice = new PlanarSlice();
            Polygon2d p2d = new Polygon2d(new List<Vector2d>(){
                new Vector2d(1,0),
                new Vector2d(0,1),
                new Vector2d(1,1)
            });
            GeneralPolygon2d poly = new GeneralPolygon2d(p2d);

            slice.AddPolygon(poly);

            Assert.IsTrue(slice.InputSolids[0] == poly);
        }

        [TestMethod()]
        public void AddPolygonsTest()
        {
            PlanarSlice slice = new PlanarSlice();
            Polygon2d p2d = new Polygon2d(new List<Vector2d>(){
                new Vector2d(1,0),
                new Vector2d(0,1),
                new Vector2d(1,1)
            });
            GeneralPolygon2d poly = new GeneralPolygon2d(p2d);
            Polygon2d p2d2 = new Polygon2d(new List<Vector2d>(){
                new Vector2d(-1,0),
                new Vector2d(0,-1),
                new Vector2d(-1,-1)
            });
            GeneralPolygon2d poly2 = new GeneralPolygon2d(p2d2);

            slice.AddPolygons(new List<GeneralPolygon2d>() { poly, poly2 });

            Assert.AreEqual(slice.InputSolids[0], poly);
            Assert.AreEqual(slice.InputSolids[1], poly2);
        }

        [TestMethod()]
        public void AddEmbeddedPathTest()
        {
            PlanarSlice slice = new PlanarSlice();
            PolyLine2d line = new PolyLine2d(new List<Vector2d>()
            {
                new Vector2d(1,0),
                new Vector2d(-1,0)
            });
            slice.AddEmbeddedPath(line);

            Assert.AreEqual(slice.EmbeddedPaths[0], line);
        }

        [TestMethod()]
        public void AddClippedPathTest()
        {
            PlanarSlice slice = new PlanarSlice();
            PolyLine2d line = new PolyLine2d(new List<Vector2d>()
            {
                new Vector2d(1,0),
                new Vector2d(-1,0)
            });
            slice.AddClippedPath(line);

            Assert.AreEqual(slice.ClippedPaths[0], line);
        }

        [TestMethod()]
        public void AddSupportPolygonTest()
        {
            PlanarSlice slice = new PlanarSlice();
            Polygon2d p2d = new Polygon2d(new List<Vector2d>(){
                new Vector2d(1,0),
                new Vector2d(0,1),
                new Vector2d(1,1)
            });
            GeneralPolygon2d poly = new GeneralPolygon2d(p2d);

            slice.AddSupportPolygon(poly);

            Assert.IsTrue(slice.SupportSolids[0] == poly);
        }

        [TestMethod()]
        public void AddSupportPolygonsTest()
        {
            PlanarSlice slice = new PlanarSlice();
            Polygon2d p2d = new Polygon2d(new List<Vector2d>(){
                new Vector2d(1,0),
                new Vector2d(0,1),
                new Vector2d(1,1)
            });
            GeneralPolygon2d poly = new GeneralPolygon2d(p2d);
            Polygon2d p2d2 = new Polygon2d(new List<Vector2d>(){
                new Vector2d(-1,0),
                new Vector2d(0,-1),
                new Vector2d(-1,-1)
            });
            GeneralPolygon2d poly2 = new GeneralPolygon2d(p2d2);

            slice.AddSupportPolygons(new List<GeneralPolygon2d>() { poly, poly2 });

            Assert.AreEqual(slice.SupportSolids[0], poly);
            Assert.AreEqual(slice.SupportSolids[1], poly2);
        }

        [TestMethod()]
        public void AddCavityPolygonTest()
        {
            PlanarSlice slice = new PlanarSlice();
            Polygon2d p2d = new Polygon2d(new List<Vector2d>(){
                new Vector2d(1,0),
                new Vector2d(0,1),
                new Vector2d(1,1)
            });
            GeneralPolygon2d poly = new GeneralPolygon2d(p2d);

            slice.AddCavityPolygon(poly);

            Assert.IsTrue(slice.InputCavities[0] == poly);
        }

        [TestMethod()]
        public void AddCavityPolygonsTest()
        {
            PlanarSlice slice = new PlanarSlice();
            Polygon2d p2d = new Polygon2d(new List<Vector2d>(){
                new Vector2d(1,0),
                new Vector2d(0,1),
                new Vector2d(1,1)
            });
            GeneralPolygon2d poly = new GeneralPolygon2d(p2d);
            Polygon2d p2d2 = new Polygon2d(new List<Vector2d>(){
                new Vector2d(-1,0),
                new Vector2d(0,-1),
                new Vector2d(-1,-1)
            });
            GeneralPolygon2d poly2 = new GeneralPolygon2d(p2d2);

            slice.AddCavityPolygons(new List<GeneralPolygon2d>() { poly, poly2 });

            Assert.AreEqual(slice.InputCavities[0], poly);
            Assert.AreEqual(slice.InputCavities[1], poly2);
        }

        [TestMethod()]
        public void AddCropRegionTest()
        {
            PlanarSlice slice = new PlanarSlice();
            Polygon2d p2d = new Polygon2d(new List<Vector2d>(){
                new Vector2d(1,0),
                new Vector2d(0,1),
                new Vector2d(1,1)
            });
            GeneralPolygon2d poly = new GeneralPolygon2d(p2d);

            slice.AddCropRegion(poly);

            Assert.IsTrue(slice.InputCropRegions[0] == poly);
        }

        [TestMethod()]
        public void AddCropRegionsTest()
        {
            PlanarSlice slice = new PlanarSlice();
            Polygon2d p2d = new Polygon2d(new List<Vector2d>(){
                new Vector2d(1,0),
                new Vector2d(0,1),
                new Vector2d(1,1)
            });
            GeneralPolygon2d poly = new GeneralPolygon2d(p2d);
            Polygon2d p2d2 = new Polygon2d(new List<Vector2d>(){
                new Vector2d(-1,0),
                new Vector2d(0,-1),
                new Vector2d(-1,-1)
            });
            GeneralPolygon2d poly2 = new GeneralPolygon2d(p2d2);

            slice.AddCropRegions(new List<GeneralPolygon2d>() { poly, poly2 });

            Assert.AreEqual(slice.InputCropRegions[0], poly);
            Assert.AreEqual(slice.InputCropRegions[1], poly2);
        }

        [TestMethod()]
        public void ResolveTest()
        {
            PlanarSlice slice = new PlanarSlice();
            Polygon2d p2d = new Polygon2d(new List<Vector2d>(){
                new Vector2d(1,0),
                new Vector2d(0,1),
                new Vector2d(1,1)
            });
            GeneralPolygon2d poly = new GeneralPolygon2d(p2d);
            Polygon2d p2d2 = new Polygon2d(new List<Vector2d>(){
                new Vector2d(-1,0),
                new Vector2d(0,-1),
                new Vector2d(-1,-1)
            });
            GeneralPolygon2d poly2 = new GeneralPolygon2d(p2d2);

            slice.AddPolygons(new List<GeneralPolygon2d>() { poly, poly2 });

            Assert.AreEqual(slice.InputSolids[0], poly);
            Assert.AreEqual(slice.InputSolids[1], poly2);

            PolyLine2d line = new PolyLine2d(new List<Vector2d>()
            {
                new Vector2d(1,0),
                new Vector2d(-1,0)
            });
            slice.AddEmbeddedPath(line);
            slice.EmbeddedPathWidth = 1;

            Polygon2d supportp2d = new Polygon2d(new List<Vector2d>(){
                new Vector2d(1,0),
                new Vector2d(0,1),
                new Vector2d(1,1)
            });
            GeneralPolygon2d supportpoly = new GeneralPolygon2d(supportp2d);
            Polygon2d supportp2d2 = new Polygon2d(new List<Vector2d>(){
                new Vector2d(-1,0),
                new Vector2d(0,-1),
                new Vector2d(-1,-1)
            });
            GeneralPolygon2d supportpoly2 = new GeneralPolygon2d(supportp2d2);

            slice.AddSupportPolygons(new List<GeneralPolygon2d>() { supportpoly, supportpoly2 });

            Polygon2d cavp2d = new Polygon2d(new List<Vector2d>(){
                new Vector2d(1,0),
                new Vector2d(0,1),
                new Vector2d(1,1)
            });
            GeneralPolygon2d cavpoly = new GeneralPolygon2d(cavp2d);
            Polygon2d cavp2d2 = new Polygon2d(new List<Vector2d>(){
                new Vector2d(-1,0),
                new Vector2d(0,-1),
                new Vector2d(-1,-1)
            });
            GeneralPolygon2d cavpoly2 = new GeneralPolygon2d(cavp2d2);

            slice.AddCavityPolygons(new List<GeneralPolygon2d>() { cavpoly, cavpoly2 });

            Polygon2d cropp2d = new Polygon2d(new List<Vector2d>(){
                new Vector2d(1,0),
                new Vector2d(0,1),
                new Vector2d(1,1)
            });
            GeneralPolygon2d croppoly = new GeneralPolygon2d(cropp2d);
            Polygon2d cropp2d2 = new Polygon2d(new List<Vector2d>(){
                new Vector2d(-1,0),
                new Vector2d(0,-1),
                new Vector2d(-1,-1)
            });
            GeneralPolygon2d croppoly2 = new GeneralPolygon2d(cropp2d2);

            slice.AddCropRegions(new List<GeneralPolygon2d>() { croppoly, croppoly2 });

            slice.Resolve();

            slice.BuildSpatialCaches();

            double dist = slice.DistanceSquared(new Vector2d(5, 5));
            Assert.AreEqual(41, dist);
        }

        [TestMethod()]
        public void ResolveEmbeddedPathWidthExceptionTest()
        {
            PlanarSlice slice = new PlanarSlice();
            PolyLine2d line = new PolyLine2d(new List<Vector2d>()
            {
                new Vector2d(1,0),
                new Vector2d(-1,0)
            });
            slice.AddEmbeddedPath(line);

            Assert.ThrowsException<Exception>(() => slice.Resolve());
        }

        [TestMethod()]
        public void DistanceSquaredExceptionTest()
        {
            PlanarSlice slice = new PlanarSlice();
            Assert.ThrowsException<Exception>(() => slice.DistanceSquared(new Vector2d(0, 0)));
        }
    }
}