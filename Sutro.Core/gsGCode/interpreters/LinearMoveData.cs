using g3;
using Sutro.Core.Models.GCode;

namespace gs
{
    /// <summary>
    /// GCodeInterpreter passes this to GCodeListener for each G1 line
    /// </summary>
    public struct LinearMoveData
    {
        public Vector3d position;
        public double rate;
        public Vector3d extrude;
        public GCodeLine source;

        public LinearMoveData(Vector2d pos,
                              double rateIn = GCodeUtil.UnspecifiedValue)
        {
            position = new Vector3d(pos.x, pos.y, GCodeUtil.UnspecifiedValue);
            rate = rateIn;
            extrude = GCodeUtil.UnspecifiedPosition;
            source = null;
        }

        public LinearMoveData(Vector3d pos, double rateIn = GCodeUtil.UnspecifiedValue)
        {
            position = pos;
            rate = rateIn;
            extrude = GCodeUtil.UnspecifiedPosition;
            source = null;
        }

        public LinearMoveData(Vector3d pos, double rateIn, Vector3d extrudeIn)
        {
            position = pos;
            rate = rateIn;
            extrude = extrudeIn;
            source = null;
        }
    }
}