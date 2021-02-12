using g3;

namespace Sutro.Core.Toolpathing
{
    public interface IFillPolygon
    {
        GeneralPolygon2d Polygon { get; }

        bool Compute();
    }
}