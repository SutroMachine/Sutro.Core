using g3;

namespace Sutro.Core.Toolpaths
{
    public interface IExtrusionVertex : IToolpathVertex
    {
        public Vector3d Extrusion { get; set; }

        public Vector2d Dimensions { get; set; }
    }
}