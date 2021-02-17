using g3;
using System.Collections.Generic;
using System.Threading;

namespace Sutro.Core.PartExteriors
{
    public class PartExteriorFullySolid : IPartExterior
    {
        public List<GeneralPolygon2d> GetExteriorRegions(int layerIndex,
            IReadOnlyCollection<GeneralPolygon2d> subject)
        {
            return new List<GeneralPolygon2d>(subject);
        }

        public void Initialize(CancellationToken cancel)
        { }
    }
}