using g3;

namespace gs
{
    public interface ISupportPointGenerator
    {
        GeneralPolygon2d MakeSupportPointPolygon(Vector2d v, double diameter = -1);
    }
}