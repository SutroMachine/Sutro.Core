using g3;

namespace Sutro.Core.Support
{
    public interface ISupportPointGenerator
    {
        GeneralPolygon2d MakeSupportPointPolygon(Vector2d pt, double diameter = -1);
    }
}