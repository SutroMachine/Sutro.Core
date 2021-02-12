using g3;

namespace Sutro.Core.Logging
{
    public interface IGraphLogger
    {
        void LogGraph(DGraph2 graph, string identifier);
    }
}