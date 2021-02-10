using Sutro.Core.Models.GCode;

namespace gs
{
    public interface IGCodeAccumulator
    {
        void Reset();
        void AddLine(GCodeLine line);
    }
}