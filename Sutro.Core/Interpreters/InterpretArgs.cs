using Sutro.Core.Models.GCode;

namespace Sutro.Core.Interpreters
{
    public struct InterpretArgs
    {
        public LineType eTypeFilter;

        public bool HasTypeFilter { get { return eTypeFilter != LineType.Blank; } }

        public static readonly InterpretArgs Default = new InterpretArgs()
        {
            eTypeFilter = LineType.Blank
        };
    }
}