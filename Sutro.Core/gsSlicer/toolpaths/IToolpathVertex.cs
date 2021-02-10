using g3;

namespace gs
{
    public interface IToolpathVertex
    {
        Vector3d Position { get; set; }
        double FeedRate { get; set; }
        TPVertexData ExtendedData { get; set; }
    }

    public interface IExtrusionVertex : IToolpathVertex
    {
        public Vector3d Extrusion { get; set; }

        public Vector2d Dimensions { get; set; }
    }
}