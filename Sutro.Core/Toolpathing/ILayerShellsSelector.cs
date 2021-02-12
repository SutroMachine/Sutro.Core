using g3;

namespace Sutro.Core.Toolpathing
{
    public interface ILayerShellsSelector
    {
        IShellsFillPolygon Next(Vector2d currentPosition);
    }
}