using g3;
using gs;
using gs.FillTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Sutro.Core.UnitTests.gsSlicer.toolpathing
{
    [TestClass]
    public class ShellsFillPolygonTests
    {
        [TestMethod]
        public void ThinFeaturesTest()
        {
            var poly = new GeneralPolygon2d(Polygon2d.MakeRectangle(Vector2d.Zero, 2, 10));
            var shellsFill = new ShellsFillPolygon(poly, new InnerPerimeterFillType())
            {
                EnableThinCheck = true,
                InsetFromInputPolygonX = 0,
                InsetInnerPolygons = false,
                Layers = 10,
                PreserveOuterShells = false,
                SelfOverlapTolerance = 0.5,
                ToolWidth = 0.5,
                ToolWidthClipMultiplier = 0.75,
            };
            shellsFill.Compute();

            var shells = shellsFill.GetFillCurves();

            Assert.AreEqual(3, shells.Count);
        }       
    }
}