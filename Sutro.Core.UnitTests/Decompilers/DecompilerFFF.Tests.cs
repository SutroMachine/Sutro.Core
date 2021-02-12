using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.Decompilers;
using Sutro.Core.Models.GCode;

namespace Sutro.Core.UnitTests.Decompilers
{
    [TestClass()]
    public class DecompilerFFFTests : DecompilerFFF
    {
        [TestMethod()]
        public void NewLayerComment()
        {
            var line = new GCodeLine(0, LineType.Comment);
            line.Comment = "layer 99, Z = 33.33";

            var isNewLine = LineIsNewLayerComment(line, out int index, out double height);

            Assert.IsTrue(isNewLine);
            Assert.AreEqual(99, index);
            Assert.AreEqual(33.33, height);
        }

        [TestMethod()]
        public void NewLayerCommentAlternate()
        {
            var line = new GCodeLine(0, LineType.Comment);
            line.Comment = "layer 99: 33.33mm";

            var isNewLine = LineIsNewLayerComment(line, out int index, out double height);

            Assert.IsTrue(isNewLine);
            Assert.AreEqual(99, index);
            Assert.AreEqual(33.33, height);
        }

        [TestMethod()]
        public void NewLayerUnknown()
        {
            var line = new GCodeLine(0, LineType.UnknownString);
            line.OriginalString = "; layer 99, Z = 33.33";

            var isNewLine = LineIsNewLayerComment(line, out int index, out double height);

            Assert.IsTrue(isNewLine);
            Assert.AreEqual(99, index);
            Assert.AreEqual(33.33, height);
        }

        [TestMethod()]
        public void NotNewLayer_Comment()
        {
            var line = new GCodeLine(0, LineType.GCode);
            line.Code = 1;
            line.Parameters = new GCodeParam[] { GCodeParam.Double(200, "X") };
            line.Comment = "layer";

            var isNewLine = LineIsNewLayerComment(line, out int index, out double height);

            Assert.IsFalse(isNewLine);
        }

        [TestMethod()]
        public void NotNewLayer_NoComment()
        {
            var line = new GCodeLine(0, LineType.GCode);
            line.Code = 1;
            line.Parameters = new GCodeParam[] { GCodeParam.Double(200, "X") };

            var isNewLine = LineIsNewLayerComment(line, out int index, out double height);

            Assert.IsFalse(isNewLine);
        }
    }
}