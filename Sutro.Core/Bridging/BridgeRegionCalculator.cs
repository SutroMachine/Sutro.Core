using g3;
using gs;
using Sutro.Core.Parallel;
using System.Collections.Generic;
using System.Threading;

namespace Sutro.Core.Bridging
{
    public class BridgeRegionCalculator : IBridgeRegionCalculator
    {
        private readonly IParallelActor _parallelActor;

        private readonly double _maxBridgeDistance;
        private readonly double _bridgeTolerance;
        private readonly double _extraExpansion;
        private readonly double _minArea;

        public BridgeRegionCalculator(IParallelActor parallelActor, double maxBridgeDistance,
            double bridgeTolerance, double minArea, double extraExpansion)
        {
            _parallelActor = parallelActor;
            _maxBridgeDistance = maxBridgeDistance;
            _bridgeTolerance = bridgeTolerance;
            _minArea = minArea;
            _extraExpansion = extraExpansion;
        }

        public virtual List<GeneralPolygon2d>[] CalculateBridgeRegions(PlanarSliceStack slices,
            CancellationToken cancellationToken)
        {
            int nLayers = slices.Count;

            var bridgeAreas = new List<GeneralPolygon2d>[nLayers];

            if (nLayers <= 1)
                return bridgeAreas;

            _parallelActor.ForEach(Interval1i.Range(nLayers - 1),
                new System.Threading.Tasks.ParallelOptions() { CancellationToken = cancellationToken },
                (layeri) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                PlanarSlice slice = slices[layeri];
                PlanarSlice next_slice = slices[layeri + 1];

                // To find bridgeable regions, we compute all floating regions in next layer.
                // Then we look for polys that are bridgeable, ie thing enough and fully anchored.
                List<GeneralPolygon2d> bridgePolys = null;

                // [RMS] bridge area is (next_solids - solids). However, for meshes with slight variations
                // in identical stacked polygons (eg like created from mesh extrusions), there will be thousands
                // of tiny polygons. We can filter them, but just computing them can take an enormous amount of time.
                // So, we slightly offset the slice here. This means the bridge poly will be slightly under-sized,
                // the assumption is we will be adding extra overlap anyway
                List<GeneralPolygon2d> expandPolys = ClipperUtil.MiterOffset(slice.Solids, _extraExpansion, _minArea);
                bridgePolys = ClipperUtil.Difference(next_slice.Solids, expandPolys, _minArea);
                bridgePolys = CurveUtils2.FilterDegenerate(bridgePolys, _minArea);
                bridgePolys = CurveUtils2.Filter(bridgePolys, (p) =>
                {
                    return layeri > 0 && IsBridgeable(slices[layeri], p, _bridgeTolerance);
                });

                bridgeAreas[layeri] = bridgePolys != null
                    ? bridgePolys : new List<GeneralPolygon2d>();
            });

            bridgeAreas[nLayers - 1] = new List<GeneralPolygon2d>();

            return bridgeAreas;
        }

        /// <summary>
        /// Check if polygon can be bridged. Currently we allow this if all hold:
        /// 1) contracting by max bridge width produces empty polygon
        /// 2) all "turning" vertices of polygon are connected to previous layer
        /// [TODO] not sure this actually guarantees that unsupported distances
        /// *between* turns are within bridge threshold...
        /// </summary>
        protected virtual bool IsBridgeable(PlanarSlice slice, GeneralPolygon2d supportPoly, double fTolDelta)
        {
            // if we inset by half bridge dist, and this doesn't completely wipe out
            // polygon, then it is too wide to bridge, somewhere
            // [TODO] this is a reasonable way to decompose into bridgeable chunks...
            double insetDelta = _maxBridgeDistance * 0.55;
            List<GeneralPolygon2d> offset = ClipperUtil.MiterOffset(supportPoly, -insetDelta);
            if (offset != null && offset.Count > 0)
                return false;

            if (!PolyIsFullyConnected(slice, supportPoly.Outer, fTolDelta))
                return false;

            foreach (var h in supportPoly.Holes)
            {
                if (!PolyIsFullyConnected(slice, h, fTolDelta))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// check if all turn vertices of poly are connected ( see is_connected(vector2d) )
        /// </summary>
        protected virtual bool PolyIsFullyConnected(PlanarSlice slice, Polygon2d poly, double fTolDelta)
        {
            int NV = poly.VertexCount;
            for (int k = 0; k < NV; ++k)
            {
                if (k > 0 && poly.OpeningAngleDeg(k) > 179)
                    continue;
                if (!VertexIsConnected(slice, poly[k], fTolDelta))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check if position is "connected" to a solid in the slice
        /// at layer i, where connected means distance is within tolerance
        /// [TODO] I don't think this will return true if pos is inside one of the solids...
        /// </summary>
        protected virtual bool VertexIsConnected(PlanarSlice slice, Vector2d pos, double fTolDelta)
        {
            double maxdist_sqr = fTolDelta * fTolDelta;

            double dist_sqr = slice.DistanceSquared(pos, maxdist_sqr, true, true);
            if (dist_sqr < maxdist_sqr)
                return true;

            return false;
        }
    }
}