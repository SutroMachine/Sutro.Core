using g3;
using System;

namespace gs
{
    using LinearToolpath = LinearToolpath3<PrintVertex>;

    /// <summary>
    /// Convert a GCodeFile to a single huge ToolpathSet
    /// </summary>
    public class GCodeToToolpaths : IGCodeListener
    {
        public ToolpathSet PathSet;
        public IBuildLinearToolpath<PrintVertex> ActivePath;

        public Vector2d PathDimensions = GCodeUtil.UnspecifiedDimensions;

        public GCodeToToolpaths()
        {
        }

        private void push_active_path()
        {
            if (ActivePath != null && ActivePath.VertexCount > 0)
                PathSet.Append(ActivePath);
            ActivePath = null;
        }

        public virtual void Begin()
        {
            PathSet = new ToolpathSet();
            ActivePath = new LinearToolpath();
        }

        public virtual void End()
        {
            push_active_path();
        }

        public virtual void BeginTravel()
        {
            var newPath = new LinearToolpath();
            newPath.Type = ToolpathTypes.Travel;
            if (ActivePath != null && ActivePath.VertexCount > 0)
            {
                PrintVertex curp = new PrintVertex(ActivePath.End.Position, GCodeUtil.UnspecifiedValue, PathDimensions, GCodeUtil.UnspecifiedValue);
                newPath.AppendVertex(curp, TPVertexFlags.IsPathStart);
            }

            push_active_path();
            ActivePath = newPath;
        }

        public virtual void BeginDeposition()
        {
            var newPath = new LinearToolpath();
            newPath.Type = ToolpathTypes.Deposition;
            if (ActivePath != null && ActivePath.VertexCount > 0)
            {
                PrintVertex curp = new PrintVertex(ActivePath.End.Position, GCodeUtil.UnspecifiedValue, PathDimensions, GCodeUtil.UnspecifiedValue);
                newPath.AppendVertex(curp, TPVertexFlags.IsPathStart);
            }

            push_active_path();
            ActivePath = newPath;
        }

        public virtual void BeginCut()
        {
            var newPath = new LinearToolpath();
            newPath.Type = ToolpathTypes.Cut;
            if (ActivePath != null && ActivePath.VertexCount > 0)
            {
                PrintVertex curp = new PrintVertex(ActivePath.End.Position, GCodeUtil.UnspecifiedValue, PathDimensions, GCodeUtil.UnspecifiedValue);
                newPath.AppendVertex(curp, TPVertexFlags.IsPathStart);
            }

            push_active_path();
            ActivePath = newPath;
        }

        public virtual void LinearMoveToAbsolute3d(LinearMoveData move)
        {
            if (ActivePath == null)
                throw new Exception("GCodeToLayerPaths.LinearMoveToAbsolute3D: ActivePath is null!");

            // if we are doing a Z-move, convert to 3D path
            bool bZMove = (ActivePath.VertexCount > 0 && ActivePath.End.Position.z != move.position.z);
            if (bZMove)
                ActivePath.ChangeType(ToolpathTypes.PlaneChange);

            PrintVertex vtx = new PrintVertex(
                move.position, move.rate, PathDimensions, move.extrude.x);

            if (move.source != null)
                vtx.Source = move.source;

            ActivePath.AppendVertex(vtx, TPVertexFlags.None);
        }

        public virtual void CustomCommand(int code, object o)
        {
            if (code == (int)CustomListenerCommands.ResetExtruder)
            {
                push_active_path();
                PathSet.Append(new ResetExtruderPathHack());
            }
        }

        public virtual void LinearMoveToRelative3d(LinearMoveData move)
        {
            throw new NotImplementedException();
        }

        public virtual void LinearMoveToAbsolute2d(LinearMoveData move)
        {
            throw new NotImplementedException();
        }

        public virtual void LinearMoveToRelative2d(LinearMoveData move)
        {
            throw new NotImplementedException();
        }

        public virtual void ArcToRelative2d(Vector2d pos, double radius, bool clockwise, double rate = 0)
        {
            throw new NotImplementedException();
        }
    }
}