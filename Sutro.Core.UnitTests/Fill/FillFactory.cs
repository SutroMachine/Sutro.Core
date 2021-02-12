using g3;
using Sutro.Core.Fill;

namespace Sutro.Core.UnitTests.Fill
{
    public static class FillFactory
    {
        public static FillLoop<FillSegment> CreateTriangleCCW()
        {
            return new FillLoop<FillSegment>(new Vector2d[] {
                new Vector2d(0, 0),
                new Vector2d(4, 0),
                new Vector2d(4, 3),
            });
        }

        public static FillLoop<FillSegment> CreateTriangleCW()
        {
            return new FillLoop<FillSegment>(new Vector2d[] {
                new Vector2d(4, 3),
                new Vector2d(4, 0),
                new Vector2d(0, 0),
            });
        }
    }
}