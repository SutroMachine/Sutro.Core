using gs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.Models.GCode;
using Sutro.Core.Parsers;

namespace Sutro.Core.UnitTests.Parsers
{
    [TestClass]
    public class GenericGCodeParserTests
    {
        [TestMethod]
        public void ParsesNCodeOnly()
        {
            var parser = new GenericGCodeParser();
            var line = parser.ParseLine("N100", 0);
            Assert.AreEqual(LineType.Blank, line.Type);
        }

        [TestMethod]
        public void ParsesNCodeWithGCode()
        {
            var parser = new GenericGCodeParser();
            var line = parser.ParseLine("N100 G1", 0);
            Assert.AreEqual(LineType.GCode, line.Type);
        }

        [TestMethod]
        [DataRow("NumberOfThings")]
        [DataRow("Number Of Things")]
        public void ParsesUnknownString(string s)
        {
            var parser = new GenericGCodeParser();
            var line = parser.ParseLine(s, 0);
            Assert.AreEqual(LineType.UnknownString, line.Type);
        }
    }
}