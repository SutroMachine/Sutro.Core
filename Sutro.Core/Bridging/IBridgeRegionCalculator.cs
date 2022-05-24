using g3;
using gs;
using System.Collections.Generic;
using System.Threading;

namespace Sutro.Core.Bridging
{
    public interface IBridgeRegionCalculator
    {
        /// <summary>
        /// Find the unsupported regions in each layer that can be bridged
        /// </summary>
        List<GeneralPolygon2d>[] CalculateBridgeRegions(PlanarSliceStack slices, CancellationToken cancellationToken);
    }
}