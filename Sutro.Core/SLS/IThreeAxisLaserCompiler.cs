using Sutro.Core.Toolpaths;

namespace Sutro.Core.SLS
{
    public interface IThreeAxisLaserCompiler
    {
        void Begin();

        void AppendPaths(ToolpathSet paths);

        void End();
    }
}