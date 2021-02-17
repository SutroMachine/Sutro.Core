using g3;
using System.Collections.Generic;
using System.Threading;

namespace Sutro.Core.PartExteriors
{
    public class PartExteriorFullyHollow : IPartExterior
    {
        public List<GeneralPolygon2d> GetExteriorRegions(int layerIndex,
            IReadOnlyCollection<GeneralPolygon2d> subject)
        {
            return new List<GeneralPolygon2d>();
        }

        public void Initialize(CancellationToken? cancel)
        { }
    }
}