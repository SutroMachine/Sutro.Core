using Sutro.Core.Models.GCode;

namespace Sutro.Core.GCodeBuilders
{
    public interface IGCodeAccumulator
    {
        void Reset();

        void AddLine(GCodeLine line);
    }
}