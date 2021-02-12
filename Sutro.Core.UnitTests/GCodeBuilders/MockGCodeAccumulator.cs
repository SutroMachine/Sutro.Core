using Sutro.Core.GCodeBuilders;
using Sutro.Core.Models.GCode;
using System.Collections.Generic;

namespace Sutro.Core.UnitTests.GCodeBuilders
{
    public class MockGCodeAccumulator : IGCodeAccumulator
    {
        public List<GCodeLine> Lines { get; }

        public MockGCodeAccumulator() => Lines = new List<GCodeLine>();

        public void AddLine(GCodeLine line)
        {
            Lines.Add(line);
        }

        public void Reset()
        {
            Lines.Clear();
        }
    }
}