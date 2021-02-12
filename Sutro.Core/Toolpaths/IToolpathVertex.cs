using g3;

namespace Sutro.Core.Toolpaths
{
    public interface IToolpathVertex
    {
        Vector3d Position { get; set; }
        double FeedRate { get; set; }
        TPVertexData ExtendedData { get; set; }
    }
}