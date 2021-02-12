using Sutro.Core.Models.GCode;

namespace Sutro.Core.Interpreters
{
    public interface IGCodeInterpreter
    {
        void AddListener(IGCodeListener listener);

        void Interpret(GCodeFile file, InterpretArgs args);
    }
}