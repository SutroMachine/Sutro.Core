using gsGCode.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.GCodeBuilders;
using Sutro.Core.Models.GCode;
using System;

namespace gsCore.UnitTests
{
    [TestClass()]
    public class GCodeBuilderTests
    {
        [TestMethod()]
        public void AddLine()
        {
            MockGCodeAccumulator mockGCA = new MockGCodeAccumulator();
            GCodeBuilder gcb = new GCodeBuilder(mockGCA);

            GCodeLine l = new GCodeLine(100, LineType.Comment);
            l.Comment = "testComment";
            gcb.AddLine(l);
            Assert.IsTrue(mockGCA.Lines.Count == 1);
            Assert.AreEqual("testComment", mockGCA.Lines[0].Comment);
            Assert.AreEqual(LineType.Comment, mockGCA.Lines[0].Type);
            Assert.AreEqual(0, mockGCA.Lines[0].LineNumber);

            GCodeLine l2 = new GCodeLine(100, LineType.GCode);
            l2.Comment = "testComment2";
            l2.Code = 10;
            GCodeParam p = GCodeParam.Double(2.5, "X");
            l2.Parameters = new GCodeParam[1] { p };
            gcb.AddLine(l2);
            Assert.IsTrue(mockGCA.Lines.Count == 2);
            Assert.AreEqual("testComment2", mockGCA.Lines[1].Comment);
            Assert.AreEqual(10, mockGCA.Lines[1].Code);
            Assert.AreEqual(LineType.GCode, mockGCA.Lines[1].Type);
            Assert.AreEqual(1, mockGCA.Lines[1].LineNumber);
            Assert.AreEqual(p, mockGCA.Lines[1].Parameters[0]);
        }

        [TestMethod()]
        public void AddCommentLine()
        {
            MockGCodeAccumulator mockGCA = new MockGCodeAccumulator();
            GCodeBuilder gcb = new GCodeBuilder(mockGCA);

            gcb.AddCommentLine("comment");
            Assert.IsTrue(mockGCA.Lines.Count == 1);
            Assert.AreEqual("comment", mockGCA.Lines[0].Comment);
            Assert.AreEqual(LineType.Comment, mockGCA.Lines[0].Type);
            Assert.AreEqual(1, mockGCA.Lines[0].LineNumber);
        }

        [TestMethod()]
        public void AddExplicitLine()
        {
            MockGCodeAccumulator mockGCA = new MockGCodeAccumulator();
            GCodeBuilder gcb = new GCodeBuilder(mockGCA);

            gcb.AddExplicitLine("explicit");
            Assert.IsTrue(mockGCA.Lines.Count == 1);
            Assert.AreEqual("explicit", mockGCA.Lines[0].OriginalString);
            Assert.IsNull(mockGCA.Lines[0].Comment);
            Assert.AreEqual(LineType.UnknownString, mockGCA.Lines[0].Type);
            Assert.AreEqual(1, mockGCA.Lines[0].LineNumber);
        }

        [TestMethod()]
        public void BeginAndEndGLine()
        {
            MockGCodeAccumulator mockGCA = new MockGCodeAccumulator();
            GCodeBuilder gcb = new GCodeBuilder(mockGCA);

            gcb.BeginGLine(1, "comment");
            Assert.IsTrue(mockGCA.Lines.Count == 0);

            gcb.EndLine();
            Assert.IsTrue(mockGCA.Lines.Count == 1);
            Assert.AreEqual("comment", mockGCA.Lines[0].Comment);
            Assert.AreEqual(LineType.GCode, mockGCA.Lines[0].Type);
            Assert.AreEqual(0, mockGCA.Lines[0].LineNumber);
        }

        [TestMethod()]
        public void BeginAndEndMLine()
        {
            MockGCodeAccumulator mockGCA = new MockGCodeAccumulator();
            GCodeBuilder gcb = new GCodeBuilder(mockGCA);

            gcb.BeginMLine(1, "comment");
            Assert.IsTrue(mockGCA.Lines.Count == 0);

            gcb.EndLine();
            Assert.IsTrue(mockGCA.Lines.Count == 1);
            Assert.AreEqual("comment", mockGCA.Lines[0].Comment);
            Assert.AreEqual(LineType.MCode, mockGCA.Lines[0].Type);
            Assert.AreEqual(0, mockGCA.Lines[0].LineNumber);
        }

        [TestMethod()]
        public void BeginGLineAndAppend()
        {
            MockGCodeAccumulator mockGCA = new MockGCodeAccumulator();
            GCodeBuilder gcb = new GCodeBuilder(mockGCA);

            gcb.BeginGLine(1, "comment");
            Assert.IsTrue(mockGCA.Lines.Count == 0);

            gcb.AppendComment("addingComment");
            gcb.AppendI("intTest", 3);
            gcb.AppendF("floatTest", 2.3);
            gcb.AppendS("stringTest", "stringTest");
            gcb.AppendL("labelTest");

            gcb.EndLine();
            Assert.IsTrue(mockGCA.Lines.Count == 1);
            Assert.AreEqual("addingComment", mockGCA.Lines[0].Comment);
            Assert.AreEqual(LineType.GCode, mockGCA.Lines[0].Type);
            Assert.AreEqual(0, mockGCA.Lines[0].LineNumber);

            var par = mockGCA.Lines[0].Parameters;
            Assert.AreEqual(4, par.Length);
            Assert.AreEqual(GCodeParamTypes.IntegerValue, par[0].Type);
            Assert.AreEqual(3, par[0].IntegerValue);
            Assert.AreEqual("intTest", par[0].Identifier);
            Assert.AreEqual(GCodeParamTypes.DoubleValue, par[1].Type);
            Assert.AreEqual(2.3, par[1].DoubleValue);
            Assert.AreEqual("floatTest", par[1].Identifier);
            Assert.AreEqual(GCodeParamTypes.TextValue, par[2].Type);
            Assert.AreEqual("stringTest", par[2].TextValue);
            Assert.AreEqual("stringTest", par[2].Identifier);
            Assert.AreEqual(GCodeParamTypes.NoValue, par[3].Type);
            Assert.AreEqual("labelTest", par[3].Identifier);
        }

        [TestMethod()]
        [ExpectedException(typeof(Exception))]
        public void AddCommentToNullLine()
        {
            MockGCodeAccumulator mockGCA = new MockGCodeAccumulator();
            GCodeBuilder gcb = new GCodeBuilder(mockGCA);

            gcb.AppendComment("addingComment");
        }
    }
}