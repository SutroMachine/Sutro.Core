using g3;
using gs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.Settings;
using System;
using System.Collections.Generic;

namespace Sutro.Core.UnitTests.gsSlicer.toolpathing
{
    [TestClass]
    public class GroupScheduler2dTests
    {
        private ToolpathSet toolpaths;
        private ToolpathSetBuilder builder;
        private SequentialScheduler2d targetScheduler;
        private GroupScheduler2d groupScheduler;

        [TestInitialize]
        public void Initialize()
        {
            var profile = new PrintProfileFFF();

            // Arrange
            toolpaths = new ToolpathSet();
            builder = new ToolpathSetBuilder(toolpaths);
            targetScheduler = new SequentialScheduler2d(builder, profile, 0);
            groupScheduler = new GroupScheduler2d(targetScheduler, new Vector2d(), profile);
        }

        [TestMethod]
        public void AddCurvesBeforeBeginGroup_Exception()
        {
            // Action
            Action act = () => { groupScheduler.AppendCurveSets(new List<FillCurveSet2d>() { new FillCurveSet2d() }); };

            // Assert
            Assert.ThrowsException<InvalidOperationException>(act);
        }

        [TestMethod]
        public void BeginGroupTwice_Exception()
        {
            // Action
            Action act = () => { groupScheduler.BeginGroup(); groupScheduler.BeginGroup(); };

            // Assert
            Assert.ThrowsException<InvalidOperationException>(act);
        }

        [TestMethod]
        public void InGroup()
        {
            Assert.IsFalse(groupScheduler.InGroup);
            groupScheduler.BeginGroup();
            Assert.IsTrue(groupScheduler.InGroup);
        }
    }
}
