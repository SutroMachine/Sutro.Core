using g3;
using gs;
using System.Collections.Generic;
using System.Threading;

namespace Sutro.Core.Bridging
{
    public class NullBridgeRegionCalculator : IBridgeRegionCalculator
    {
        public List<GeneralPolygon2d>[] CalculateBridgeRegions(PlanarSliceStack slices, CancellationToken cancellationToken)
        {
            var bridgeAreas = new List<GeneralPolygon2d>[slices.Count];
            for (int i = 0; i < bridgeAreas.Length; i++)
                bridgeAreas[i] = new List<GeneralPolygon2d>();
            return bridgeAreas;
        }
    }
}