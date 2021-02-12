using g3;

namespace gs
{
    public interface ILayerShellsSelector
    {
        IShellsFillPolygon Next(Vector2d currentPosition);
    }
}