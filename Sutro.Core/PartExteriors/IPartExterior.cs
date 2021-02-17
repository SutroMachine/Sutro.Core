using g3;
using System.Collections.Generic;
using System.Threading;

namespace Sutro.Core.PartExteriors
{
    public interface IPartExterior
    {
        void Initialize(CancellationToken? cancel);

        List<GeneralPolygon2d> GetExteriorRegions(int layerIndex,
            IReadOnlyCollection<GeneralPolygon2d> subject);
    }
}