using Sutro.Core.Toolpaths;

namespace Sutro.Core.SLS
{
    public interface IPathsAssembler
    {
        void AppendPaths(IToolpathSet paths);

        // [TODO] we should replace this w/ a separte assembler/builder, even if the assembler is trivial!!
        ToolpathSet TempGetAssembledPaths();
    }
}