using g3;

namespace gs
{
    public interface IGCodeListener
    {
        void Begin();

        void End();

        void BeginTravel();

        void BeginDeposition();

        void BeginCut();

        // for hacks
        void CustomCommand(int code, object o);

        void LinearMoveToAbsolute2d(LinearMoveData move);

        void LinearMoveToRelative2d(LinearMoveData move);

        void ArcToRelative2d(Vector2d pos, double radius, bool clockwise, double rate = 0);

        void LinearMoveToAbsolute3d(LinearMoveData move);

        void LinearMoveToRelative3d(LinearMoveData move);
    }
}