using g3;

namespace Sutro.Core.Toolpaths
{
    public class PrintVertex : IExtrusionVertex
    {
        public Vector3d Position { get; set; }
        public double FeedRate { get; set; }
        public TPVertexData ExtendedData { get; set; }

        /// <summary> Dimensions of extrusion at this vertex. width=x, height=y. </summary>
        public Vector2d Dimensions { get; set; }

        /// <summary> Position of up to three extruders at this vertex. </summary>
        public Vector3d Extrusion { get; set; }

        public object Source { get; set; }

        public PrintVertex(Vector3d pos, double rate, Vector2d dimensions)
        {
            Position = pos;
            FeedRate = rate;
            Dimensions = dimensions;
            Extrusion = Vector3d.Zero;
            ExtendedData = null;
            Source = null;
        }

        public PrintVertex(Vector3d pos, double rate, Vector2d dimensions, double ExtruderA)
        {
            Position = pos;
            FeedRate = rate;
            Dimensions = dimensions;
            Extrusion = new Vector3d(ExtruderA, 0, 0);
            ExtendedData = null;
            Source = null;
        }

        public PrintVertex(PrintVertex other)
        {
            Position = new Vector3d(other.Position);
            FeedRate = other.FeedRate;
            Dimensions = new Vector2d(other.Dimensions);
            Extrusion = new Vector3d(other.Extrusion);
            ExtendedData = other.ExtendedData == null ? null : new TPVertexData(other.ExtendedData);
            Source = other.Source;
        }

        public static implicit operator Vector3d(PrintVertex v)
        {
            return v.Position;
        }
    };
}