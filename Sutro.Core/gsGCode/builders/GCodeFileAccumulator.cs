using Sutro.Core.Models.GCode;

namespace gs
{
    public class GCodeFileAccumulator : IGCodeAccumulator
    {
        public GCodeFile File;

        public GCodeFileAccumulator()
        {
            Reset();
        }

        public virtual void AddLine(GCodeLine line)
        {
            File.AppendLine(line);
        }

        public void Reset()
        {
            File = new GCodeFile();
        }
    }
}