using g3;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sutro.Core.Generators
{
    public class FillRegions
    {
        public IReadOnlyList<GeneralPolygon2d> Solid { get; }

        public IReadOnlyList<GeneralPolygon2d> Sparse { get; }

        public FillRegions(IEnumerable<GeneralPolygon2d> solid, IEnumerable<GeneralPolygon2d> sparse)
        {
            Solid = new ReadOnlyCollection<GeneralPolygon2d>(new List<GeneralPolygon2d>(solid));
            Sparse = new ReadOnlyCollection<GeneralPolygon2d>(new List<GeneralPolygon2d>(sparse));
        }
    }
}