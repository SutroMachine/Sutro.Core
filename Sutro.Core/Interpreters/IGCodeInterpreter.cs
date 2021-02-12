using Sutro.Core.Models.GCode;

namespace gs
{
    public interface IGCodeInterpreter
    {
        void AddListener(IGCodeListener listener);

        void Interpret(GCodeFile file, InterpretArgs args);
    }
}