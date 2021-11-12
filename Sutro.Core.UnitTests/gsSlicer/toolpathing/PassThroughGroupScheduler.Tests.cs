using g3;
using gs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.Settings;
using System.Collections.Generic;

namespace Sutro.Core.UnitTests.gsSlicer.toolpathing
{
    [TestClass]
    public class PassThroughGroupSchedulerTests
    {
        [TestMethod]
        public void Basic()
        {
            // Arrange
            var toolpaths = new ToolpathSet();
            var builder = new ToolpathSetBuilder(toolpaths);
            var targetScheduler = new SequentialScheduler2d(builder, new PrintProfileFFF(), 0);
            var groupScheduler = new PassThroughGroupScheduler(targetScheduler);
 
            var fill = new FillCurve<FillSegment>();
            fill.BeginCurve(Vector2d.Zero);
            fill.AddToCurve(Vector2d.One);
            var curveSets = new List<FillCurveSet2d>() { new FillCurveSet2d() };
            curveSets[0].Curves.Add(fill);
            
            // Action
            groupScheduler.AppendCurveSets(curveSets);

            groupScheduler.BeginGroup();
            Assert.IsFalse(groupScheduler.InGroup);
            groupScheduler.EndGroup();
            Assert.IsFalse(groupScheduler.InGroup);

            // Assert
            //builder.Paths.
        }
    }
}
