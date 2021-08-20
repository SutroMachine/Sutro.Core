using g3;

namespace Sutro.Core.Support
{
    public class CircularSupportPointGenerator : ISupportPointGenerator
    {
        private readonly double defaultSupportPointDiameter;
        private readonly int supportPointSides;

        public CircularSupportPointGenerator(double defaultSupportPointDiameter, int supportPointSides)
        {
            this.defaultSupportPointDiameter = defaultSupportPointDiameter;
            this.supportPointSides = supportPointSides;
        }


        /// <summary>
        /// Generate a support point polygon (e.g. circle)
        /// </summary>
        public virtual GeneralPolygon2d MakeSupportPointPolygon(Vector2d point, double diameter = -1)
        {
            if (diameter <= 0)
                diameter = defaultSupportPointDiameter;

            var circle = Polygon2d.MakeCircle(
                diameter * 0.5, supportPointSides);

            circle.Translate(point);
            return new GeneralPolygon2d(circle);
        }
    }
}