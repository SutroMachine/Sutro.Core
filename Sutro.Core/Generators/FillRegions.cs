using g3;
using System.Collections.Generic;

namespace Sutro.Core.Generators
{
    public class FillRegions
    {
        public readonly List<GeneralPolygon2d> Solid;
        public readonly List<GeneralPolygon2d> Sparse;

        public FillRegions(List<GeneralPolygon2d> solid, List<GeneralPolygon2d> sparse)
        {
            Solid = solid;
            Sparse = sparse;
        }
    }
}