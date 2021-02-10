using g3;
using System;

namespace gs
{
    public class TPVertexData
    {
        // information about the move *to* this vertex (ie the segment before it)
        public TPVertexFlags Flags = TPVertexFlags.None;

        // (optional) modifier functions that will be applied to this vertex
        // during gcode emission
        public Func<Vector3d, Vector3d> PositionF = null;

        public Func<double, double> FeedRateModifierF = null;
        public Func<Vector3d, Vector3d> ExtrusionModifierF = null;

        public TPVertexData()
        { }

        public TPVertexData(TPVertexData other)
        {
            Flags = other.Flags;
            PositionF = other.PositionF;
            FeedRateModifierF = other.FeedRateModifierF;
            ExtrusionModifierF = other.ExtrusionModifierF;
        }
    }
}